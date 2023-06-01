using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class EditPublishedLinkContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger<EditPublishedLinkContentPickerResultProvider> _logger;

        public EditPublishedLinkContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session, ILogger<EditPublishedLinkContentPickerResultProvider> logger)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _logger = logger;
        }

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            _logger.LogInformation($"Search: searchContext {searchContext}");
            var contentTypes = searchContext.ContentTypes;
            if (searchContext.DisplayAllContentTypes)
            {
                contentTypes = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(x => string.IsNullOrEmpty(x.GetSettings<ContentTypeSettings>().Stereotype))
                    .Select(x => x.Name)
                    .AsEnumerable();
            }

            var query = _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(contentTypes) && x.Published);

            if (!string.IsNullOrEmpty(searchContext.Query))
            {
                query.With<ContentItemIndex>(x => x.DisplayText.Contains(searchContext.Query) || x.ContentType.Contains(searchContext.Query));
            }
            _logger.LogInformation($"Search: query {query}"); 
            var contentItems = await query.Take(50).ListAsync();

            var results = new List<ContentPickerResult>();
            _logger.LogInformation($"Search: results {results}");
            foreach (var contentItem in contentItems)
            {
                results.Add(new ContentPickerResult
                {
                    ContentItemId = contentItem.ContentItemId,
                    DisplayText = contentItem.ToString(),
                    HasPublished = true
                });
            }

            return results.OrderBy(x => x.DisplayText);
        }

        public string Name => "EditPublishedLink";
    }
}
