using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    //https://neo4j.com/docs/cypher-manual/current/syntax/values/
    public enum NeoPropertyType
    {
        Number,
        String
    }
    
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
            //todo: one driver per application : singleton.
//            There are a few points that need to be highlighted when adding this driver into your project:
//
//            Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
//                It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
//                The driver is thread-safe, while the session or the transaction is not thread-safe.
//todo: add configuration/settings menu item/page so user can enter this
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "ESCO3"));
        }

        public async Task MergeNode(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri")
        {
            await RunStatement(new Statement(
                $"MERGE (n:{nodeLabel} {{ {idPropertyName}:'{propertyMap[idPropertyName]}' }}) SET n=$properties RETURN n",
                new Dictionary<string,object> {{"properties", propertyMap}}));
        }
        
        //todo: object value (jtoken -> object)
        // public async Task MergeNode(string nodeLabel, IDictionary<string, object> properties) // todo: string idPropertyName
        // {
        //     var parameterisedPropertiesFragment = string.Join(',', properties.Select(p => $"{NcsPrefix}{p.Key}: ${p.Key}"));
        //     // param could match namespaced version
        //     //var parameterisedPropertiesFragment = string.Join(',', propertyDictionary.Select(kv => $"{kv.Key}: ${p.Name}"));
        //     var session = _driver.AsyncSession();
        //     try
        //     {
        //         //todo: use merge with set, matching node on uri/id
        //         // MERGE (n: {uri: ''}) SET n.prop1 = x, n.prop2 etc.
        //         var statement = new Statement($"MERGE (n:{NcsPrefix}{nodeLabel} {{ {parameterisedPropertiesFragment} }}) RETURN n", properties);
        //         
        //         IStatementResultCursor cursor = await session.RunAsync(statement);
        //         var resultSummary = await cursor.ConsumeAsync();
        //
        //         var x = resultSummary.ResultAvailableAfter;
        //     }
        //     finally
        //     {
        //         await session.CloseAsync();
        //     }
        // }

        // public Task MergeRelationships(string sourceNodeLabel,
        //     string sourceIdPropertyValue, string destinationIdPropertyValue,
        //     string relationshipType,
        //     string sourceIdPropertyName = "uri", string destinationIdPropertyName = "uri")
        // {
        //     throw new NotImplementedException();
        // }

        
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
            
            
//             $"match (src:{sourceNodeLabel} {{{sourceIdPropertyName}:$sourceIdPropertyValue}}), ";
//             var statementString = $@"match (src:{sourceNodeLabel} {{{sourceIdPropertyName}:$sourceIdPropertyValue}}), (a:ncs__Activity {uri:'auri'}), (w:ncs__wank) 
// merge (jp)-[r:hasActivity]->(a)
// merge (jp)-[r2:hasSumfink]->(a)
// merge (jp)-[r3:hasTit]->(w)
// return jp, r, w, r3, r2, a, type(r), type(r2)";
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

        
        //todo: dictionary of properties
//        public async Task MergeNode(string nodeLabel, string propertyName, object propertyValue) //, NeoPropertyType propertyType) //enum would be better
//        {
//            //todo: transaction, parameterisation/construct Statement
//            var session = _driver.AsyncSession(); // <v4 does not support multiple databases : o => o.WithDatabase("ESCO3")); //withDatabase("neo4j"));
//            try
//            {
//                // https://neo4j.com/docs/cypher-manual/current/clauses/merge/
//                // doesn't support partial matches - would be an issue if we add a field to a content type
////                IStatementResultCursor cursor = await session.RunAsync($"MERGE (n:{NcsPrefix}{nodeLabel} {{ {propertyName}: {PropertyToCypherLiteral(propertyValue, propertyType)} }}) RETURN n");
//                // looks like node labels can't be parameterised, nor property names
//                //todo: all properties in single merge statement
//                
//                var statement = new Statement($"MERGE (n:{NcsPrefix}{nodeLabel} {{ {NcsPrefix}{propertyName}: $property_value }}) RETURN n",
//                    //a dic of dic or somesuch could be passed in
////                    new {ncs_node_label = nodeLabel, property_name = propertyName, property_value = propertyValue.ToString()});
//                    new {property_value = propertyValue.ToString()});
//                IStatementResultCursor cursor = await session.RunAsync(statement);
//                var resultSummary = await cursor.ConsumeAsync();
//
//                var x = resultSummary.ResultAvailableAfter;
//                //types: https://neo4j.com/docs/driver-manual/1.7/cypher-values/
//            }
//            finally
//            {
//                await session.CloseAsync();
//            }
//
//            //pre 4 example from docs
////            using (var session = _driver.Session())
////            {
////                var greeting = session.WriteTransaction(tx =>
////                {
////                    var result = tx.Run("CREATE (a:Greeting) " +
////                                        "SET a.message = $message " +
////                                        "RETURN a.message + ', from node ' + id(a)",
////                        new {message});
////                    return result.Single()[0].As<string>();
////                });
////                Console.WriteLine(greeting);
////            }
//        }
        
        public void Dispose()
        {
            _driver?.Dispose();
        }
        
        // wrap types up in class?
        // borrow code from client?
        // private string PropertyToCypherLiteral(object value, NeoPropertyType propertyType)
        // {
        //     if (propertyType == NeoPropertyType.Number)
        //         return value.ToString();
        //     return $"'{value}'";
        // }
    }
}