using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IContentManager _contentManager;

        public DiffBuilderService(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public List<DiffItem> BuildDiffList(ContentItem baseVersion, ContentItem compareVersion)
        {
            if (baseVersion.ContentType != compareVersion.ContentType)
            {
                throw new ArgumentException(
                    $"The base content type '{baseVersion.ContentType}' is different to the compare content type '{compareVersion.ContentType}'");
            }

            var contentType = _contentDefinitionManager.GetTypeDefinition(baseVersion.ContentType);
            var partsLIst = contentType.Parts.Select(p => p).ToList();
            var baseJObject = JObject.FromObject(baseVersion);
            var compareJObject = JObject.FromObject(compareVersion);

            var basePartDictionary = new Dictionary<string, PropertyDto>();
            var comparePartDictionary = new Dictionary<string, PropertyDto>();
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

        private List<DiffItem> MergeDictionaries(Dictionary<string, PropertyDto> basePartDictionary,
            Dictionary<string, PropertyDto> comparePartDictionary)
        {
            var diffList = new List<DiffItem>();
            var keys = basePartDictionary.Keys.Union(comparePartDictionary.Keys);
            foreach (string key in keys)
            {
                diffList.Add(new DiffItem{
                    Name = basePartDictionary.ContainsKey(key) ? basePartDictionary[key].Name : comparePartDictionary[key].Name,
                    BaseItem = basePartDictionary.ContainsKey(key) ? basePartDictionary[key].Value : string.Empty,
                    BaseURLs = basePartDictionary.ContainsKey(key) ? basePartDictionary[key].Links : null,
                    CompareItem = comparePartDictionary.ContainsKey(key) ? comparePartDictionary[key].Value : string.Empty,
                    CompareURLs = comparePartDictionary.ContainsKey(key) ? comparePartDictionary[key].Links : null
                });
            }
            return diffList;
        }

        private void LoadPropertyValues(JObject part, Dictionary<string, PropertyDto> propertyDictionary)
        {
            foreach (JProperty jProperty in part.Properties())
            {
                // TODO: I think some kind of rules engine would be better suited to extracting the differing property types
                var propertyName = ValidDictionaryKey(propertyDictionary, jProperty.Name);
                var partProperty = part.GetValue(jProperty.Name);

                if (partProperty == null) // Handle nulls gracefully
                {
                    propertyDictionary.Add(propertyName, new PropertyDto { Name = propertyName});
                }
                else if (partProperty.Type != JTokenType.Object && partProperty.Type != JTokenType.Array) // Basic properties on the part
                {
                    var objectValue = new ObjectValue {Value = partProperty.ToString()};
                    propertyDictionary.Add(propertyName, new PropertyDto { Name = jProperty.Name, Value = objectValue.ToString()});
                } 
                else if(partProperty.Type == JTokenType.Array && jProperty.Name == "Widgets") // Specifically for the FlowPart
                {
                    // We need to go further down the rabbit hole
                    var widgets = partProperty.Select(w => w.ToObject<Widget>()).ToList();
                    foreach (var widget in widgets)
                    {
                        if (widget == null)
                        {
                            continue;
                        }
                        propertyName = $"{widget.ContentType}-{widget.ContentItemId}";
                        if (widget.ContentType == "HTMLShared")
                        {
                            var linkInfo = widget.HTMLShared?.SharedContent?.ContentItemIds
                                .Select(c => new {Id = c, Name = GetContentName(c).Result})
                                .ToDictionary(k => k.Id, v => v.Name);
                            propertyDictionary.Add(propertyName, new PropertyDto { Name = "HTMLShared", Links = linkInfo});
                        }
                        else
                        {
                            propertyDictionary.Add(propertyName, new PropertyDto { Name = "HTML", Value = widget?.HtmlBodyPart?.Html ?? string.Empty});
                        }
                    }
                }
                else if (partProperty.Type == JTokenType.Object && partProperty.Children().Count() == 1) // Properties exposed a JObjects with a single value
                {
                    var objectValue = partProperty.ToObject<ObjectValue>();
                    if (objectValue != null)
                    {
                        propertyDictionary.Add(propertyName, new PropertyDto { Name = jProperty.Name, Value = objectValue.ToString() });
                    }
                }
                else if (partProperty.Type == JTokenType.Object) // Objects with multiple values
                {
                    foreach (JToken child in partProperty.Children())
                    {
                        if (child.Type == JTokenType.Object)
                        {
                            LoadPropertyValues((JObject)child, propertyDictionary);
                        }
                        else if(child.Type == JTokenType.Property)
                        {
                            var childProperty = child as JProperty;
                            var childPropertyName = $"{propertyName}-{childProperty?.Name}";
                            propertyDictionary.Add(childPropertyName, new PropertyDto{Name = childPropertyName, Value = childProperty?.Value.ToString()});
                        }
                    }
                }
            }
        }

        private async Task<string> GetContentName(string contentItemId)
        {
            if (string.IsNullOrWhiteSpace(contentItemId))
            {
                return string.Empty;
            }
            var contentItem = await _contentManager.GetAsync(contentItemId);
            if (contentItem != null)
            {
                return contentItem.DisplayText;
            }

            return string.Empty;
        }

        private string ValidDictionaryKey(Dictionary<string, PropertyDto> propertyDictionary, string key)
        {
            var i = 1;
            while (propertyDictionary.ContainsKey(key))
            {
                key = $"{key}{i++}";
            }

            return key;
        }
    }
}
