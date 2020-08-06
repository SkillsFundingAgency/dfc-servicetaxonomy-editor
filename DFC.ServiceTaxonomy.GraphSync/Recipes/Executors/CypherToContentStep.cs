using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    //todo: we need to speed up import
    // as the session isn't thread safe within a request, we could send requests to ourselves
    // we might be able to avoid going over the loopback address
    // (see ProcessRequestAsync? https://stackoverflow.com/questions/50407760/programmatically-invoking-the-asp-net-core-request-pipeline)
    // if we have to go via loopback, we'll have to set authentication headers and anti forgery token
    // and keep n batches processing at once

    public class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphCluster _graphCluster;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";

        public CypherToContentStep(
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentManagerSession contentManagerSession,
            IContentItemIdGenerator idGenerator,
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals,
            IMemoryCache memoryCache,
            ILogger<CypherToContentStep> logger)
        {
            _graphCluster = graphCluster;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _contentManagerSession = contentManagerSession;
            _idGenerator = idGenerator;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        //todo: need to add validation, at least to detect when import same thing twice!
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                var step = context.Step.ToObject<CypherToContentStepModel>();

                foreach (CypherToContentModel? cypherToContent in step!.Queries ?? Enumerable.Empty<CypherToContentModel?>())
                {
                    if (cypherToContent == null)
                        continue;

                    _logger.LogInformation($"{StepName} step");

                    var getContentItemsAsJsonQuery = _serviceProvider.GetRequiredService<IGetContentItemsAsJsonQuery>();

                    getContentItemsAsJsonQuery.QueryStatement = cypherToContent.Query;

                    _logger.LogInformation($"Executing query to retrieve content for items:\r\n{cypherToContent.Query}");

                    // for now, populate from the published graph
                    // we _may_ want to introduce support for creating draft items from the draft replica set at some point
                    List<string> contentItemsJsonPreTokenization = await _graphCluster.Run(GraphReplicaSetNames.Published, getContentItemsAsJsonQuery);

                    IEnumerable<string> contentItemsJson = contentItemsJsonPreTokenization.Select(ReplaceCSharpHelpers);

                    var contentItemJObjects = contentItemsJson
                        .Select(JsonConvert.DeserializeObject<List<JObject>>)
                        .SelectMany(cijo => cijo);

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    var preparedContentItems = contentItemJObjects
                        .Select(PrepareContentItem)
                        .Where(i => i != null)
                        .Select(i => i!);

                    foreach (ContentItem preparedContentItem in preparedContentItems)
                    {
                        await CreateContentItem(preparedContentItem, cypherToContent.SyncBackRequired);
                    }

                    //todo: log this, but ensure no double enumeration
                    // _logger.LogInformation($"Created {contentItemJObjects.Count()} content items in {stopwatch.Elapsed}");
                    _logger.LogInformation($"Created content items in {stopwatch.Elapsed}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Exception: {ex}");
                throw;
            }
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
            _contentManagerSession.Clear();
        }

        private static readonly Regex _cSharpHelperRegex = new Regex(@"\[c#:([^\]]+)\]", RegexOptions.Compiled);
        private string ReplaceCSharpHelpers(string recipeFragment)
        {
            string processedRecipeFragment = _cSharpHelperRegex.Replace(recipeFragment, match => EvaluateCSharp(match.Groups[1].Value).GetAwaiter().GetResult());

#pragma warning disable S1215
            GC.Collect();
#pragma warning restore S1215

            return processedRecipeFragment;
        }

        private static readonly Regex _getContentItemIdByDisplayTextRegex = new Regex(@"^\s*await\s*Content.GetContentItemIdByDisplayText\s*\(\s*""([^""]+)""\s*,\s*""([^""]+)""\s*\)\s*$", RegexOptions.Compiled);
        private async Task<string> EvaluateCSharp(string code)
        {
            // can't see how to get json.net to unescape value strings!
            code = code.Replace("\\\"", "\"");

            // memory optimisation
            Match match = _getContentItemIdByDisplayTextRegex.Match(code);
            if (match.Success)
            {
                return await _cypherToContentCSharpScriptGlobals.Content.GetContentItemIdByDisplayText(match.Groups[1].Value, match.Groups[2].Value);
            }

            var script = CSharpScript.Create<string>(code, globalsType: typeof(ICypherToContentCSharpScriptGlobals));
            ScriptRunner<string> runner = script.CreateDelegate();
            return await runner(_cypherToContentCSharpScriptGlobals);
        }

        public class CypherToContentModel
        {
            public string? Query { get; set; }
            public bool SyncBackRequired { get; set; }
        }

        public class CypherToContentStepModel
        {
            public CypherToContentModel[]? Queries { get; set; }
        }
    }
}
