using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow
{
    public class FlowPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, IFlowPartEmbeddedContentItemsGraphSyncer
    {
        private const string Ordinal = "Ordinal";
        private const string Alignment = "Alignment";
        private const string Size = "Size";
        private const string FlowMetaData = "FlowMetadata";

        public FlowPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(
            ContentItem contentItem,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget")
                .Select(t => t.Name);
        }

        protected override async Task<Dictionary<string, object>?> GetRelationshipProperties(
            ContentItem contentItem,
            int ordinal,
            IGraphSyncHelper graphSyncHelper)
        {
            // set the FlowMetaData as the relationship's properties

            var flowMetaData = new Dictionary<string, object>
            {
                {Ordinal, (long)ordinal}
            };

            //todo: do we need more config/method for RelationshipPropertyName (and rename existing NodePropertyName?)
            //todo: handle nulls?

            JObject flowMetaDataContent = (JObject)contentItem.Content[FlowMetaData]!;

            FlowAlignment alignment = (FlowAlignment)(int)flowMetaDataContent[Alignment]!;
            flowMetaData.Add(await graphSyncHelper!.PropertyName(Alignment), alignment.ToString());

            flowMetaData.Add(await graphSyncHelper!.PropertyName(Size), (long)flowMetaDataContent[Size]!);

            return flowMetaData;
        }
    }
}
