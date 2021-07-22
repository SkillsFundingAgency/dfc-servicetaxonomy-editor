using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.VersionCompare.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Entities;
using YesSql;

namespace DFC.ServiceTaxonomy.VersionCompare.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private const string IndexView = "IndexAlt";
        public AdminController(ISession session, IContentManager contentManager)
        {
            _contentManager = contentManager;
            _session = session;
        }
        public async Task<ActionResult> Index(string contentId)
        {
            var versionList = await GetAllVersions(contentId);
            var versions = versionList.ToDictionary(k => k.ContentItem.ContentItemVersionId,
                v => v.VersionNumber.ToString());
            var firstContentItem = versionList[0];
            var secondContentItem = versionList.Count > 1 ? versionList[1] : firstContentItem;
            return View(IndexView, BuildViewModel(firstContentItem, secondContentItem, versions));
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPost(string contentId, VersionComparisonViewModel model)
        {
            var versionList = await GetAllVersions(contentId);
            var versions = versionList.ToDictionary(k => k.ContentItem.ContentItemVersionId,
                v => v.VersionNumber.ToString());
            var firstContentItem = versionList.First(v => v.ContentItem.ContentItemVersionId == model.BaseVersionId);
            var secondContentItem = versionList.First(v => v.ContentItem.ContentItemVersionId == model.CompareVersionId);
            return View(IndexView, BuildViewModel(firstContentItem, secondContentItem, versions, model.SelectedProperty));
        }

        private readonly List<string>  defaultPropertyNames = new List<string>
        {
            "TitlePart",
            "DisplayName",
            "FlowPart"
        };


        private JObject BuildDiffObject(string contentType, JObject source)
        {
            var diffObject = new JObject();
            foreach (string propertyName in defaultPropertyNames)
            {
                if (source.TryGetValue(propertyName, StringComparison.InvariantCultureIgnoreCase, out JToken? token))
                {
                    diffObject.Add(propertyName, token);
                }
            }

            var contentTypeProperty = source.GetValue(contentType) as JObject;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var property in contentTypeProperty?.Properties())
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            {
                diffObject.Add(contentType + "." + property.Name, property.Value);
            }

            return diffObject;
        }

        private void SetCompareProperties(JProperty? baseProperty, JProperty? compareProperty, PropertyDiffViewModel model)
        {
            model.BaseValue = baseProperty?.Value.ToString();
            model.CompareValue = compareProperty?.Value.ToString();
            model.LanguageType = baseProperty?.Name == "Html" ? LanguageType.Html : LanguageType.Text;
        }

        private List<PropertyDiffViewModel> GetDiffViewModel(JObject? baseObject, JObject? compareObject)
        {
            var list = new List<PropertyDiffViewModel>();
            if (baseObject == null)
            {
                return list;
            }
            foreach (var property in baseObject.Properties())
            {
                if (property.Name == "FlowPart")
                {
                    var compartWidget = compareObject?.Property(property.Name);
                    list.AddRange(SortOutFlowPart(property, compartWidget));
                }
                else
                {
                    var propertyDiff = new PropertyDiffViewModel
                    {
                        PropertyName = property.Name,
                        PropertyLabel = property.Name.Substring(property.Name.IndexOf('.') + 1)
                    };

                    var propertyDiffValue = (property.Value as JObject)?.Properties().First();
                    var compareProperty = compareObject?.Property(property.Name);
                    var compareDiffValue = (compareProperty?.Value as JObject)?.Properties().First();
                    SetCompareProperties(propertyDiffValue, compareDiffValue, propertyDiff);

                    list.Add(propertyDiff);
                }
            }

            return list;
        }

        public class HtmlBodyPart
        {
            public string? Html { get; set; }
        }

        public class SharedContent
        {
            public string[]? ContentItemIds { get; set; }
        }

        public class HtmlShared
        {
            public SharedContent? SharedContent { get; set; }
        }

        public class Widget
        {
            public string? ContentItemId { get; set; }
            public string? ContentType { get; set; }
            public HtmlBodyPart? HtmlBodyPart { get; set; }
            public HtmlShared? HTMLShared { get; set; }

        }

        private List<PropertyDiffViewModel> SortOutFlowPart(JProperty baseFlowPart, JProperty? compartFlowPart)
        {

            var list = new List<PropertyDiffViewModel>();
            var baseWidgets = baseFlowPart.Value["Widgets"] as JArray;
            var baseWidgetObjects = baseWidgets.Select(w => w.ToObject<Widget>()).ToList();

            var compareWidgets = compartFlowPart?.Value["Widgets"] as JArray;
            var compareWidgetObjects = compareWidgets.Select(w => w.ToObject<Widget>()).ToList();
            var indexer = 1;
            foreach (var baseWidgetObject in baseWidgetObjects)
            {
                var compareWidgetObject =
                    compareWidgetObjects.FirstOrDefault(w => w?.ContentItemId == baseWidgetObject?.ContentItemId);
                var propDiff = new PropertyDiffViewModel
                {
                    PropertyName = "FlowPart.Item_" + indexer.ToString("00"),
                    PropertyLabel = "FlowPart." + baseWidgetObject?.ContentType + ".Item_" + indexer.ToString("00")

                };
                if (baseWidgetObject?.ContentType == "HTMLShared")
                {
                    propDiff.BaseValue =
                         GetContentName(baseWidgetObject?.HTMLShared?.SharedContent?.ContentItemIds?[0]).Result;
                    propDiff.CompareValue =
                        GetContentName(compareWidgetObject?.HTMLShared?.SharedContent?.ContentItemIds?[0]).Result;
                    propDiff.IsHtmlShared = true;
                    propDiff.LanguageType = LanguageType.Text;
                    propDiff.BaseContentId = baseWidgetObject?.HTMLShared?.SharedContent?.ContentItemIds?[0];
                    propDiff.CompareContentId = compareWidgetObject?.HTMLShared?.SharedContent?.ContentItemIds?[0];
                }
                else
                {
                    propDiff.BaseValue = baseWidgetObject?.HtmlBodyPart?.Html;
                    propDiff.CompareValue = compareWidgetObject?.HtmlBodyPart?.Html;
                    propDiff.LanguageType = LanguageType.Html;
                }
                indexer++;
                list.Add(propDiff);
            }
            return list;

        }

        private async Task<string> GetContentName(string? contentItemId)
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

        private void RemoveProperties(JObject jObject)
        {
            var propertiesToRemove = new[]
            {
                "ContentItemId",
                "ContentItemVersionId",
                "ContentType",
                "Latest",
                "Published",
                "ModifiedUtc",
                "PublishedUtc",
                "CreatedUtc",
                "Owner",
                "AuditTrailPart",
                "GraphSyncPart",
                //"FlowPart",
                "ContentApprovalPart",
                "GraphSyncPart"
                //"SitemapPart",
                //"PageLocationPart"
            };
            foreach (string property in propertiesToRemove)
            {
                jObject.Remove(property);
            }
        }

        private VersionComparisonViewModel BuildViewModel(AuditTrailContentEvent firstContentItem, AuditTrailContentEvent secondContentItem, Dictionary<string,string> versions, string? selectedProperty = null)
        {

            var currentJson = JObject.FromObject(firstContentItem.ContentItem);
            var previousJson = JObject.FromObject(secondContentItem.ContentItem);

            currentJson = BuildDiffObject(firstContentItem.ContentItem.ContentType, currentJson);
            previousJson = BuildDiffObject(secondContentItem.ContentItem.ContentType, previousJson);
            var propertyNames = currentJson.Properties().Select(p => p.Name).ToList();
            propertyNames.Insert(0, "All");
            var propertyListOptions = propertyNames.Select(p => new SelectListItem { Value = p, Text = p }).ToList();
            var selectListOptions = versions.Select(v => new SelectListItem {Value = v.Key, Text = v.Value}).ToList();

            var propertyDiffs = new List<PropertyDiffViewModel>();
            if(!string.IsNullOrWhiteSpace(selectedProperty) && !selectedProperty.Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                if (selectedProperty == "FlowPart")
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    propertyDiffs.AddRange(SortOutFlowPart(currentJson.Property("FlowPart"), previousJson.Property("FlowPart")));
#pragma warning restore CS8604 // Possible null reference argument.
                }
                else
                {
                    currentJson = currentJson?.GetValue(selectedProperty) as JObject;
                    previousJson = previousJson?.GetValue(selectedProperty) as JObject;
                    var propertyDiff = new PropertyDiffViewModel
                    {
                        PropertyName = selectedProperty,
                        PropertyLabel = selectedProperty.Substring(selectedProperty.IndexOf('.') + 1)
                    };
                    SetCompareProperties(currentJson?.Properties().First(), previousJson?.Properties().First(),
                        propertyDiff);
                    propertyDiffs.Add(propertyDiff);
                }
            }
            else
            {
                propertyDiffs = GetDiffViewModel(currentJson, previousJson);
            }


            return new VersionComparisonViewModel
            {
                BaseVersionJson = currentJson?.ToString(),
                BaseVersionId = firstContentItem.ContentItem.ContentItemVersionId,
                BaseVersionNumber = firstContentItem.VersionNumber,
                CompareVersionJson = previousJson?.ToString(),
                CompareVersionId = secondContentItem.ContentItem.ContentItemVersionId,
                CompareVersionNumber = secondContentItem.VersionNumber,
                Options = selectListOptions,
                Properties = propertyListOptions,
                PropertyDiffs = propertyDiffs
            };
        }

        private async Task<List<AuditTrailContentEvent>> GetAllVersions(string contentItemId)
        {
            var allAuditVersions = await _session
                .Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index =>
                    index.Category == "Content" &&
                    index.CorrelationId == contentItemId)
                .OrderByDescending(index => index.Id)
                .ListAsync();

            var auditTrailContentEventsList
                = new List<AuditTrailContentEvent>();
            foreach (var auditTrailEvent in allAuditVersions)
            {
                var auditTrailContentEvent = auditTrailEvent.As<AuditTrailContentEvent>();
                if (auditTrailContentEventsList.All(k => k.VersionNumber != auditTrailContentEvent.VersionNumber))
                {
                    auditTrailContentEventsList.Add(auditTrailContentEvent);
                }
            }
            return auditTrailContentEventsList;
        }

    }
}
