using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    public class GraphSyncSettings
    {
        public GraphSyncSettings(string name, string bagPartContentItemRelationshipType, string nodeNameTransform, string createRelationshipType, string idPropertyName, string generateIDValue)
        {
            Name = name;
            BagPartContentItemRelationshipType = bagPartContentItemRelationshipType;
            NodeNameTransform = nodeNameTransform;
            CreateRelationshipType = createRelationshipType;
            IDPropertyName = idPropertyName;
            GenerateIDValue = generateIDValue;
        }

        public string Name { get; set; }
        public string BagPartContentItemRelationshipType { get; set; }
        public string NodeNameTransform { get; set; }
        public string CreateRelationshipType { get; set; }
        public string IDPropertyName { get; set; }
        public string GenerateIDValue { get; set; }

    }
}
