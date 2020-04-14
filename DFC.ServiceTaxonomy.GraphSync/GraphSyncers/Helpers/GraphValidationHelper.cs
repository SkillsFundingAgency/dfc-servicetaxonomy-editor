using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    //todo: dreadful name :*)
    // could move contents into graphsynchelper, although it doesn't _really_ belong there
    public class GraphValidationHelper : IGraphValidationHelper
    {
        // could be static, but better for unit testing like this
        //todo: better name
        //todo: better distinguish between fieldTYPEname and fieldname
        //todo: log validation failed reasons
        public bool StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            sourceNode.Properties.TryGetValue(nodePropertyName, out object? nodePropertyValue);

            JValue? contentItemFieldValue = (JValue?)contentItemField?[contentKey];
            if (contentItemFieldValue == null || contentItemFieldValue.Type == JTokenType.Null)
            {
                return nodePropertyValue == null;
            }

            if (nodePropertyValue == null)
            {
                return false;
            }

            return contentItemFieldValue.As<string>() == (string)nodePropertyValue;
        }
    }
}
