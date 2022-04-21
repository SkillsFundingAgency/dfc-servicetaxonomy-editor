using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Services.Interface;
using Newtonsoft.Json.Linq;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public class TitlePartDataSyncer : ContentPartDataSyncer
    {
        private readonly ITitlePartCloneGenerator _titlePartCloneGenerator;

        public TitlePartDataSyncer(ITitlePartCloneGenerator titlePartCloneGenerator)
        {
            _titlePartCloneGenerator = titlePartCloneGenerator;
        }

        public override string PartName => nameof(TitlePart);

        private const string _contentTitlePropertyName = "Title";

        //todo: configurable??
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            string? title = context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);
            if (title == null)
                context.MergeNodeCommand.Properties.Add(NodeTitlePropertyName, context.ContentItem.DisplayText);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
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
