using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
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
    /// This recipe step creates a set of content items,
    /// without adding the content item to the ContentManagerSession cache (to keep memory usage down)
    /// or running any content handlers (for speed).
    /// It does however sync the items to the appropriate graphs.
    /// Ideally we should merge this and CSharpContentStep into a single step, but if we did that, most recipes
    /// (that don't require csharp scripting) would pay an unnecessary time and memory cost, so for now, we have
    /// two separate step handlers.
    /// </summary>
    public class ContentNoCacheStep : IRecipeStepHandler
    {
        private readonly ISession _session;
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly ILogger<ContentNoCacheStep> _logger;

        public const string StepName = "ContentNoCache";

        public ContentNoCacheStep(
            ISession session,
            ISyncOrchestrator syncOrchestrator,
            ILogger<ContentNoCacheStep> logger)
        {
            _session = session;
            _syncOrchestrator = syncOrchestrator;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                _logger.LogInformation("Running {StepName} for {RecipeName} recipe.", StepName, context.RecipeDescriptor.Name);

                Stopwatch stopwatch = Stopwatch.StartNew();

                ContentStepModel? model = context.Step.ToObject<ContentStepModel>();
                JArray? data = model?.Data;
                if (data == null)
                    return;

                foreach (JToken? token in data)
                {
                    ContentItem? contentItem = token.ToObject<ContentItem>();
                    if (contentItem == null)
                        continue;

                    // assume item doesn't currently exist (for speed!)

                    _logger.LogInformation("Saving '{DisplayText}' {ContentType} to SQL DB.", contentItem.DisplayText, contentItem.ContentType);
                    _session.Save(contentItem);
                    _logger.LogInformation("Publishing '{DisplayText}' {ContentType} to graphs.", contentItem.DisplayText, contentItem.ContentType);
                    await _syncOrchestrator.Publish(contentItem);
                }

                // needed to fix an object already disposed exception, see
                // https://github.com/OrchardCMS/OrchardCore/issues/3191
                // https://github.com/OrchardCMS/OrchardCore/blob/d4602a736196a46bb2dacf2f370cd71e2b2e5941/src/OrchardCore.Modules/OrchardCore.Contents/Recipes/ContentStep.cs#L53
                // await _session.CommitAsync();

                _logger.LogInformation("Created content items in {TimeTaken}.", stopwatch.Elapsed);

                //todo: should we still collect?
#pragma warning disable S1215
                GC.Collect();
#pragma warning restore S1215
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
