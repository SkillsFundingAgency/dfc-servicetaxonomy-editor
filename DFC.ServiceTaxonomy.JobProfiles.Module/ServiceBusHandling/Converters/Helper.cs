using System.Collections.Generic;
using System.Linq;
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
        public static List<ContentItem> GetContentItems(dynamic contentPicker, IContentManager contentManager)
        {
            if(contentPicker != null)
            {
                var contentItemIds = (JArray)contentPicker.ContentItemIds;
                if (contentItemIds.Any())
                {
                    var idList = contentItemIds.Select(c => c.Value<string>()).ToList();
                    var contentItems = contentManager.GetAsync(idList).Result;
                    return contentItems.ToList();
                }
            }

            return new List<ContentItem>();
        }

        public static IEnumerable<Classification> MapClassificationData(List<ContentItem> contentItems)
        {
            var classificationData = new List<Classification>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    classificationData.Add(new Classification
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<TitlePart>().Title,
                        Description = GetClassificationDescriptionText(contentItem)
                    });
                }
            }

            return classificationData;
        }

        private static string GetClassificationDescriptionText(ContentItem contentItem)
        {
            switch (contentItem.ContentType)
            {
                case ContentTypes.HiddenAlternativeTitle:
                    return contentItem.Content.HiddenAlternativeTitle.Description.Text;
                case ContentTypes.JobProfileSpecialism:
                    return contentItem.Content.JobProfileSpecialism.Description.Text;
                case ContentTypes.Workinghoursdetail:
                    return contentItem.Content.Workinghoursdetail.Description.Text;
                case ContentTypes.Workingpatterns:
                    return contentItem.Content.Workingpatterns.Description.Text;
                case ContentTypes.Workingpatterndetail:
                    return contentItem.Content.Workingpatterndetail.Description.Text;
                default: return string.Empty;
            }
        }

    }
}
