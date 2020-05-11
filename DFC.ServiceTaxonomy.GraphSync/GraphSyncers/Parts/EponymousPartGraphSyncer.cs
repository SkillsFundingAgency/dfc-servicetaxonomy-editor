using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
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
    public class EponymousPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IEnumerable<IContentFieldGraphSyncer> _contentFieldGraphSyncer;
        private readonly ILogger<EponymousPartGraphSyncer> _logger;

        public EponymousPartGraphSyncer(
            IEnumerable<IContentFieldGraphSyncer> contentFieldGraphSyncer,
            ILogger<EponymousPartGraphSyncer> logger)
        {
            _contentFieldGraphSyncer = contentFieldGraphSyncer;
            _logger = logger;
        }

        //todo: might be better to call it EponymousPart and check for that, rather than null
        /// <summary>
        /// null is a special case to indicate a match when the part is the eponymous named content type part
        /// </summary>
        public string? PartName => null;

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            foreach (var contentFieldGraphSyncer in _contentFieldGraphSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    contentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldGraphSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = content[contentPartFieldDefinition.Name];
                    if (contentItemField == null)
                        continue;

                    //todo: might need another level of indirection to be able to test this method :*(
                    IContentPartFieldDefinition contentPartFieldDefinitionWrapper
                        = new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition);

                    await contentFieldGraphSyncer.AddSyncComponents(
                        contentItemField,
                        mergeNodeCommand,
                        replaceRelationshipsCommand,
                        contentPartFieldDefinitionWrapper,
                        graphSyncHelper);
                }
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            foreach (var contentFieldGraphSyncer in _contentFieldGraphSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    contentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldGraphSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = content[contentPartFieldDefinition.Name] as JObject;
                    if (contentItemField == null)
                    {
                        _logger.LogWarning($"Found unexpected content field. Most likely GetJobProfiles importer has generated a badly formed content item: {content}");
                        continue;
                    }

                    IContentPartFieldDefinition contentPartFieldDefinitionWrapper
                        = new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition);

                    (bool validated, string failureReason) = await contentFieldGraphSyncer.ValidateSyncComponent(
                        contentItemField,
                        contentPartFieldDefinitionWrapper,
                        nodeWithOutgoingRelationships,
                        graphSyncHelper,
                        graphValidationHelper,
                        expectedRelationshipCounts);

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
