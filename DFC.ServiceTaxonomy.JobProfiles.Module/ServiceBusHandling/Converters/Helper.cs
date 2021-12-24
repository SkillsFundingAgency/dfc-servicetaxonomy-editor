using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;

using Newtonsoft.Json.Linq;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public static class Helper
    {
        public static async Task<IEnumerable<ContentItem>> GetContentItemsAsync(dynamic contentPicker, IContentManager contentManager)
        {
            if (contentPicker != null)
            {
                var contentItemIds = (JArray)contentPicker.ContentItemIds;
                if (contentItemIds.Any())
                {
                    var idList = contentItemIds.Select(c => c.Value<string>());
                    var contentItems = await contentManager.GetAsync(idList);
                    return contentItems;
                }
            }

            return Enumerable.Empty<ContentItem>();
        }

        public static IEnumerable<Classification> MapClassificationData(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new Classification
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Description = GetClassificationDescriptionText(contentItem)
            }) ?? Enumerable.Empty<Classification>();

        private static string GetClassificationDescriptionText(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.HiddenAlternativeTitle => contentItem.Content.HiddenAlternativeTitle.Description.Text,
                ContentTypes.JobProfileSpecialism => contentItem.Content.JobProfileSpecialism.Description.Text,
                ContentTypes.Workinghoursdetail => contentItem.Content.Workinghoursdetail.Description.Text,
                ContentTypes.Workingpatterns => contentItem.Content.Workingpatterns.Description.Text,
                ContentTypes.Workingpatterndetail => contentItem.Content.Workingpatterndetail.Description.Text,
                _ => string.Empty,
            };

    }
}
