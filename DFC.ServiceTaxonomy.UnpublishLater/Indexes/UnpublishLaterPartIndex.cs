using System;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.UnpublishLater.Indexes
{
    public class UnpublishLaterPartIndex : MapIndex
    {
        public DateTime? ScheduledUnpublishUtc { get; set; }
    }

    public class UnpublishLaterPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<UnpublishLaterPartIndex>()
                .Map(contentItem =>
                {
                    var unpublishLaterPart = contentItem.As<UnpublishLaterPart>();
                    if (unpublishLaterPart == null || !unpublishLaterPart.ScheduledUnpublishUtc.HasValue)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    return new UnpublishLaterPartIndex
                    {
                        ScheduledUnpublishUtc = unpublishLaterPart.ScheduledUnpublishUtc
                    };
                });
        }
    }
}
