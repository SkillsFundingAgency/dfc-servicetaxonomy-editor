using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    public class CSharpContentStep : IRecipeStepHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public CSharpContentStep(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "CSharpContent", StringComparison.OrdinalIgnoreCase))
                return;

            //todo: logging & error handling
            CSharpContentStepModel? model = context.Step.ToObject<CSharpContentStepModel>();
            if (model?.Data == null)
                return;

            foreach (JToken token in model.Data)
            {
                ContentItem? contentItem = token.ToObject<ContentItem>();
                if (contentItem == null)
                    continue;

                DateTime? modifiedUtc = contentItem.ModifiedUtc;
                DateTime? publishedUtc = contentItem.PublishedUtc;
                ContentItem existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);

                if (existing == null)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;
                    await _contentManager.CreateAsync(contentItem);

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

        public class CSharpContentStepModel
        {
            public JArray? Data { get; set; }
        }
    }
}
