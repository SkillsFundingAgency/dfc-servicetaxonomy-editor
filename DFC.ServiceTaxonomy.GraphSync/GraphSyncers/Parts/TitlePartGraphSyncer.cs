using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class TitlePartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(TitlePart);

        private const string _contentTitlePropertyName = "Title";

        //todo: configurable??
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            string? title = context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);
            if (title == null)
                context.MergeNodeCommand.Properties.Add(NodeTitlePropertyName, context.ContentItem.DisplayText);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode));
        }
    }
}
