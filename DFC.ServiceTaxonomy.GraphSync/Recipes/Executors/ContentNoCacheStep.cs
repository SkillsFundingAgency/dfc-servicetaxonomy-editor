using System;
using System.Threading.Tasks;
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
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IContentManagerSession _contentManagerSession;

        public const string StepName = "ContentNoCache";

        public ContentNoCacheStep(
            IContentManager contentManager,
            ISession session,
            IContentManagerSession contentManagerSession)
        {
            _contentManager = contentManager;
            _session = session;
            _contentManagerSession = contentManagerSession;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ContentStepModel? model = context.Step.ToObject<ContentStepModel>();
            JArray? data = model?.Data;
            if (data == null)
                return;

            foreach (JToken? token in data)
            {
                ContentItem? contentItem = token.ToObject<ContentItem>();
                if (contentItem == null)
                    continue;

                DateTime? modifiedUtc = contentItem.ModifiedUtc;
                DateTime? publishedUtc = contentItem.PublishedUtc;
                ContentItem? existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);

                if (existing == null)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;
                    await _contentManager.CreateAsync(contentItem);
                    _contentManagerSession.Clear();

                    // Overwrite ModifiedUtc & PublishedUtc values that handlers have changes
                    // Should not be necessary if IContentManager had an Import method
                    contentItem.ModifiedUtc = modifiedUtc;
                    contentItem.PublishedUtc = publishedUtc;
                }
                else
                {
                    // Replaces the id to force the current item to be updated
                    existing.Id = contentItem.Id;
                    _session.Save(existing);
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JArray? Data { get; set; }
    }
}
