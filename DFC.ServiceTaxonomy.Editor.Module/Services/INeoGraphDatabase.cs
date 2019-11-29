using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    public interface INeoGraphDatabase
    {
        Statement MergeNodeStatement(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri");

        Statement MergeRelationshipsStatement(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>
                relationships);

        /// <summary>
        /// Run statements, in order, within a write transaction. No results returned.
        /// </summary>
        Task RunWriteStatements(params Statement[] statements);
    }
}
