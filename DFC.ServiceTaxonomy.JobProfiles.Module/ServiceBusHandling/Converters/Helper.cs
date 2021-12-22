using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;


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
    }
}
