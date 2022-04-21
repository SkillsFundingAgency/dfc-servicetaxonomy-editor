using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Orchestrators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers
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
    public class ContentFieldsDataSyncer : IContentFieldsDataSyncer
    {
        private readonly IEnumerable<IContentFieldDataSyncer> _contentFieldDataSyncer;
        private readonly ILogger<ContentFieldsDataSyncer> _logger;

        public ContentFieldsDataSyncer(
            IEnumerable<IContentFieldDataSyncer> contentFieldDataSyncer,
            ILogger<ContentFieldsDataSyncer> logger)
        {
            _contentFieldDataSyncer = contentFieldDataSyncer;
            _logger = logger;
        }

        public async Task AddRelationship(JObject content, IDescribeRelationshipsContext context)
        {
            foreach (var contentFieldDataSyncer in _contentFieldDataSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    context.ContentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldDataSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = (JObject?)content[contentPartFieldDefinition.Name];
                    if (contentItemField == null)
                        continue;

                    context.SetContentPartFieldDefinition(contentPartFieldDefinition);

                    await contentFieldDataSyncer.AddRelationship(contentItemField, context);
                }

                context.SetContentPartFieldDefinition(default);
            }
        }

        //todo: share code with above
        public async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            foreach (var contentFieldDataSyncer in _contentFieldDataSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    context.ContentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldDataSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    context.SetContentPartFieldDefinition(contentPartFieldDefinition);

                    // if we're syncing after field has been detached from the part, don't sync it
                    if (contentPartFieldDefinition.Settings["ContentPartFieldSettings"]?
                        [ContentTypeOrchestrator.ZombieFlag]?.Value<bool>() == true)
                    {
                        await contentFieldDataSyncer.AddSyncComponentsDetaching(context);
                    }
                    else
                    {
                        JObject? contentItemField = content[contentPartFieldDefinition.Name] as JObject;
                        if (contentItemField == null)
                        {
                            _logger.LogWarning("The '{ContentItem}' {ContentType} is missing content for the {FieldName} {FieldType}.",
                                context.ContentItem.DisplayText,
                                context.ContentItem.ContentType,
                                contentPartFieldDefinition.Name,
                                contentPartFieldDefinition.FieldDefinition.Name);
                            continue;
                        }

                        await contentFieldDataSyncer.AddSyncComponents(contentItemField, context);
                    }
                }

                context.SetContentPartFieldDefinition(default);
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            foreach (var contentFieldDataSyncer in _contentFieldDataSyncer)
            {
                IEnumerable<ContentPartFieldDefinition> contentPartFieldDefinitions =
                    context.ContentTypePartDefinition.PartDefinition.Fields
                        .Where(fd => fd.FieldDefinition.Name == contentFieldDataSyncer.FieldTypeName);

                foreach (ContentPartFieldDefinition contentPartFieldDefinition in contentPartFieldDefinitions)
                {
                    JObject? contentItemField = content[contentPartFieldDefinition.Name] as JObject;
                    if (contentItemField == null)
                    {
                        _logger.LogWarning("Found unexpected content for {ContentPartFieldDefinitionName} field. Content: {Content}",
                            contentPartFieldDefinition.Name, content);
                        continue;
                    }

                    context.SetContentPartFieldDefinition(contentPartFieldDefinition);

                    (bool validated, string failureReason) = await contentFieldDataSyncer.ValidateSyncComponent(
                        contentItemField, context);

                    if (!validated)
                    {
                        return (false, $"{contentPartFieldDefinition.Name} {contentFieldDataSyncer.FieldTypeName} did not validate: {failureReason}");
                    }
                }
            }

            return (true, "");
        }
    }
}
