using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class LinkFieldGraphSyncer : FieldGraphSyncer, IContentFieldGraphSyncer
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

        public async Task<bool> VerifySyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            string nodeBasePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            string nodeUrlPropertyName = $"{nodeBasePropertyName}{LinkUrlPostfix}";

            if (!StringContentPropertyMatchesNodeProperty(
                UrlFieldKey,
                contentItemField,
                nodeUrlPropertyName,
                sourceNode))
            {
                return false;
            }

            string nodeTextPropertyName = $"{nodeBasePropertyName}{LinkTextPostfix}";

            return StringContentPropertyMatchesNodeProperty(
                TextFieldKey,
                contentItemField,
                nodeTextPropertyName,
                sourceNode);
        }
    }
}
