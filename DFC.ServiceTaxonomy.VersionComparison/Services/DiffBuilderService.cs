using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public class DiffBuilderService : IDiffBuilderService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IPropertyService> _propertyServices;
        // We could look at creating a setting for this module to handle the parts black list but not a short piece of work
        private readonly List<string> partsBlackList = new List<string>
        {
            nameof(AuditTrailPart),
            "GraphSyncPart",
            "ContentApprovalPart"
        };

        public DiffBuilderService(IContentDefinitionManager contentDefinitionManager, IEnumerable<IPropertyService> propertyServices)
        {
            _propertyServices = propertyServices;
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
            var partsLIst = contentType.Parts
                .Select(p => p)
                .Where(p => partsBlackList.All(bl => !bl.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();
            var baseJObject = JObject.FromObject(baseVersion);
            var compareJObject = JObject.FromObject(compareVersion);

            var basePartDictionary = new Dictionary<string, PropertyExtract>();
            var comparePartDictionary = new Dictionary<string, PropertyExtract>();
            foreach (var contentTypePartDefinition in partsLIst)
            {
                LoadDictionaries(basePartDictionary, contentTypePartDefinition, baseJObject);
                LoadDictionaries(comparePartDictionary, contentTypePartDefinition, compareJObject);
            }

            return MergeDictionaries(basePartDictionary, comparePartDictionary);
        }

        private void LoadDictionaries(Dictionary<string, PropertyExtract> dictionary,
            ContentTypePartDefinition? partDefinition, JObject? jObject)
        {
            var basePart = jObject?.GetValue(partDefinition?.Name) as JObject;
            if (basePart != null)
            {
                LoadPropertyValues(basePart, dictionary, partDefinition);
            }
        }

        private List<DiffItem> MergeDictionaries(Dictionary<string, PropertyExtract> basePartDictionary,
            Dictionary<string, PropertyExtract> comparePartDictionary)
        {
            var diffList = new List<DiffItem>();
            var keys = basePartDictionary.Keys.Union(comparePartDictionary.Keys);
            foreach (string key in keys)
            {
                var basePartContainsKey = basePartDictionary.ContainsKey(key);
                var comparePartContainsKey = comparePartDictionary.ContainsKey(key);
                diffList.Add(new DiffItem{
                    Name = basePartContainsKey ? basePartDictionary[key].Name : comparePartDictionary[key].Name,
                    BaseItem = basePartContainsKey ? basePartDictionary[key].Value : string.Empty,
                    BaseURLs = basePartContainsKey ? basePartDictionary[key].Links : null,
                    CompareItem = comparePartContainsKey ? comparePartDictionary[key].Value : string.Empty,
                    CompareURLs = comparePartContainsKey ? comparePartDictionary[key].Links : null
                });
            }
            return diffList;
        }

        private void LoadPropertyValues(JObject part, Dictionary<string, PropertyExtract> propertyDictionary, ContentTypePartDefinition? partDefinition)
        {
            var fieldArray = partDefinition?.PartDefinition.Fields.ToArray() ?? Array.Empty<ContentPartFieldDefinition>();
            var fieldLookUp = fieldArray.ToDictionary(k => k.Name, v => v.DisplayName());

            foreach (JProperty jProperty in part.Properties())
            {

                var propertyName = jProperty.Name;
                if (fieldLookUp.ContainsKey(propertyName))
                {
                    propertyName = fieldLookUp[propertyName];
                }
                var propertyKey = ValidDictionaryKey(propertyDictionary, jProperty.Name);
                var partProperty = part.GetValue(jProperty.Name);

                var propertyService = _propertyServices.FirstOrDefault(ps => ps.CanProcess(partProperty, propertyName));
                if (propertyService != null)
                {
                    var properties = propertyService.Process(propertyName, partProperty);
                    foreach (PropertyExtract? propertyDto in properties)
                    {
                        propertyDictionary.Add(propertyDto.Key ?? propertyKey, propertyDto);
                    }
                }
                else if (partProperty != null && partProperty.Type == JTokenType.Object) // Objects with multiple values
                {
                    foreach (JToken child in partProperty.Children())
                    {
                        if (child.Type == JTokenType.Object)
                        {
                            LoadPropertyValues((JObject)child, propertyDictionary, partDefinition);
                        }
                        else if (child.Type == JTokenType.Property)
                        {
                            var childProperty = child as JProperty;
                            var childPropertyName = $"{propertyKey}-{childProperty?.Name}";
                            propertyDictionary.Add(childPropertyName, new PropertyExtract { Name = childPropertyName, Value = childProperty?.Value.ToString() });
                        }
                    }
                }
            }
        }

        private string ValidDictionaryKey(Dictionary<string, PropertyExtract> propertyDictionary, string key)
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
