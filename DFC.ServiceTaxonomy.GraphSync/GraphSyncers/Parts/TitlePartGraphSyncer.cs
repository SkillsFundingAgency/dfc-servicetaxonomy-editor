using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Newtonsoft.Json.Linq;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class TitlePartGraphSyncer : ContentPartGraphSyncer
    {
        private readonly ITitlePartCloneGenerator _titlePartCloneGenerator;

        public TitlePartGraphSyncer(ITitlePartCloneGenerator titlePartCloneGenerator)
        {
            _titlePartCloneGenerator = titlePartCloneGenerator;
        }

        public override string PartName => nameof(TitlePart);

        private const string _contentTitlePropertyName = "Title";

        //todo: configurable??
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public const string UniqueTitlePartNodeLabel = "uniquetitle_Title";

        public override Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            string? title = context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);
            if (title == null)
                context.MergeNodeCommand.Properties[NodeTitlePropertyName] = context.ContentItem.DisplayText;

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            var sourceNode = context.NodeWithRelationships.SourceNode!;
            if (content[_contentTitlePropertyName]?.Type == JTokenType.Null && sourceNode.Properties.ContainsKey(UniqueTitlePartNodeLabel))
            {
                return Task.FromResult((true, string.Empty));
            }
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithRelationships.SourceNode!));
        }

        public override Task MutateOnClone(JObject content, ICloneContext context)
        {
            string title = (string?)content[nameof(TitlePart.Title)] ?? string.Empty;

            var newTitle = _titlePartCloneGenerator.Generate(title);

            context.ContentItem.DisplayText = newTitle;
            content[nameof(TitlePart.Title)] = newTitle;

            return Task.CompletedTask;
        }
    }
}
