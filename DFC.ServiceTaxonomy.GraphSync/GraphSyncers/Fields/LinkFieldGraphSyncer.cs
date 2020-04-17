using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class LinkFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "LinkField";

        private const string UrlFieldKey = "Url", TextFieldKey = "Text";
        private const string LinkUrlPostfix = "_url", LinkTextPostfix = "_text";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            string basePropertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);

            JValue? value = (JValue?)contentItemField[UrlFieldKey];
            if (value != null && value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{LinkUrlPostfix}", value.As<string>());

            value = (JValue?)contentItemField[TextFieldKey];
            if (value != null && value.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add($"{basePropertyName}{LinkTextPostfix}", value.As<string>());
        }

        public async Task<(bool verified, string failureReason)> VerifySyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            string nodeBasePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            string nodeUrlPropertyName = $"{nodeBasePropertyName}{LinkUrlPostfix}";

            (bool matched, string failureReason) = graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                UrlFieldKey,
                contentItemField,
                nodeUrlPropertyName,
                sourceNode);

            if (!matched)
                return (false, $"url did not verify: {failureReason}");

            string nodeTextPropertyName = $"{nodeBasePropertyName}{LinkTextPostfix}";

            (matched, failureReason) = graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TextFieldKey,
                contentItemField,
                nodeTextPropertyName,
                sourceNode);

            return (matched, matched ? "" : $"text did not verify: {failureReason}");
        }
    }
}
