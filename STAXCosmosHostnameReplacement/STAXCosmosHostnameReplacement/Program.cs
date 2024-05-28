//See https://aka.ms/new-console-template for more information
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Container = Microsoft.Azure.Cosmos.Container;

var logger = new StringBuilder();

//Console.WriteLine("Cosmos Db Connection string: Please enter the Cosmos db connection string (e.g. AccountEndpoint=https://HOSTNAME:443/;AccountKey=KEY;)");
//string cosmosConnectionString = Console.ReadLine()!.Trim();
//string cosmosConnectionString = "AccountEndpoint=https://dfc-dev-pers-sharedresources-cdb.documents.azure.com:443/;AccountKey=EjdqtkeR9h583mCNAx3YwAwmtBUjZ1Z5jAblEoG6fyTCemym8Hm34KZiL5EvllYGWGukNUuxGeGwN9lox96eFg==;";
string cosmosConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
Console.WriteLine($"Cosmos Db connection string - {cosmosConnectionString}");
CosmosClient cosmosClient = new CosmosClient(cosmosConnectionString);
string searchString = "https://dfc-test-";
string replaceString = "https://dfc-dev-";


Console.WriteLine("searchString {0} replaceString{1} ", searchString, replaceString);


ListDatabasesAsync();
Console.WriteLine("Finished");


Console.WriteLine("Cosmos Db Database name: Please enter the Cosmos Db database name (usually 'dfc-cms-data')");
string cosmosDbDatabaseName = Console.ReadLine()!.Trim();
	@@ -33,8 + 51,8 @@
Console.WriteLine();

Console.WriteLine($"Cosmos Db connection string - {cosmosConnectionString}");
//Console.WriteLine($"Cosmos Db database name - {cosmosDbDatabaseName}");
//Console.WriteLine($"Cosmos Db preview container name - {cosmosDbContainerNamePreview}");
Console.WriteLine($"Preview hostname to swap from (find) - {originalPreviewUri}");
Console.WriteLine($"Preview hostname to swap to (replace) - {eventualPreviewUri}");
Console.WriteLine($"Cosmos Db published container name - {cosmosDbContainerNamePublished}");
	@@ -55,7 + 73,7 @@

var cosmosDb = new CosmosClient(cosmosConnectionString);

foreach (bool isPublished in new[] { false, true })
{
    int totalDocumentsCount = await GetDocumentsCount(isPublished);
    int outerForeachCount = 0;
	@@ -117,7 + 135,7 @@ void WriteAndLog(string message, int blankLinesAfter = 0)
    for (int idx = 0; idx < blankLinesAfter; idx++)
    {
        Console.WriteLine(string.Empty);
        logger.AppendLine(string.Empty);
    }
}

	@@ -166,7 + 184,7 @@ async Task<int> GetDocumentsCount(bool isPublished)
        $"select c from c order by c.id asc OFFSET {startNumber} LIMIT {batchSize}",
        requestOptions: new QueryRequestOptions
        {
            MaxItemCount = -1,
        });

while (iteratorLoop.HasMoreResults)
	@@ -217,3 + 235,144 @@ Container GetContainer(bool isPublished)
{
    return cosmosDb.GetDatabase(cosmosDbDatabaseName).GetContainer(isPublished ? cosmosDbContainerNamePublished : cosmosDbContainerNamePreview);
}

async Task ListContainersAsync(string DatabaseId)
{
    //Console.WriteLine("Listing containers in the database: {0}", DatabaseId);

    using (FeedIterator<ContainerProperties> iterator = cosmosClient.GetDatabase(DatabaseId).GetContainerQueryIterator<ContainerProperties>())
    {
        while (iterator.HasMoreResults)
        {
            FeedResponse<ContainerProperties> response = await iterator.ReadNextAsync();
            foreach (ContainerProperties container in response)
            {
                //Console.WriteLine("......Container Id: {0} ", container.Id);
                var sqlQueryText = "SELECT * FROM c";
                //Console.WriteLine("Running query: {0}\n", sqlQueryText);

                try
                {
                    //Console.WriteLine("Attempting to get container with ID: {0} in database: {1}", container.Id, DatabaseId);
                    Container container1 = cosmosClient.GetDatabase(DatabaseId).GetContainer(container.Id);
                    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                    FeedIterator<dynamic> queryResultSetIterator = container1.GetItemQueryIterator<dynamic>(queryDefinition);

                    List<dynamic> items = new List<dynamic>();

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (var item in currentResultSet)
                        {

                            // Convert item to JObject for easy searching and updating
                            JObject jsonItem = JObject.FromObject(item);
                            if (ItemContainsAndReplaceString(jsonItem, searchString, replaceString))
                            {
                                Console.WriteLine("\tFound and replaced search string in database: {0} container {1} \n", DatabaseId, container.Id);

                                // Save the updated item back to the container
                                await container1.ReplaceItemAsync(jsonItem, jsonItem["id"].ToString());
                                Console.WriteLine("\tReplaced Found and replaced search string in database: {0} container {1} {2}\n", DatabaseId, container.Id, jsonItem["id"].ToString());
                            }
                        }
                    }
                }
                catch (CosmosException cosmosException)
                {
                    // Log the CosmosException with its status code and message
                    Console.WriteLine("CosmosException with status code {0}: {1}", cosmosException.StatusCode, cosmosException.Message);
                }
                catch (Exception e)
                {
                    // Log the general exception
                    Console.WriteLine("Exception: {0}", e.Message);
                }

            }
        }
    }
}
async Task ListDatabasesAsync()
{
    //Console.WriteLine("Listing databases in the Cosmos DB account");

    using (FeedIterator<DatabaseProperties> iterator = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>())
    {
        try
        {
            while (iterator.HasMoreResults)
            {
                FeedResponse<DatabaseProperties> response = await iterator.ReadNextAsync();
                foreach (DatabaseProperties database in response)
                {
                    //Console.WriteLine("Database Id: {0}", database.Id);
                    ListContainersAsync(database.Id);
                }
            }
        }
        catch (Exception e)
        {

            throw;
        }
    }
}
bool ItemContainsString(JObject item, string searchString)
{
    foreach (var property in item.Properties())
    {
        if (property.Value.Type == JTokenType.String && property.Value.ToString().Contains(searchString))
        {
            return true;
        }
        else if (property.Value.Type == JTokenType.Object)
        {
            if (ItemContainsString((JObject)property.Value, searchString))
            {
                return true;
            }
        }
    }
    return false;
}


bool ItemContainsAndReplaceString(JToken item, string searchString, string replaceString)
{
    bool found = false;

    if (item.Type == JTokenType.Object)
    {
        foreach (var property in ((JObject)item).Properties())
        {
            if (ItemContainsAndReplaceString(property.Value, searchString, replaceString))
            {
                found = true;
            }
        }
    }
    else if (item.Type == JTokenType.Array)
    {
        for (int i = 0; i < ((JArray)item).Count; i++)
        {
            if (ItemContainsAndReplaceString(((JArray)item)[i], searchString, replaceString))
            {
                found = true;
            }
        }
    }
    else if (item.Type == JTokenType.String)
    {
        string strValue = item.ToString();
        if (strValue.Contains(searchString))
        {
            item.Replace(strValue.Replace(searchString, replaceString));
            found = true;
        }
    }

    return found;
}
