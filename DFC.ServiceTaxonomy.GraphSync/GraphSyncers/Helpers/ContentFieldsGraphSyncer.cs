using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    /// <remarks>
    /// we map from Orchard Core's types to Neo4j's driver types (which map to cypher type)
    /// we might also want to map to rdf types here (accept flag to say store with type?)
    /// will be useful if we import into neo using keepCustomDataTypes
    /// we can append the datatype to the value, i.e. value^^datatype
    /// see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types
    ///
    /// Type mappings
    /// -------------
    /// OC UI Field Type | OC Content | Neo Driver    | Cypher     | NSMNTX postfix | RDF             | Notes
    /// Boolean            ?            see notes       Boolean                       xsd:boolean       neo docs say driver is boolean. do they mean Boolean or bool?
    /// Content Picker                                                                                  creates relationships
    /// Date
    /// Date Time
    /// Html               Html         string          String
    /// Link               Url+Text     string+string   String+String
    /// Markdown
    /// Media
    /// Numeric            Value        long            Integer                       xsd:integer       \ OC always present numeric as floats. we check the fields scale to decide whether to store an int or a float
    /// Numeric            Value        float           Float                                           / (RDF supports xsd:int & xsd:integer, are they different or synonyms)
    /// Text               Text         string          String                        xsd:string        no need to specify in RDF - default?
    /// Time
    /// Youtube
    ///
    /// see
    /// https://github.com/neo4j/neo4j-dotnet-driver
    /// https://www.w3.org/2011/rdf-wg/wiki/XSD_Datatypes
    /// https://neo4j.com/docs/labs/nsmntx/current/import/
    /// </remarks>
    public class ContentFieldsGraphSyncer : IContentFieldsGraphSyncer
    {
        private readonly IEnumerable<IContentFieldGraphSyncer> _contentFieldGraphSyncer;
        private readonly ILogger<ContentFieldsGraphSyncer> _logger;

        public ContentFieldsGraphSyncer(
            IEnumerable<IContentFieldGraphSyncer> contentFieldGraphSyncer,
            ILogger<ContentFieldsGraphSyncer> logger)
        {
            _contentFieldGraphSyncer = contentFieldGraphSyncer;
            _logger = logger;
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            foreach (var contentFieldGraphSyncer in _contentFieldGraphSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    context.ContentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldGraphSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = (JObject?)content[contentPartFieldDefinition.Name];
                    if (contentItemField == null)
                        continue;

                    context.SetContentPartFieldDefinition(contentPartFieldDefinition);

                    await contentFieldGraphSyncer.AddSyncComponents(contentItemField, context);
                }

                context.SetContentPartFieldDefinition(default);
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ValidateAndRepairContext context)
        {
            foreach (var contentFieldGraphSyncer in _contentFieldGraphSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    context.ContentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldGraphSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = content[contentPartFieldDefinition.Name] as JObject;
                    if (contentItemField == null)
                    {
                        _logger.LogWarning($"Found unexpected content for {contentPartFieldDefinition.Name} field. Content: {content}");
                        continue;
                    }

                    context.SetContentPartFieldDefinition(contentPartFieldDefinition);

                    (bool validated, string failureReason) = await contentFieldGraphSyncer.ValidateSyncComponent(
                        contentItemField, context);

                    if (!validated)
                    {
                        return (false, $"{contentPartFieldDefinition.Name} {contentFieldGraphSyncer.FieldTypeName} did not validate: {failureReason}");
                    }
                }
            }

            return (true,"");
        }
    }
}
