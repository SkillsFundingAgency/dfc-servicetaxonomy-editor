using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public class DiffBuilderService : IDiffBuilderService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DiffBuilderService(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public List<DiffItem> BuildDiffList(ContentItem baseVersion, ContentItem compareVersion)
        {
            if (baseVersion.ContentType != compareVersion.ContentType)
            {
                throw new ArgumentException(
                    $"The base content type '{baseVersion.ContentType}' is different to the compare content tyep '{compareVersion.ContentType}'");
            }

            var contentType = _contentDefinitionManager.GetTypeDefinition(baseVersion.ContentType);
            var partsLIst = contentType.Parts.Select(p => p).ToList();
            var baseJObject = JObject.FromObject(baseVersion);
            var compareJObject = JObject.FromObject(compareVersion);

            var basePartDictionary = new Dictionary<string, string>();
            var comparePartDictionary = new Dictionary<string, string>();
            foreach (var contentTypePartDefinition in partsLIst)
            {
                var basePart = baseJObject.GetValue(contentTypePartDefinition.Name) as JObject;
                if (basePart != null)
                {
                    LoadPropertyValues(basePart, basePartDictionary);
                }
                var comparePart = compareJObject.GetValue(contentTypePartDefinition.Name) as JObject;
                if (comparePart != null)
                {
                    LoadPropertyValues(comparePart, comparePartDictionary);
                }
            }

            return MergeDictionaries(basePartDictionary, comparePartDictionary);

        }

        private List<DiffItem> MergeDictionaries(Dictionary<string, string> basePartDictionary,
            Dictionary<string, string> comparePartDictionary)
        {
            var diffList = new List<DiffItem>();
            var keys = basePartDictionary.Keys.Union(comparePartDictionary.Keys);
            foreach (string key in keys)
            {
                diffList.Add(new DiffItem{
                    Name = key,
                    BaseItem = basePartDictionary.ContainsKey(key) ? basePartDictionary[key] : string.Empty,
                    CompareItem = comparePartDictionary.ContainsKey(key) ? comparePartDictionary[key] : string.Empty
                });
            }

            return diffList;
        }

        private void LoadPropertyValues(JObject part, Dictionary<string, string> propertyDictionary)
        {
            foreach (JProperty jProperty in part.Properties())
            {
                var propertyName = ValidDictionaryKey(propertyDictionary, jProperty.Name);
                var partProperty = part.GetValue(jProperty.Name);

                if (partProperty == null)
                {
                    propertyDictionary.Add(propertyName, string.Empty);
                }
                else if (partProperty.Type != JTokenType.Object && partProperty.Type != JTokenType.Array)
                {
                    propertyDictionary.Add(propertyName, partProperty.ToString());
                }
                else if(partProperty.Type == JTokenType.Array && jProperty.Name == "Widgets")
                {
                    // We need to go further down the rabbit hole
                    var widgets = partProperty.Select(w => w.ToObject<Widget>()).ToList();
                    foreach (var widget in widgets.Where(w => w != null))
                    {
                        propertyName = ValidDictionaryKey(propertyDictionary, widget?.ContentType ?? string.Empty);
                        if (widget?.ContentType == "HTMLShared")
                        {
                            propertyDictionary.Add(propertyName, string.Join(", ",widget.HTMLShared?.SharedContent?.ContentItemIds ?? Array.Empty<string>()));
                        }
                        else
                        {
                            propertyDictionary.Add(propertyName, widget?.HtmlBodyPart?.Html ?? string.Empty);
                        }
                    }
                }
            }
        }

        private string ValidDictionaryKey(Dictionary<string, string> propertyDictionary, string key)
        {
            var i = 1;
            while (propertyDictionary.ContainsKey(key))
            {
                key = $"{key}{i++}";
            }

            return key;
        }

        private List<DiffItem> GetPropertyValues(JObject? basePart, JObject? comparePart)
        {
            var partDiff = new List<DiffItem>();
            var properties = basePart?.Properties() ?? new List<JProperty>();

            foreach (var property in properties)
            {
                var baseProperty = basePart?.GetValue(property.Name);
                var compareProperty = comparePart?.GetValue(property.Name);
                if (baseProperty == null && compareProperty == null)
                {
                    continue;
                }
                if (baseProperty?.Type != JTokenType.Object)
                {
                    partDiff.Add(new DiffItem
                    {
                        BaseItem = baseProperty?.ToString(),
                        CompareItem = compareProperty?.ToString(),
                        Name = property.Name
                    });
                }
                else 
                {
                    baseProperty = baseProperty?.First;
                    compareProperty = compareProperty?.First;
                    partDiff.Add(new DiffItem
                    {
                        BaseItem = baseProperty?.ToString(),
                        CompareItem = compareProperty?.ToString(),
                        Name = property.Name
                    });
                }
            }

            return partDiff;
        }
    }
}
