using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class WidgetPropertyService : IPropertyService
    {
        private readonly IContentNameService _contentServiceHelper;

        public WidgetPropertyService(IContentNameService contentServiceHelper)
        {
            _contentServiceHelper = contentServiceHelper;
        }

        public bool CanProcess(JsonElement? jElement, string? propertyName = null)
        {
            return jElement != null && jElement.GetValueOrDefault().ValueKind == JsonValueKind.Array && !string.IsNullOrWhiteSpace(propertyName) && propertyName.StartsWith("Widgets");
        }

        public IList<PropertyExtract> Process(string propertyName, JsonElement? jElement)
        {
            var widgetList = new List<PropertyExtract>();
            var widgets = JArray.FromObject(jElement)!.Select(w => w.ToObject<Widget>()).ToList() ?? new List<Widget?>();
            foreach (var widget in widgets)
            {
                if (widget == null)
                {
                    continue;
                }
                var key = $"{widget.ContentType}-{widget.ContentItemId}";
                if (widget.ContentType == "HTMLShared")
                {
                    var linkInfo = widget.HTMLShared?.SharedContent?.ContentItemIds!
                        .Select(c => new { Id = c, Name = _contentServiceHelper.GetContentNameAsync(c).Result })
                        .ToDictionary(k => k.Id, v => v.Name);
                    widgetList.Add(new PropertyExtract { Key = key, Name = "HTMLShared", Links = linkInfo });
                }
                else
                {
                    widgetList.Add(new PropertyExtract { Key = key, Name = "HTML", Value = widget?.HtmlBodyPart?.Html ?? string.Empty });
                }
            }

            return widgetList;
        }
    }
}
