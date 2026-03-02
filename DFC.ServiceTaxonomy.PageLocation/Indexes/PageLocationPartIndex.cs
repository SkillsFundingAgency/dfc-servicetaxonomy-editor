using System.Collections.Generic;
using System.Text.Json;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation.Indexes
{
    public class PageLocationPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? Url { get; set; }
        public string? UrlName { get; set; }
        public bool? UseInTriageTool { get; set; }
    }

    public class PageLocationPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PageLocationPartIndex>()
                .Map(contentItem =>
                {
                    var url = contentItem.As<PageLocationPart>()?.FullUrl;
                    var urlName = contentItem.As<PageLocationPart>()?.UrlName;

                    if (!string.IsNullOrEmpty(url) && (contentItem.Published || contentItem.Latest))
                    {
                        var content = JsonSerializer.Serialize(contentItem.Content);

                        //var root = JToken.Parse(content);
                        //var tile = (bool?)root.SelectToken("..UseInTriageTool.Value");


                        using var d = JsonDocument.Parse(content);
                        bool? tile = null;

                        var s = new Stack<JsonElement>();
                        s.Push(d.RootElement);

                        while (s.Count > 0 && tile == null)
                        {
                            var e = s.Pop();
                            if (e.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var p in e.EnumerateObject())
                                {
                                    if (p.NameEquals("UseInTriageTool"))
                                    {
                                        var v = p.Value;
                                        if (v.ValueKind == JsonValueKind.True) tile = true;
                                        else if (v.ValueKind == JsonValueKind.False) tile = false;
                                        else if (v.ValueKind == JsonValueKind.Object && v.TryGetProperty("Value", out var x))
                                            tile = x.ValueKind == JsonValueKind.True || x.ValueKind == JsonValueKind.False;
                                        break;
                                    }

                                    if (p.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                                        s.Push(p.Value);
                                }
                            }
                            else if (e.ValueKind == JsonValueKind.Array)
                                foreach (var x in e.EnumerateArray())
                                    if (x.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                                        s.Push(x);
                        }


                        return new PageLocationPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            Url = url,
                            UrlName = urlName,
                            UseInTriageTool = tile
                        };
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                });
        }
    }
}
