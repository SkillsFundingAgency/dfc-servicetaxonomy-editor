using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : ContentPartGraphSyncer, IGraphSyncPartGraphSyncer
    {
        private readonly IPreExistingContentItemVersion _preExistingContentItemVersion;

        public GraphSyncPartGraphSyncer(IPreExistingContentItemVersion preExistingContentItemVersion)
        {
            _preExistingContentItemVersion = preExistingContentItemVersion;
        }

        public override int Priority { get => int.MaxValue; }
        public override string PartName => nameof(GraphSyncPart);

        public override Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            object? idValue = context.SyncNameProvider.GetIdPropertyValue(content, context.ContentItemVersion);
            if (idValue != null)
            {
                // id is added as a special case as part of SyncAllowed,
                // so we allow an overwrite, which will occur as part of syncing
                //todo: something cleaner
                context.MergeNodeCommand.Properties[context.SyncNameProvider.IdPropertyName()] = idValue;
                //context.MergeNodeCommand.Properties.Add(context.SyncNameProvider.IdPropertyName(), idValue);
            }

            return Task.CompletedTask;
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            string newIdPropertyValue = await context.SyncNameProvider.GenerateIdPropertyValue(context.ContentItem.ContentType);

            //todo: which is the best way? if we want to use Alter, we'd have to pass the part, rather than content
            // (so that named parts work, where >1 type of part is in a content type)

            content[nameof(GraphSyncPart.Text)] = newIdPropertyValue;
            //context.ContentItem.Alter<GraphSyncPart>(p => p.Text = newIdPropertyValue);
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            var syncSettings = context.SyncNameProvider.GetGraphSyncPartSettings(context.ContentItem.ContentType);

            if (syncSettings.PreexistingNode)
            {
                _preExistingContentItemVersion.SetContentApiBaseUrl(syncSettings.PreExistingNodeUriPrefix);
            }

            return Task.FromResult(context.GraphValidationHelper.ContentPropertyMatchesNodeProperty(
                context.SyncNameProvider.ContentIdPropertyName,
                content,
                context.SyncNameProvider.IdPropertyName(),
                context.NodeWithOutgoingRelationships.SourceNode,
                (contentValue, nodeValue) =>
                    nodeValue is string nodeValueString
                    && Equals((string)contentValue!,
                        context.SyncNameProvider.IdPropertyValueFromNodeValue(nodeValueString, syncSettings.PreexistingNode ? _preExistingContentItemVersion : context.ContentItemVersion))));
        }
    }
}
