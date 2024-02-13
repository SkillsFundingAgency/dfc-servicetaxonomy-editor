﻿using DFC.ServiceTaxonomy.CompUi.Enums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using YesSql.Indexes;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Indexes
{
    public class RelatedContentItemIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? ContentType { get; set; }
        public string? RelatedContentIds { get; set; }
    }

    public class RelatedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<RelatedContentItemIndex>()
            .When(contentItem => Enum.IsDefined(typeof(PublishedContentTypes), contentItem.ContentType))
                .Map(contentItem =>
                    {
                        if (!contentItem.Published && !contentItem.Latest)
                        {
                            return default!;
                        }

                        var content = JsonConvert.SerializeObject(contentItem.Content);

                        var root = JToken.Parse(content);

                        var tiles = root.SelectTokens("..ContentItemIds[*]");

                        return new RelatedContentItemIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            ContentType = contentItem.ContentType,
                            RelatedContentIds = string.Join(',', tiles),
                        };
                    });
        }
    }
}
