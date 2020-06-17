using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.FlowPart
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
