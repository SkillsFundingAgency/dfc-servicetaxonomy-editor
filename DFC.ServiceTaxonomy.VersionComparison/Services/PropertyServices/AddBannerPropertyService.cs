using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class AddBannerPropertyService : IPropertyService
    {
        private readonly IContentNameService _contentServiceHelper;

        public AddBannerPropertyService(IContentNameService contentServiceHelper)
        {
            _contentServiceHelper = contentServiceHelper;
        }

        public bool CanProcess(JsonElement? jElement, string? propertyName = null)
        {
            return propertyName?.Replace(" ", string.Empty).Equals("Addabanner", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        public IList<PropertyExtract> Process(string propertyName, JsonElement? jElement)
        {
            var properties = new List<PropertyExtract>();
            var addBanner = jElement?.Deserialize<AddBanner>();
            if (addBanner?.ContentItemIds?.Any() ?? false)
            {
                var linkInfo = addBanner.ContentItemIds.Select(c => new {Id = c, Name = _contentServiceHelper.GetContentNameAsync(c).Result})
                    .ToDictionary(k => k.Id, v => v.Name);
                properties.Add(new PropertyExtract { Name = propertyName, Links = linkInfo});
            }

            return properties;
        }
    }
}
