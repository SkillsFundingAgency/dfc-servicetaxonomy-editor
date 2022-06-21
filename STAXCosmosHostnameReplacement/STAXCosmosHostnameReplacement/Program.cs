// See https://aka.ms/new-console-template for more information
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

var logger = new StringBuilder();

Console.WriteLine("Cosmos Db Connection string: Please enter the Cosmos db connection string (e.g. AccountEndpoint=https://HOSTNAME:443/;AccountKey=KEY;)");
string cosmosConnectionString = Console.ReadLine()!.Trim();

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
Console.WriteLine($"Cosmos Db database name - {cosmosDbDatabaseName}");
Console.WriteLine($"Cosmos Db preview container name - {cosmosDbContainerNamePreview}");
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

foreach (bool isPublished in new[] {false, true})
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
            MaxItemCount =-1,
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
