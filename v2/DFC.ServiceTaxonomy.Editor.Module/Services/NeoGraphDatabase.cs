using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    // https://github.com/neo4j/neo4j-dotnet-driver
    // (not updated for 4 yet: https://neo4j.com/docs/driver-manual/1.7/get-started/)
    public class NeoGraphDatabase : INeoGraphDatabase, IDisposable
    {
        private readonly IDriver _driver;
        // we'd have to globally add the namespace prefix to neo if we wanted to export to RDF
//        private const string NcsPrefix = "http://nationalcareers.service.gov.uk/taxonomy#";
        private const string NcsPrefix = "ncs__";

        public NeoGraphDatabase()
        {
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "ESCO3"));
        }

        public async Task MergeNode(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri")
        {
            await RunStatement(new Statement(
                $"MERGE (n:{nodeLabel} {{ {idPropertyName}:'{propertyMap[idPropertyName]}' }}) SET n=$properties RETURN n",
                new Dictionary<string,object> {{"properties", propertyMap}}));
        }
 
        public Task MergeRelationships(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue, IDictionary<(string destNodeLabel,string destIdPropertyName,string relationshipType),IEnumerable<string>> relationships)
        {
            //todo: for same task for create/edit, first delete all given relationships between source and dest nodes
            //todo: bi-directional relationships
            //todo: rewrite for elegance/perf. selectmany?
            const string sourceIdPropertyValueParamName = "sourceIdPropertyValue";
            var matchBuilder = new StringBuilder($"match (s:{sourceNodeLabel} {{{sourceIdPropertyName}:${sourceIdPropertyValueParamName}}})");
            var mergeBuilder = new StringBuilder();
            var parameters = new Dictionary<string, Object> {{sourceIdPropertyValueParamName, sourceIdPropertyValue}};
            int destOrdinal = 0;
            //todo: better name relationship=> relationships, relationships=>? 
            foreach (var relationship in relationships)
            {
                foreach (var destIdPropertyValue in relationship.Value)
                {
                    var destNodeVariable = $"d{++destOrdinal}";
                    var destIdPropertyValueParamName = $"{destNodeVariable}Value";
                    matchBuilder.Append($", ({destNodeVariable}:{relationship.Key.destNodeLabel} {{{relationship.Key.destIdPropertyName}:${destIdPropertyValueParamName}}})");
                    parameters.Add(destIdPropertyValueParamName, destIdPropertyValue);
                    mergeBuilder.Append($"\r\nmerge (s)-[:{relationship.Key.relationshipType}]->({destNodeVariable})");
                }
            }

            //todo: return?
            return RunStatement(new Statement($"{matchBuilder}\r\n{mergeBuilder} return s", parameters));
        }

        private async Task RunStatement(Statement statement)
        {
            var session = _driver.AsyncSession();
            try
            {
                IStatementResultCursor cursor = await session.RunAsync(statement);
                var resultSummary = await cursor.ConsumeAsync();

                var x = resultSummary.ResultAvailableAfter;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}