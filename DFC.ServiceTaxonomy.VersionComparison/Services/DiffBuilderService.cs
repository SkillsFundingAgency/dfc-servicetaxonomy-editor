using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices;
using Json.More;
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

            var contentType = _contentDefinitionManager.GetTypeDefinitionAsync(baseVersion.ContentType).Result;
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
            ContentTypePartDefinition? partDefinition, JsonObject? jObject)
        {
            var basePart = jObject?[partDefinition!.Name!]!.GetValue<JsonObject>();
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

        private void LoadPropertyValues(JsonObject part, Dictionary<string, PropertyExtract> propertyDictionary, ContentTypePartDefinition? partDefinition)
        {
            var fieldArray = partDefinition?.PartDefinition.Fields.ToArray() ?? Array.Empty<ContentPartFieldDefinition>();
            var fieldLookUp = fieldArray.ToDictionary(k => k.Name, v => v.DisplayName());


            foreach (var jProperty in part)
            {

                var propertyName = jProperty.Key;
                if (fieldLookUp.ContainsKey(propertyName))
                {
                    propertyName = fieldLookUp[propertyName];
                }
                var propertyKey = ValidDictionaryKey(propertyDictionary, jProperty.Key);
                var partProperty = part[jProperty.Key]!.GetValue<JsonElement?>(); 

                var propertyService = _propertyServices.FirstOrDefault(ps => ps.CanProcess(partProperty, propertyName));
                if (propertyService != null)
                {
                    var properties = propertyService.Process(propertyName, partProperty);
                    foreach (PropertyExtract? propertyDto in properties)
                    {
                        propertyDictionary.Add(propertyDto.Key ?? propertyKey, propertyDto);
                    }
                }
                else if (partProperty != null && partProperty.GetValueOrDefault().ValueKind == JsonValueKind.Object) 
                {
                    foreach (var child in partProperty.GetValueOrDefault().EnumerateObject())
                    {
                        if (child.Value.ValueKind == JsonValueKind.Object)
                        {
                            LoadPropertyValues(JObject.Parse(child.Value.ToJsonString())!, propertyDictionary, partDefinition);
                        }
                        else 
                        {
                            var childPropertyName = $"{propertyKey}-{child.Name}";
                            propertyDictionary.Add(childPropertyName, new PropertyExtract { Name = childPropertyName, Value = child.Value.ToString() });
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
