using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Html.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class HtmlBodyPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(HtmlBodyPart);

        private static readonly Func<string, string> _htmlBodyFieldsPropertyNameTransform = n => $"htmlbody_{n}";
        private const string HtmlPropertyName = "Html";

        public async Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = graphSyncHelper.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            JValue? htmlValue = (JValue?)content[HtmlPropertyName];
            if (htmlValue != null && htmlValue.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(HtmlPropertyName), htmlValue.As<string>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            // prefix field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = graphSyncHelper.PushPropertyNameTransform(_htmlBodyFieldsPropertyNameTransform);

            return graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                HtmlPropertyName,
                content,
                await graphSyncHelper!.PropertyName(HtmlPropertyName),
                nodeWithOutgoingRelationships.SourceNode);
        }
    }
#pragma warning restore S1481
}
