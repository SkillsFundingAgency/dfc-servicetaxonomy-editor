using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataAccess.Repositories;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using DFC.ServiceTaxonomy.Title.Indexes;
using DFC.ServiceTaxonomy.Title.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.Title.Handlers
{
    public class ContentItemImportHandler : IRecipeStepHandler
    {
        private readonly IGenericIndexRepository<UniqueTitlePartIndex> _uniqueTitleIndexRepository;
        private readonly ILogger<ContentItemImportHandler> _logger;

        public ContentItemImportHandler(
            IGenericIndexRepository<UniqueTitlePartIndex> uniqueTitleIndexRepository,
            ILogger<ContentItemImportHandler> logger)
        {
            _uniqueTitleIndexRepository = uniqueTitleIndexRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            ContentStepModel model = context.Step.ToObject<ContentStepModel>();
            int numberOfDuplicates = 0;
          
            JArray data = model?.Data;
            if (data == null)
                return;

            _logger.LogInformation($"ExecuteAsync data {data}");

            foreach (JToken token in data)
            {
                ContentItem contentItem = token.ToObject<ContentItem>();
                if (contentItem == null)
                    continue;

                var part = contentItem.As<UniqueTitlePart>();
                if (part != null)
                {
                    var matches = await _uniqueTitleIndexRepository.GetCount(b =>
                            b.Title == part.Title && b.ContentItemId != part.ContentItem.ContentItemId && b.ContentType == part.ContentItem.ContentType);

                    _logger.LogInformation($"ExecuteAsync UniqueTitlePart {part.Title}"); 
                    if (matches > 0)
                    {
                        numberOfDuplicates += 1;
                        _logger.LogError(new Exception($"Duplicate No. {numberOfDuplicates} : Failed to import data for item with ContentItemId = {contentItem.ContentItemId} and Title = {part.Title}"), "Duplicate Content Item Exception");

                    }
                }
            }

            if(numberOfDuplicates > 0)
            {
                _logger.LogError($"Total Number of Duplicate items = {numberOfDuplicates} ");
                throw new ArgumentException("The Title is already in use on another content item");
            }

        }
    }



}
