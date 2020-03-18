using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    //todo: it was graph sync that done it, aka graph sync is slow! optimise it

    // creating OccupationLabels : conclusion => noise!
    // # content items : batch size : time
    // 100 :  1 : 00:00:00.3099636
    // 100 :  5 : 00:00:00.3161795
    // 100 : 50 : 00:00:00.3081626
    public class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";
        private const int CreateContentBatchSize = 50;

        public CypherToContentStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentItemIdGenerator idGenerator,
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals,
            ILogger<CypherToContentStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _idGenerator = idGenerator;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
            _logger = logger;
        }

        //todo: new recipes in importer in order
        //todo: occupation in job profile bag : readonly
        //todo: need to disable graph sync when creating content items from graph. can't do it globally, as not single user
        //todo: why can't GraphSyncPartDisplayDriver injectGraphSyncHelper?
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            var step = context.Step.ToObject<CypherToContentStepModel>();

            foreach (var queries in step!.Queries ?? Enumerable.Empty<CypherToContentModel?>())
            {
                if (queries == null)
                    continue;

                _logger.LogInformation($"{StepName} step");

                var getContentItemsAsJsonQuery = _serviceProvider.GetRequiredService<IGetContentItemsAsJsonQuery>();

                getContentItemsAsJsonQuery.QueryStatement = queries.Query;

                _logger.LogInformation($"Executing query to retrieve content for items:\r\n{queries.Query}");

                List<string> contentItemsJsonPreTokenization = await _graphDatabase.Run(getContentItemsAsJsonQuery);

                IEnumerable<string> contentItemsJson = contentItemsJsonPreTokenization.Select(ReplaceCSharpHelpers);

                var contentItemJObjects = contentItemsJson
                    .Select(JsonConvert.DeserializeObject<List<JObject>>)
                    .SelectMany(cijo => cijo)
                    .ToArray();

                _logger.LogInformation($"Content for {contentItemJObjects.Length} items retrieved");

                var contentItemJObjectsBatches = contentItemJObjects
                    .Batch(CreateContentBatchSize);

                _logger.LogInformation($"Creating content items in batches of {CreateContentBatchSize}");

                Stopwatch stopwatch = Stopwatch.StartNew();

//todo: would using rx be faster? <- slowness is adding to oc db, bot reading from neo
//todo: parallel foreach?

                foreach (IEnumerable<JObject> contentJObjectBatch in contentItemJObjectsBatches)
                {
                    var createContentItemTasks = contentJObjectBatch.Select(CreateContentItem);

                    await Task.WhenAll(createContentItemTasks);
                }

                _logger.LogInformation($"Created {contentItemJObjects.Length} content items in {stopwatch.Elapsed}");
            }
        }

        private async Task CreateContentItem(JObject contentJObject)
        {
            ContentItem? contentItem = contentJObject.ToObject<ContentItem>();

            if (contentItem?.Content == null)
            {
                _logger.LogWarning("Missing content, unable to import.");
                return;
            }

// either _contentManager.New or get at json of results

            // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
            //contentItem.Id = 0;

            //should we allow the query to override any of these values?

            string? title = contentItem.Content.TitlePart?.Title;
            if (title != null)
            {
                contentItem.DisplayText = title;
            }

            contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);

            contentItem.CreatedUtc = contentItem.ModifiedUtc = contentItem.PublishedUtc = DateTime.UtcNow;
            //these get set when version id is 0
            // contentItem.Latest = true;
            // contentItem.Published = true;
            contentItem.Owner = "admin";
            contentItem.Author = "admin";

            //contentItem.Author = $"{StepName}:admin";
            contentItem.Content.DontSync = true;

            await _contentManager.CreateAsync(contentItem);

            // Overwrite ModifiedUtc & PublishedUtc values that handlers have changes
            // Should not be necessary if IContentManager had an Import method
            //contentItem.ModifiedUtc = modifiedUtc;
            //contentItem.PublishedUtc = publishedUtc;
        }

        private static readonly Regex _cSharpHelperRegex = new Regex(@"\[c#:([^\]]+)\]", RegexOptions.Compiled);
        private string ReplaceCSharpHelpers(string recipeFragment)
        {
            return _cSharpHelperRegex.Replace(recipeFragment, match => EvaluateCSharp(match.Groups[1].Value).GetAwaiter().GetResult());
        }

        private async Task<string> EvaluateCSharp(string code)
        {
            // can't see how to get json.net to unescape value strings!
            code = code.Replace("\\\"", "\"");

            return await CSharpScript.EvaluateAsync<string>(code, globals: _cypherToContentCSharpScriptGlobals);
        }

        public class CypherToContentModel
        {
            public string? Query { get; set; }
        }
//todo: setting whether serial or parallel (or parallel by default and multiple recipes for serial) and split label create across queries to speed up import (or wait until 1 recipe to import multiple recipes for parallalisation?)
        //todo: better names!
        public class CypherToContentStepModel
        {
            public CypherToContentModel[]? Queries { get; set; }
        }
    }
}
