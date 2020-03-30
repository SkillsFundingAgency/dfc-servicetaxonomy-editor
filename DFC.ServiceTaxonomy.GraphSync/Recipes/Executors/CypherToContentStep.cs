using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.Caching.Memory;
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
    //todo: utilise innerrecipres? see https://github.com/OrchardCMS/OrchardCore/blob/6cddd9f9904ff117ba82587f39fc76f49fac3cc3/src/OrchardCore/OrchardCore.Recipes.Core/Services/RecipeExecutor.cs
    //what does recipe step do?
//todo: https://github.com/OrchardCMS/OrchardCore/blob/6cddd9f9904ff117ba82587f39fc76f49fac3cc3/src/OrchardCore.Modules/OrchardCore.Recipes/RecipeSteps/RecipesStep.cs

    public class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";
        //private const int CreateContentBatchSize = 50;

        public CypherToContentStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentItemIdGenerator idGenerator,
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals,
            IMemoryCache memoryCache,
            ILogger<CypherToContentStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _idGenerator = idGenerator;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        //todo: occupation in job profile bag : readonly
        //todo: need to add validation, at least to detect when import same thing twice!
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            var step = context.Step.ToObject<CypherToContentStepModel>();

            foreach (CypherToContentModel? cypherToContent in step!.Queries ?? Enumerable.Empty<CypherToContentModel?>())
            {
                if (cypherToContent == null)
                    continue;

                _logger.LogInformation($"{StepName} step");

                var getContentItemsAsJsonQuery = _serviceProvider.GetRequiredService<IGetContentItemsAsJsonQuery>();

                getContentItemsAsJsonQuery.QueryStatement = cypherToContent.Query;

                _logger.LogInformation($"Executing query to retrieve content for items:\r\n{cypherToContent.Query}");

                List<string> contentItemsJsonPreTokenization = await _graphDatabase.Run(getContentItemsAsJsonQuery);

                IEnumerable<string> contentItemsJson = contentItemsJsonPreTokenization.Select(ReplaceCSharpHelpers);

                //todo: still slow even after disabling graph sync, need to speed it up more
                var contentItemJObjects = contentItemsJson
                    .Select(JsonConvert.DeserializeObject<List<JObject>>)
                    .SelectMany(cijo => cijo);

                Stopwatch stopwatch = Stopwatch.StartNew();

                var preparedContentItems = contentItemJObjects
                    .Select(PrepareContentItem)
                    .Where(i => i != null)
                    .Select(i => i!);

                int CreateContentBatchSize = 8;

                var preparedContentItemsBatches = preparedContentItems
                    .Batch(CreateContentBatchSize);

                //poc
                //todo: if works, keep n batches processing at once, avoid httpclient (ProcessRequestAsync? https://stackoverflow.com/questions/50407760/programmatically-invoking-the-asp-net-core-request-pipeline)
                // (if use httpclient wil have to set authentication headers and anti forgery token)

                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                HttpClient httpClient = new HttpClient(handler);
                var client = new GetJobProfiles.RestHttpClient.RestHttpClient(httpClient);

                var first8 = preparedContentItemsBatches.Take(8);

                var first8Tasks = first8.Select(b =>
                    {
                        string contentBatchJson = JsonConvert.SerializeObject(b);
                        string batchRecipe = WrapInNonSetupRecipe(contentBatchJson);
                        #pragma warning disable S1075
                        //return client.PostAsJson("https://127.0.0.1:5001/Admin/OrchardCore.Deployment/Import/Import", batchRecipe);

                        // var accessor = _serviceProvider.GetService<IHttpContextAccessor>();
                        // var generator = _serviceProvider.GetService<LinkGenerator>();
                        //
                        // var url = generator.GetUriByRouteValues(accessor.HttpContext, "Home", new {});
                        //var url = urlHelper.RouteUrl("Home"); //new { area = "DFC.ServiceTaxonomy.GraphSync", controller = "Home", action = "Index" });

                        return client.PostAsJson("https://127.0.0.1:5001/GraphSync/Index", batchRecipe);



#pragma warning restore S1075
                    });

                await Task.WhenAll(first8Tasks);

                // foreach (ContentItem preparedContentItem in preparedContentItems)
                // {
                //     await CreateContentItem(preparedContentItem, cypherToContent.SyncBackRequired);
                // }

                // batched up async, CreateAsync kabooms: SqlException (0x80131904): Violation of PRIMARY KEY constraint 'PK__Document__3214EC07C3A68634'. Cannot insert duplicate key in object 'dbo.Document'

                ////todo: ratio to cores
                // int CreateContentBatchSize = 8;
                //
                // var contentItemJObjectsBatches = contentItemJObjects
                //     .Batch(CreateContentBatchSize);
                //
                // _logger.LogInformation($"Creating content items in batches of {CreateContentBatchSize}");
                //
                // foreach (IEnumerable<JObject> contentJObjectBatch in contentItemJObjectsBatches)
                // {
                //     var createContentItemTasks = contentJObjectBatch
                //         .Select(jo => CreateContentItem(jo, cypherToContent.SyncBackRequired));
                //
                //     await Task.WhenAll(createContentItemTasks);
                // }

                // batched up created on different threads

                ////todo: ratio to cores
                // int CreateContentBatchSize = 8;
                //
                // var contentItemJObjectsBatches = contentItemJObjects
                //     .Batch(CreateContentBatchSize);
                //
                // _logger.LogInformation($"Creating content items in batches of {CreateContentBatchSize}");
                //
                // foreach (IEnumerable<JObject> contentJObjectBatch in contentItemJObjectsBatches)
                // {
                //     // looks like can't call CreateAsync on non-ui threads
                //     var createContentItemTasks = contentJObjectBatch
                //         .Select(jo => Task.Run(() => CreateContentItem(jo, cypherToContent.SyncBackRequired)));
                //
                //     await Task.WhenAll(createContentItemTasks);
                // }

                // parallel preparation of content items, very slight speed up, but not worth the extra complexity

//
//                 ConcurrentQueue<ContentItem> contentItems = new ConcurrentQueue<ContentItem>();
// //quicker to reuse contentitem? CreateAsync is the bottleneck, and that doesn't seem to work off the ui thread :(
//                 var prepareContentItemTasks = Task.Run(() => Parallel.ForEach(contentItemJObjects, contentItemJObject =>
//                 {
//                     ContentItem? contentItem = PrepareContentItem(contentItemJObject);
//                     if (contentItem != null)
//                         contentItems.Enqueue(contentItem);
//                 }));
//
//                 bool contentItemAvailable;
//                 do
//                 {
//                     contentItemAvailable = contentItems.TryDequeue(out ContentItem? preparedContentItem);
//                     if (contentItemAvailable)
//                         await CreateContentItem(preparedContentItem!, cypherToContent.SyncBackRequired);
//                 } while (!prepareContentItemTasks.IsCompleted || contentItemAvailable);

                // simple 1 at a time, not batched

                // foreach (JObject contentJObject in contentItemJObjects)
                // {
                //     await CreateContentItem(contentJObject, cypherToContent.SyncBackRequired);
                // }

                //todo: log this, but ensure no double enumeration
//                _logger.LogInformation($"Created {contentItemJObjects.Count()} content items in {stopwatch.Elapsed}");
                _logger.LogInformation($"Created content items in {stopwatch.Elapsed}");
            }
        }

        public static string WrapInNonSetupRecipe(string content)
        {
            return $@"{{
  ""name"": """",
  ""displayName"": """",
  ""description"": """",
  ""author"": """",
  ""website"": """",
  ""version"": """",
  ""issetuprecipe"": false,
  ""categories"": """",
  ""tags"": [],
  ""steps"": [
    {{
      ""name"": ""Content"",
      ""data"": [
{content}
      ]
    }}
  ]
}}";
        }

        private ContentItem? PrepareContentItem(JObject contentJObject)
        {
            ContentItem? contentItem = contentJObject.ToObject<ContentItem>();

            if (contentItem?.Content == null)
            {
                _logger.LogWarning("Missing content, unable to import.");
                return null;
            }

            string? title = contentItem.Content.TitlePart?.Title;
            if (title != null)
            {
                contentItem.DisplayText = title;
            }

            contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);
            contentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(contentItem);
            contentItem.Published = contentItem.Latest = true;
            contentItem.CreatedUtc = contentItem.ModifiedUtc = contentItem.PublishedUtc = DateTime.UtcNow;
            contentItem.Owner = contentItem.Author = "admin";

            return contentItem;
        }

        private async Task CreateContentItem(ContentItem contentItem, bool syncBackRequired)
        {
            //todo: could put contenttype in there for extra safety!? overkill?

            // if the cache expires before the sync gets called, that's fine as its only an optimisation
            // to not sync the content item. if it's synced, the graph will still be correct
            // (we're essentially skipping a no-op)
            // what we want to avoid, is _not_ syncing when we should
            // that's why we use ContentItemVersionId, instead of ContentItemId
            if (!syncBackRequired)
            {
                string cacheKey = $"DisableSync_{contentItem.ContentItemVersionId}";
                _memoryCache.Set(cacheKey, contentItem.ContentItemVersionId,
                    new TimeSpan(0, 0, 30));
            }

            //todo: log adding content type + id? how would we (easily) get the contenttype??

            await _contentManager.CreateAsync(contentItem);
        }

        private async Task CreateContentItem(JObject contentJObject, bool syncBackRequired)
        {
            ContentItem? contentItem = contentJObject.ToObject<ContentItem>();

            if (contentItem?.Content == null)
            {
                _logger.LogWarning("Missing content, unable to import.");
                return;
            }

            string? title = contentItem.Content.TitlePart?.Title;
            if (title != null)
            {
                contentItem.DisplayText = title;
            }

            contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);
            contentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(contentItem);
            contentItem.Published = contentItem.Latest = true;
            contentItem.CreatedUtc = contentItem.ModifiedUtc = contentItem.PublishedUtc = DateTime.UtcNow;
            contentItem.Owner = contentItem.Author = "admin";

            //todo: could put contenttype in there for extra safety!? overkill?

            // if the cache expires before the sync gets called, that's fine as its only an optimisation
            // to not sync the content item. if it's synced, the graph will still be correct
            // (we're essentially skipping a no-op)
            // what we want to avoid, is _not_ syncing when we should
            // that's why we use ContentItemVersionId, instead of ContentItemId
            if (!syncBackRequired)
            {
                string cacheKey = $"DisableSync_{contentItem.ContentItemVersionId}";
                _memoryCache.Set(cacheKey, contentItem.ContentItemVersionId,
                    new TimeSpan(0, 0, 30));
            }

            //todo: log adding content type + id? how would we (easily) get the contenttype??

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
            public bool SyncBackRequired { get; set; }
        }

        //todo: setting whether serial or parallel (or parallel by default and multiple recipes for serial) and split label create across queries to speed up import (or wait until 1 recipe to import multiple recipes for parallalisation?)
        public class CypherToContentStepModel
        {
            public CypherToContentModel[]? Queries { get; set; }
        }
    }
}
