using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    //todo: look for more ways to keep memory down and speed up the import
    // e.g. we could replicate CreateAsync, and not add the item to the cache and take out what isn't necessary
    /// <summary>
    /// This recipe step creates a set of content items, without adding the content item to the ContentManagerSession cache, to keep memory usage down.
    /// Ideally we should merge this and CSharpContentStep into a single step, but if we did that, most recipes
    /// (that don't require csharp scripting) would pay an unnecessary time and memory cost, so for now, we have
    /// two separate step handlers.
    /// </summary>
    public class ContentNoCacheStep : IRecipeStepHandler
    {
        private readonly ISession _session;
        private readonly ILogger<ContentNoCacheStep> _logger;

        public const string StepName = "ContentNoCache";

        public ContentNoCacheStep(
            ISession session,
            ILogger<ContentNoCacheStep> logger)
        {
            _session = session;
            _logger = logger;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                ContentStepModel? model = context.Step.ToObject<ContentStepModel>();
                JArray? data = model?.Data;
                if (data == null)
                    return Task.CompletedTask;

                foreach (JToken? token in data)
                {
                    ContentItem? contentItem = token.ToObject<ContentItem>();
                    if (contentItem == null)
                        continue;

                    // assume item doesn't currently exist (for speed!)

                    _session.Save(contentItem);

                    // DateTime? modifiedUtc = contentItem.ModifiedUtc;
                    // DateTime? publishedUtc = contentItem.PublishedUtc;
                    // ContentItem? existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);
                    //
                    // if (existing == null)
                    // {
                    //     // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    //     contentItem.Id = 0;
                    //     await _contentManager.CreateAsync(contentItem);
                    //     _contentManagerSession.Clear();
                    //
                    //     // Overwrite ModifiedUtc & PublishedUtc values that handlers have changes
                    //     // Should not be necessary if IContentManager had an Import method
                    //     contentItem.ModifiedUtc = modifiedUtc;
                    //     contentItem.PublishedUtc = publishedUtc;
                    // }
                    // else
                    // {
                    //     // Replaces the id to force the current item to be updated
                    //     existing.Id = contentItem.Id;
                    //     _session.Save(existing);
                    // }
                }

                _logger.LogInformation("Created content items in {TimeTaken}.", stopwatch.Elapsed);

#pragma warning disable S1215
                GC.Collect();
#pragma warning restore S1215

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ContentNoCacheStep execute exception.");
                throw;
            }
        }
    }

    public class ContentStepModel
    {
        public JArray? Data { get; set; }
    }
}
