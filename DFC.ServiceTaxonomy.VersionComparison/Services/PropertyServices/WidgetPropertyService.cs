using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class WidgetPropertyService : IPropertyService
    {
        private readonly IContentManager _contentManager;

        public WidgetPropertyService(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public bool CanProcess(JToken? jToken, string? propertyName = null)
        {
            return jToken != null && jToken.Type == JTokenType.Array && !string.IsNullOrWhiteSpace(propertyName) && propertyName.StartsWith("Widgets");
        }

        public IList<PropertyDto> Process(string propertyName, JToken? jToken)
        {
            var widgetList = new List<PropertyDto>();
            var widgets = jToken?.Select(w => w.ToObject<Widget>()).ToList() ?? new List<Widget?>();
            foreach (var widget in widgets)
            {
                if (widget == null)
                {
                    continue;
                }
                var key = $"{widget.ContentType}-{widget.ContentItemId}";
                if (widget.ContentType == "HTMLShared")
                {
                    var linkInfo = widget.HTMLShared?.SharedContent?.ContentItemIds
                        .Select(c => new { Id = c, Name = GetContentName(c).Result })
                        .ToDictionary(k => k.Id, v => v.Name);
                    widgetList.Add(new PropertyDto { Key = key, Name = "HTMLShared", Links = linkInfo });
                }
                else
                {
                    widgetList.Add(new PropertyDto { Key = key, Name = "HTML", Value = widget?.HtmlBodyPart?.Html ?? string.Empty });
                }
            }

            return widgetList;
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
    }
}
