﻿// See https://aka.ms/new-console-template for more information

using System.Text;
using Microsoft.Azure.Cosmos;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

Console.WriteLine("Neo4j Endpoint: Please enter the Neo4j endpoint (e.g. bolt://HOSTNAME:7687)");
var neo4JEndpoint = Console.ReadLine()!.Trim();

Console.WriteLine("Neo4j Username: Please enter the Neo4j username");
var neo4JUsername = Console.ReadLine()!.Trim();

Console.WriteLine("Neo4j Password: Please enter the Neo4j password");
var neo4JPassword = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos Db Connection string: Please enter the Cosmos db connection string (e.g. AccountEndpoint=https://HOSTNAME:443/;AccountKey=KEY;)");
var cosmosConnectionString = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos Db Database name: Please enter the Cosmos Db database name (e.g. dev)");
var cosmosDbDatabaseName = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos Db Container name: Please enter the Cosmos Db container name to write to (e.g. published)");
var cosmosDbContainerName = Console.ReadLine()!.Trim();

Console.WriteLine("Amount of parallelisation: The amount of saves can do at once (higher the better - but balance needed not to saturate). If container is 400RUs try 15, if 1000RUs try 35");
var parallelisationAmountStr = Console.ReadLine()!.Trim();
var parallelisationAmount = string.IsNullOrEmpty(parallelisationAmountStr) ? 15 : int.Parse(parallelisationAmountStr);

Console.WriteLine("Please review the entered information in the summary below.");
Console.WriteLine();

Console.WriteLine($"Neo4j endpoint - {neo4JEndpoint}");
Console.WriteLine($"Neo4j username - {neo4JUsername}");
Console.WriteLine($"Neo4j password - {neo4JPassword}");
Console.WriteLine($"Cosmos Db connection string - {cosmosConnectionString}");
Console.WriteLine($"Cosmos Db database name - {cosmosDbDatabaseName}");
Console.WriteLine($"Cosmos Db container name - {cosmosDbContainerName}");
Console.WriteLine($"Amount of parallelisation - {parallelisationAmount}");

Console.WriteLine();
Console.WriteLine("Press Y to proceed - or any other key to quite");

var y = Console.ReadKey().KeyChar.ToString();
Console.WriteLine();

if (!y.Equals("y", StringComparison.InvariantCultureIgnoreCase))
{
    Console.WriteLine("'Y' not hit - quitting");
    return;
}

var cosmosDb = new CosmosClient(cosmosConnectionString);

var exclusions = new List<string>
{
    "Resource",
    "_GraphConfig",
    "_NsPrefDef",
    "skosxl__Label",
    "skos__Concept",
    "skos__ConceptScheme",
    "esco__NodeLiteral",
    "esco__AssociationObject",
    "esco__MemberConcept",
    "esco__Skill",
    "esco__Label",
    "esco__Occupation",
    "esco-rp__Regulation",
    "esco__ConceptScheme",
    "esco__Structure",
    "esco__LabelRole",
    "DynamicTitlePrefix"
};

Console.WriteLine("Request content type (label) information.");

var contentTypes = (await RunQuery(new Query("call db.labels()")))
    .Select(label => label.Values.Values.First().As<string>())
    .Where(label => !exclusions.Contains(label))
    .ToList();

Console.WriteLine($"{contentTypes.Count} labels (content types) received, after filtering. Looping through content types now to fetch data.");
var contentTypeCount = 1;
var contentTypeTotal = contentTypes.Count;

var reportDictionary = new Dictionary<string, (int, int)>();

foreach (var contentType in contentTypes)
{
    Console.WriteLine($"Requesting document data from {contentType}. Content type {contentTypeCount} of {contentTypeTotal}...");
    var documents = await RunQuery(new Query($"match (a:{contentType}) return a"));
    Console.WriteLine($"{documents.Count} records found for {contentType} - Content type {contentTypeCount} of {contentTypeTotal}.");

    Console.WriteLine($"Requesting contHas relationship records for {contentType} - Content type {contentTypeCount} of {contentTypeTotal}...");
    var contHasRelationships = await RunQuery(new Query($"match (a:{contentType})-[r]->(b) return a.uri, r, b.uri"));
    Console.WriteLine($"{contHasRelationships.Count} contHas relationship records found for {contentType} - Content type {contentTypeCount} of {contentTypeTotal}.");

    Console.WriteLine($"Requesting incoming relationship records for {contentType} - Content type {contentTypeCount} of {contentTypeTotal}...");
    var incomingRelationships = await RunQuery(new Query($"match (a:{contentType})<-[r]-(b) return a.uri, r, b.uri"));
    Console.WriteLine($"{incomingRelationships.Count} incoming relationship records found for {contentType} - Content type {contentTypeCount} of {contentTypeTotal}.");

    var documentCount = 1;
    var documentTotal = documents.Count;

    var documentGroups = documents.Chunk(parallelisationAmount);

    foreach (var documentGroup in documentGroups)
    {
        var tasks = new List<Task>();

        foreach (var document in documentGroup)
        {
            var documentNode = document.Values["a"] as INode;
            var documentUri = documentNode!.Properties["uri"].As<string>();

            var relevantIncomingData = incomingRelationships
                .Where(relationship => (string) relationship.Values["a.uri"] == documentUri).ToList();
            var relevantContHasData = contHasRelationships
                .Where(relationship => (string) relationship.Values["a.uri"] == documentUri).ToList();

            tasks.Add(ProcessAndSaveDocument(documentNode, documentUri, relevantIncomingData, relevantContHasData,
                contentType, documentCount++, documentTotal));
        }

        await Task.WhenAll(tasks);
    }

    reportDictionary.Add(contentType, (documentCount - 1, documentTotal));
    contentTypeCount++;
}

Console.WriteLine("Finished processing. Summary report..");
Console.WriteLine();

string reportStr = GenerateReport(reportDictionary);
Console.Write(reportStr);
File.WriteAllText($"{DateTime.Now.ToString("yyyy-MM-dd")}-report.txt", reportStr);

string GenerateReport(Dictionary<string, (int, int)> reportDictionary)
{
    var sb = new StringBuilder();
    sb.AppendLine("Content type,CountInGraph,CountSavedToCosmos");

    foreach ((string? key, (int, int) value) in reportDictionary)
    {
        sb.AppendLine($"{key},{value.Item1},{value.Item2}");
    }

    return sb.ToString();
}

async Task ProcessAndSaveDocument(
    INode documentNode,
    string documentUri,
    List<IRecord> incomingData,
    List<IRecord> contHasData,
    string contentType,
    int documentCount,
    int documentTotal)
{
    var (_, id, cont) = GetContentTypeAndId(documentUri);
    var curies = new List<Dictionary<string, object>>
    {
        new()
        {
            {"name", "cont"},
            {"href", cont}
        },
        new()
        {
            {"name", "incoming"},
            {"items", new List<Dictionary<string, object>>()}
        }
    };

    var links = new Dictionary<string, object>
    {
        {"self", documentUri},
        {"curies", curies}
    };

    var document = new Dictionary<string, object>(documentNode.Properties)
    {
        {"id", id.ToString()},
        {"ContentType", contentType.ToLower()},
        {"_links", links}
    };

    if (document.ContainsKey("ModifiedDate") && document["ModifiedDate"] is ZonedDateTime modifiedDate)
    {
        document["ModifiedDate"] = modifiedDate.ToDateTimeOffset().UtcDateTime.ToString("o");
    }

    if (document.ContainsKey("CreatedDate") && document["CreatedDate"] is ZonedDateTime createdDate)
    {
        document["CreatedDate"] = createdDate.ToDateTimeOffset().UtcDateTime.ToString("o");
    }

    AddContHasRelationships(document, links, contHasData);
    AddIncomingRelationships(document, links, incomingData);

    Console.WriteLine($"Saving record {id} for {contentType} ({documentCount} of {documentTotal}) - content type {contentTypeCount} of {contentTypeTotal}.");
    await SaveToCosmosDb(document);
}

void AddContHasRelationships(
    Dictionary<string, object> document,
    Dictionary<string, object> linksSection,
    List<IRecord> contHasData)
{
    foreach (var contHasDataRow in contHasData)
    {
        string relationshipUri = (string) contHasDataRow.Values["b.uri"];
        (string contentType, _, string contUrl) = GetContentTypeAndId(relationshipUri);
        string href = relationshipUri.Replace(contUrl, string.Empty);

        var dataToAdd = new Dictionary<string, object>
        {
            { "href", href },
            { "contentType", contentType }
        };

        var relationship = (contHasDataRow.Values["r"] as IRelationship)!;

        if (relationship.Properties?.Keys.Any() == true)
        {
            foreach ((string? key, object? value) in relationship.Properties)
            {
                dataToAdd.Add(key, value);
            }
        }

        string contHasKey = $"cont:{relationship.Type}";

        if (!linksSection.ContainsKey(contHasKey))
        {
            linksSection.Add(contHasKey, dataToAdd);
        }
        else
        {
            object contHasSection = linksSection[contHasKey];

            switch (contHasSection)
            {
                case JObject existingDataWrapper:
                {
                    var existingData = existingDataWrapper.ToObject<Dictionary<string, object>>();
                    var newList = new List<Dictionary<string, object>>
                    {
                        existingData,
                        dataToAdd
                    };

                    linksSection[contHasKey] = newList;
                    break;
                }
                case Dictionary<string, object> existingData:
                {
                    var newList = new List<Dictionary<string, object>>
                    {
                        existingData,
                        dataToAdd
                    };

                    linksSection[contHasKey] = newList;
                    break;
                }
                case JArray existingArrayWrapper:
                {
                    var existingArray = existingArrayWrapper.ToObject<List<Dictionary<string, Object>>>();
                    existingArray.Add(dataToAdd);

                    linksSection[contHasKey] = existingArray;
                    break;
                }
                case List<Dictionary<string, object>> existingList:
                {
                    existingList.Add(dataToAdd);

                    linksSection[contHasKey] = existingList;
                    break;
                }
                default:
                    throw new Exception($"Didn't expect type {contHasSection.GetType().Name}");
            }
        }
    }

    document["_links"] = linksSection;
}

void AddIncomingRelationships(
    Dictionary<string, object> document,
    Dictionary<string, object> linksSection,
    List<IRecord> incomingData)
{
    var curiesSection = SafeCastToList(linksSection["curies"]);
    int incomingPosition = curiesSection.FindIndex(curie =>
        (string)curie["name"] == "incoming");

    var incomingObject = curiesSection.Count > incomingPosition ? curiesSection[incomingPosition] : null;

    if (incomingObject == null)
    {
        throw new MissingFieldException("Incoming property missing");
    }

    var incomingList = SafeCastToList(incomingObject["items"]);

    foreach (IRecord incomingDataRow in incomingData)
    {
        string uri = (string)incomingDataRow.Values["b.uri"];
        (string contentType, Guid id, _) = GetContentTypeAndId(uri);

        if (incomingList.Any(incomingItem =>
                (string)incomingItem["contentType"] == contentType && (string) incomingItem["id"] == id.ToString()))
        {
            Console.WriteLine($"Link already exists for {contentType}.");
            continue;
        }

        var dataToAdd = new Dictionary<string, object>
        {
            { "contentType", contentType.ToLower() },
            { "id", id.ToString() }
        };

        var relationship = (incomingDataRow.Values["r"] as IRelationship)!;

        if (relationship.Properties?.Keys.Any() == true)
        {
            foreach ((string? key, object? value) in relationship.Properties)
            {
                dataToAdd.Add(key, value);
            }
        }

        incomingList.Add(dataToAdd);
    }

    incomingObject["items"] = incomingList;
    curiesSection[incomingPosition] = incomingObject;
    linksSection["curies"] = curiesSection;
    document["_links"] = linksSection;
}

static (string ContentType, Guid Id, string Cont) GetContentTypeAndId(string uri)
{
    try
    {
        var uriType = new Uri(uri, UriKind.Absolute);
        string pathOnly = uriType.AbsolutePath;
        pathOnly = pathOnly.ToLower().Replace("/api/execute", string.Empty);

        string[] uriParts = pathOnly.Trim('/').Split('/');
        string contentType = uriParts[0].ToLower();
        var id = Guid.Parse(uriParts[1]);

        return (contentType, id, $"{uriType.Scheme}://{uriType.Host}/api/execute");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        throw;
    }
}

async Task SaveToCosmosDb(Dictionary<string, object> properties)
{
    var container = GetContainer();
    string? contentType = properties["ContentType"].As<string>();
    int tries = 0;
    Exception? lastException = null;

    while (tries++ < 5)
    {
        try
        {

            await container.UpsertItemAsync(properties, new PartitionKey(contentType));
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

Container GetContainer()
{
    return cosmosDb.GetDatabase(cosmosDbDatabaseName).GetContainer(cosmosDbContainerName);
}

async Task<List<IRecord>> RunQuery(Query query)
{
    var session = GetAsyncSession();

    try
    {
        var cursor = await session.RunAsync(query);
        return await cursor.ToListAsync(record => record);
    }
    finally
    {
        await session.CloseAsync();
    }
}

IAsyncSession GetAsyncSession()
{
    return GetNeo4JDriver().AsyncSession();
}

IDriver GetNeo4JDriver()
{
    return GraphDatabase.Driver(neo4JEndpoint,AuthTokens.Basic(neo4JUsername, neo4JPassword));
}

List<Dictionary<string, object>> SafeCastToList(object value)
{
    if (value is JArray valAry)
    {
        return valAry.ToObject<List<Dictionary<string, object>>>();
    }

    return (List<Dictionary<string, object>>)value;
}
