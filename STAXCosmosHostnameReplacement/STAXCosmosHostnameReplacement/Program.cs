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
string replaceString= "https://dfc-dev-";


Console.WriteLine("searchString {0} replaceString{1} ", searchString, replaceString);


ListDatabasesAsync();
Console.WriteLine("Finished");


Console.WriteLine("Cosmos Db Database name: Please enter the Cosmos Db database name (usually 'dfc-cms-data')");
string cosmosDbDatabaseName = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos Db Preview Container name: Please enter the Cosmos Db container name for preview (usually 'preview')");
string cosmosDbContainerNamePreview = Console.ReadLine()!.Trim();

Console.WriteLine("Preview hostname to swap from: Please enter the uri to find for preview (e.g. https://dfc-sit-draft-api-cont-fa.azurewebsites.net)");
string originalPreviewUri = Console.ReadLine()!.Trim();

Console.WriteLine("Preview hostname to swap to: Please enter the uri to replace with for preview (e.g. https://dfc-dev-draft-api-cont-fa.azurewebsites.net)");
string eventualPreviewUri = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos Db Published Container name: Please enter the Cosmos Db container name for publish (usually 'published')");
string cosmosDbContainerNamePublished = Console.ReadLine()!.Trim();

Console.WriteLine("Published hostname to swap from: lease enter the uri to find for published (e.g. https://dfc-sit-api-cont-fa.azurewebsites.net)");
string originalPublishedUri = Console.ReadLine()!.Trim();

Console.WriteLine("Published hostname to swap to: Please enter the uri to replace with for published (e.g. https://dfc-dev-api-cont-fa.azurewebsites.net)");
string eventualPublishedUri = Console.ReadLine()!.Trim();

Console.WriteLine("Please review the entered information in the summary below.");
Console.WriteLine();

Console.WriteLine($"Cosmos Db connection string - {cosmosConnectionString}");
//Console.WriteLine($"Cosmos Db database name - {cosmosDbDatabaseName}");
//Console.WriteLine($"Cosmos Db preview container name - {cosmosDbContainerNamePreview}");
Console.WriteLine($"Preview hostname to swap from (find) - {originalPreviewUri}");
Console.WriteLine($"Preview hostname to swap to (replace) - {eventualPreviewUri}");
Console.WriteLine($"Cosmos Db published container name - {cosmosDbContainerNamePublished}");
Console.WriteLine($"Published hostname to swap from (find) - {originalPublishedUri}");
Console.WriteLine($"Published hostname to swap to (replace) - {eventualPublishedUri}");

Console.WriteLine();
Console.WriteLine("Press Y to proceed - or any other key to quit");

string y = Console.ReadKey().KeyChar.ToString();
Console.WriteLine();

if (!y.Equals("y", StringComparison.InvariantCultureIgnoreCase))
{
    Console.WriteLine("'Y' not hit - quitting");
    return;
}

var cosmosDb = new CosmosClient(cosmosConnectionString);

foreach (bool isPublished in new[] { false, true })
{
    int totalDocumentsCount = await GetDocumentsCount(isPublished);
    int outerForeachCount = 0;
    const int readBatchSize = 100;
    var count = 1;

    while (outerForeachCount * readBatchSize < totalDocumentsCount)
    {
        var documents = await FetchDocuments(
            readBatchSize * outerForeachCount++,
            readBatchSize,
            isPublished);

        foreach (var document in documents)
        {
            var links = document["_links"] as JObject;
            var curies = links!["curies"] as JArray;
            var cont = curies!.First();

            string uri = (string)document["uri"];
            string? self = links["self"].ToObject<string>();
            string? href = (cont["href"] as JValue)!.ToObject<string>();

            string originalHostname = isPublished ? originalPublishedUri : originalPreviewUri;
            string id = (string)document["id"];

            if (uri.IndexOf(originalHostname, StringComparison.Ordinal) == -1
                && self.IndexOf(originalHostname, StringComparison.Ordinal) == -1
                && href.IndexOf(originalHostname, StringComparison.Ordinal) == -1)
            {
                WriteAndLog($"Entry skipped for {count}, {id} as didn't contain hostname - {DateTime.Now:yyyy-MM-dd hh:mm:ss}");
                count++;

                continue;
            }

            document["uri"] = ReplaceHostname(uri, isPublished);
            links["self"] = ReplaceHostname(self, isPublished);
            cont["href"] = ReplaceHostname(href, isPublished);
            links["curies"] = curies;
            document["_links"] = links;

            await SaveToCosmosDb(document, isPublished, count);
            count++;
        }
    }
}

Console.WriteLine("Finished processing. Summary report..");
Console.WriteLine();

File.WriteAllText($"{DateTime.Now:yyyy-MM-dd}-report.txt", logger.ToString());

void WriteAndLog(string message, int blankLinesAfter = 0)
{
    Console.WriteLine(message);
    logger.AppendLine(message);

    for (int idx = 0; idx < blankLinesAfter; idx++)
    {
        Console.WriteLine(string.Empty);
        logger.AppendLine(string.Empty);
    }
}

string ReplaceHostname(string inputString, bool isPublished)
{
    return inputString.Replace(
        (isPublished ? originalPublishedUri : originalPreviewUri),
        isPublished ? eventualPublishedUri : eventualPreviewUri);
}

async Task<int> GetDocumentsCount(bool isPublished)
{
    string word = isPublished ? "published" : "preview";

    WriteAndLog($"Started fetching document count for {word} - {DateTime.Now:yyyy-MM-dd hh:mm:ss}");
    var start = DateTime.Now;

    var container = GetContainer(isPublished);
    var iteratorLoop = container.GetItemQueryIterator<int>(
        "select value count(c) from c",
        requestOptions: new QueryRequestOptions
        {
            MaxItemCount = -1
        });

    int count = (await iteratorLoop.ReadNextAsync()).First();

    WriteAndLog($"Finished fetching document count for {word} ({count}) - {DateTime.Now:yyyy-MM-dd hh:mm:ss} - took " +
            $"{(DateTime.Now - start).TotalSeconds} seconds");

    return count;
}

async Task<List<Dictionary<string, object>>> FetchDocuments(int startNumber, int batchSize, bool isPublished)
{
    string word = isPublished ? "published" : "preview";

    WriteAndLog(
        $"Started fetching documents for {word} - {startNumber} to {startNumber + batchSize} - {DateTime.Now:yyyy-MM-dd hh:mm:ss}");
    var start = DateTime.Now;

    var container = GetContainer(isPublished);
    var returnList = new List<Dictionary<string, object>>();

    var iteratorLoop = container.GetItemQueryIterator<Dictionary<string, object>>(
        $"select c from c order by c.id asc OFFSET {startNumber} LIMIT {batchSize}",
        requestOptions: new QueryRequestOptions
        {
            MaxItemCount = -1,
        });

    while (iteratorLoop.HasMoreResults)
    {
        returnList.AddRange((await iteratorLoop.ReadNextAsync())
            .Select(dyn => (dyn["c"] as JObject)!.ToObject<Dictionary<string, object>>())
            .ToList());
    }

    WriteAndLog(
        $"Finished fetching documents for {word} - {startNumber} to {startNumber + batchSize}. Found {returnList.Count} - {DateTime.Now:yyyy-MM-dd hh:mm:ss} - " +
        $"took {(DateTime.Now - start).TotalSeconds} seconds");

    return returnList;
}

async Task SaveToCosmosDb(Dictionary<string, object> properties, bool isPublished, int count)
{
    var start = DateTime.Now;

    var container = GetContainer(isPublished);
    string? contentType = (string?)properties["ContentType"];
    int tries = 0;
    Exception? lastException = null;

    while (tries++ < 5)
    {
        try
        {
            await container.UpsertItemAsync(properties, new PartitionKey(contentType));

            WriteAndLog(
                $"Saved document {count} {properties["id"]} - {DateTime.Now:yyyy-MM-dd hh:mm:ss} - took {(DateTime.Now - start).TotalSeconds} seconds");

            return;
        }
        catch (Exception? ex)
        {
            await Task.Delay(5000);
            lastException = ex;
        }
    }

    throw new Exception("Failed to save after 5 attempts", lastException);
}

Container GetContainer(bool isPublished)
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
                        FeedResponse <dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
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

