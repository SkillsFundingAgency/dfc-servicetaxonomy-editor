using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: better name to reflect only for PropertyFieldGraphSyncer??
    //todo: inheritance/composition/extension??
    public class FieldGraphSyncer
    {
        //todo: better name
        //todo: better distinguish between fieldTYPEname and fieldname
        //todo: log validation failed reasons
        protected bool StringContentPropertyMatchesNodeProperty(
            string contentKey,
            JObject contentItemField,
            string nodePropertyName,
            INode sourceNode)
        {
            //string nodePropertyName = await graphSyncHelper.PropertyName(fieldName); //contentPartFieldDefinition.Name);
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
