using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace STAXCosmosHostnameReplacement.Commands
{
    internal class STAXCosmosHostNameReplacementCommand : AsyncCommand<STAXCosmosHostNameReplacementOptions>
    {
        internal CosmosClient _cosmosClient;

        public override ValidationResult Validate(CommandContext context, STAXCosmosHostNameReplacementOptions settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                return ValidationResult.Error("Cosmos Connection string is a required parameter, and should not be empty.");
            }

            if (string.IsNullOrWhiteSpace(settings.SearchDomain))
            {
                return ValidationResult.Error("The Domain to search for is a required paramter, and should not be empty.");
            }

            if (string.IsNullOrWhiteSpace(settings.ReplacementDomain))
            {
                return ValidationResult.Error("The Domain to replace is a required paramter, and should not be empty.");
            }

            return ValidationResult.Success();
        }

        public override async Task<int> ExecuteAsync(CommandContext context, STAXCosmosHostNameReplacementOptions settings)
        {
            _cosmosClient = new CosmosClient(settings.ConnectionString);

            var databases = await GetDatabases();

            foreach (var database in databases)
            {
                var containers = await GetContainersOnDatabase(database);

                foreach (var container in containers)
                {
                    await UpdateMatchingDocumentsInCollection(database, container, settings.SearchDomain, settings.ReplacementDomain);
                }
            }

            return 0;
        }

        async Task<List<string>> GetDatabases()
        {
            var databases = new List<string>();

            using (FeedIterator<DatabaseProperties> databaseIterator = _cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>())
            {
                while (databaseIterator.HasMoreResults)
                {
                    FeedResponse<DatabaseProperties> response = await databaseIterator.ReadNextAsync();
                    foreach (DatabaseProperties database in response)
                    {
                        databases.Add(database.Id);
                    }
                }
            }

            return databases;
        }

        async Task<List<string>> GetContainersOnDatabase(string database)
        {
            var containers = new List<string>();

            using (FeedIterator<ContainerProperties> iterator = _cosmosClient.GetDatabase(database).GetContainerQueryIterator<ContainerProperties>())
            {
                while (iterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> response = await iterator.ReadNextAsync();

                    foreach(ContainerProperties container in response)
                    {
                        containers.Add(container.Id);
                    }
                }
            }

            return containers;
        }

        async Task UpdateMatchingDocumentsInCollection(string database, string collection, string search, string replacement)
        {
            var container = _cosmosClient.GetDatabase(database).GetContainer(collection);

            QueryDefinition queryDef = new QueryDefinition("SELECT * FROM c");

            FeedIterator<dynamic> documentIterator = container.GetItemQueryIterator<dynamic>(queryDef);

            while (documentIterator.HasMoreResults)
            {
                FeedResponse<dynamic> documentResultSet = await documentIterator.ReadNextAsync();

                foreach(var item in documentResultSet)
                {
                    JObject jsonItem = JObject.FromObject(item);

                    var replaceCount = PerformItemReplacements(jsonItem, search, replacement);

                    if(replaceCount > 0)
                    {
                        await container.ReplaceItemAsync(jsonItem, jsonItem["id"].ToString());
                    }
                }
            }
        }


        int PerformItemReplacements(JToken item, string search, string replace)
        {
            int replaceCount = 0;
            var searchRegex = $"https://(.*){search}";

            if (item.Type == JTokenType.Object)
            {
                foreach (var property in ((JObject)item).Properties())
                {
                    replaceCount += PerformItemReplacements(property.Value, search, replace);
                }
            }
            else if (item.Type == JTokenType.Array)
            {

                for (int i = 0; i < ((JArray)item).Count; i++)
                {
                    replaceCount += PerformItemReplacements(((JArray)item)[i], search, replace);
                }
            }
            else if (item.Type == JTokenType.String)
            {
                string strValue = item.ToString();

                var matches = Regex.Matches(strValue, searchRegex);

                foreach (object? match in matches)
                {
                    var termToReplace = matches[0].Value;

                    var oldHostName = matches[0].Groups[1].Captures[0].Value;

                    var replacement = $"https://{oldHostName}-ver2{replace}";
                    item.Replace(strValue.Replace(termToReplace, replacement));
                    replaceCount++;
                }
            }

            return replaceCount;
        }
    }
}
