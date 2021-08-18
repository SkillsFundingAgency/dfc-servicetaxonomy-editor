using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class BannerContentPickerResultProvider : IBannerContentPickerResultProvider
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;

        public BannerContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
        }

        public string Name => "Banner";

        public async Task<IEnumerable<BannerContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            IEnumerable<string>? contentTypes = searchContext.ContentTypes;
            if (searchContext.DisplayAllContentTypes)
            {
                contentTypes = _contentDefinitionManager
                                .ListTypeDefinitions()
                                .Where(x => string.IsNullOrEmpty(x.GetSettings<ContentTypeSettings>().Stereotype))
                                .Select(x => x.Name)
                                .AsEnumerable();
            }

            IQuery<ContentItem, ContentItemIndex>? query = _session.Query<ContentItem, ContentItemIndex>()
                                                           .With<ContentItemIndex>(x => x.ContentType.IsIn(contentTypes) && x.Latest);

            if (!string.IsNullOrEmpty(searchContext.Query))
            {
                query.With<ContentItemIndex>(x => x.DisplayText.Contains(searchContext.Query) || x.ContentType.Contains(searchContext.Query));
            }

            IEnumerable<ContentItem>? contentItems = await query.Take(50).ListAsync();

            var results = new List<BannerContentPickerResult>();

            foreach (ContentItem? contentItem in contentItems)
            {
                results.Add(new BannerContentPickerResult
                {
                    ContentItemId = contentItem.ContentItemId,
                    DisplayText = contentItem.ToString(),
                    HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem),
                    IsActive = contentItem.Content.Banner.IsActive.Value
                });
            }

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
