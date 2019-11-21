using System;
using System.Threading.Tasks;
using Neo4j.Driver;

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
    public class NeoGraphDatabase : IDisposable
    {
        private readonly IDriver _driver;
//        private const string NcsPrefix = "https://nationalcareers.service.gov.uk/";
        private const string NcsPrefix = "ncs__";

        public NeoGraphDatabase()
        {
            //todo: one driver per application : singleton.
//            There are a few points that need to be highlighted when adding this driver into your project:
//
//            Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.
//                It is considerably cheap to create new sessions and transactions, as sessions and transactions do not create new connections as long as there are free connections available in the connection pool.
//                The driver is thread-safe, while the session or the transaction is not thread-safe.
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "ESCO3"));
        }

        //todo: dictionary of properties
        public async Task Merge(string nodeLabel, string propertyName, object propertyValue, NeoPropertyType propertyType) //enum would be better
        {
            //todo: transaction, parameterisation/construct Statement
            var session = _driver.AsyncSession(); // <v4 does not support multiple databases : o => o.WithDatabase("ESCO3")); //withDatabase("neo4j"));
            try
            {
                // https://neo4j.com/docs/cypher-manual/current/clauses/merge/
                // doesn't support partial matches - would be an issue if we add a field to a content type
                IStatementResultCursor cursor = await session.RunAsync($"MERGE (n:{NcsPrefix}{nodeLabel} {{ {propertyName}: {PropertyToCypherLiteral(propertyValue, propertyType)} }}) RETURN n");
                var resultSummary = await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }

            //pre 4 example from docs
//            using (var session = _driver.Session())
//            {
//                var greeting = session.WriteTransaction(tx =>
//                {
//                    var result = tx.Run("CREATE (a:Greeting) " +
//                                        "SET a.message = $message " +
//                                        "RETURN a.message + ', from node ' + id(a)",
//                        new {message});
//                    return result.Single()[0].As<string>();
//                });
//                Console.WriteLine(greeting);
//            }
        }
        
        public void Dispose()
        {
            _driver?.Dispose();
        }
        
        // wrap types up in class?
        // borrow code from client?
        private string PropertyToCypherLiteral(object value, NeoPropertyType propertyType)
        {
            if (propertyType == NeoPropertyType.Number)
                return value.ToString();
            return $"'{value}'";
        }
    }
}