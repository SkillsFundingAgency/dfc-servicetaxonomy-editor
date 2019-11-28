using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    //todo: use this for unit tests https://www.nuget.org/packages/xunit.analyzers/

    // https://github.com/neo4j/neo4j-dotnet-driver
    // (not updated for 4 yet: https://neo4j.com/docs/driver-manual/1.7/get-started/)
    public class NeoGraphDatabase : INeoGraphDatabase, IDisposable
    {
        private readonly IDriver _driver;

        public NeoGraphDatabase(IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions)
        {
            // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
            // It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
            //  driver is thread-safe, while the session or the transaction is not thread-safe.
            //todo: add configuration/settings menu item/page so user can enter this
            var neo4jConfiguration = neo4jConfigurationOptions.CurrentValue;

            _driver = GraphDatabase.Driver(neo4jConfiguration.Endpoint.Uri, AuthTokens.Basic(neo4jConfiguration.Endpoint.Username, neo4jConfiguration.Endpoint.Password));
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
            var parameters = new Dictionary<string, object> {{sourceIdPropertyValueParamName, sourceIdPropertyValue}};
            int destOrdinal = 0;
            //todo: better name relationship=> relationships, relationships=>?
            foreach (var relationship in relationships)
            {
                foreach (string destIdPropertyValue in relationship.Value)
                {
                    string destNodeVariable = $"d{++destOrdinal}";
                    string destIdPropertyValueParamName = $"{destNodeVariable}Value";
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
            IAsyncSession session = _driver.AsyncSession();
            try
            {
                IStatementResultCursor cursor = await session.RunAsync(statement);
                //IResultSummary resultSummary =
                    await cursor.ConsumeAsync();

                //var x = resultSummary.ResultAvailableAfter;
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
