using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class PageLocationsPropertyService : IPropertyService
    {
        private readonly IContentManager _contentManager;
        public PageLocationsPropertyService(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public bool CanProcess(JToken? jToken, string? propertyName = null)
        {
            return propertyName?.Replace(" ", string.Empty).Equals("PageLocations", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }
        public IList<PropertyExtract> Process(string propertyName, JToken? jToken)
        {
            var pageLocationsList = new List<PropertyExtract>();
            var pageLocations = jToken?.ToObject<PageLocations>();
            if (pageLocations != null && !string.IsNullOrWhiteSpace(pageLocations.TaxonomyContentItemId) && pageLocations.TermContentItemIds != null && pageLocations.TermContentItemIds.Any())
            {
                var taxonomy = _contentManager.GetAsync(pageLocations.TaxonomyContentItemId).Result;
                var taxonomyPart = JObject.FromObject(taxonomy)?.GetValue("TaxonomyPart")?.ToObject<TaxonomyPart>();
                if (taxonomyPart == null)
                {
                    return pageLocationsList;
                }
                pageLocationsList.Add(new PropertyExtract
                {
                    Name = propertyName,
                    Links = pageLocations.TermContentItemIds.ToDictionary(k => k, v => taxonomyPart.GetDisplayTextById(v))
                });
            }

            return pageLocationsList;
        }
    }
}
