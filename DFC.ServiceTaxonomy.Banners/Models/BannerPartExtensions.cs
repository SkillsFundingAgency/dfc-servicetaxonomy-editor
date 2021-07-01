using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFC.ServiceTaxonomy.Banners.Indexes;
using Microsoft.Extensions.Localization;
using YesSql;

namespace DFC.ServiceTaxonomy.Banners.Models
{
    public static class BannerPartExtensions
    {
        public static async IAsyncEnumerable<ValidationResult> ValidateAsync(this BannerPart part, IStringLocalizer S,
            ISession session)
        {
            if (string.IsNullOrWhiteSpace(part.WebPageName))
            {
                yield return new ValidationResult(S["A value is required for Webpage name field."]);
            }
            if (string.IsNullOrWhiteSpace(part.WebPageURL))
            {
                yield return new ValidationResult(S["A value is required for webpage location."]);
            }

            var matches = await session.QueryIndex<BannerPartIndex>(b =>
                b.WebPageName == part.WebPageName && b.ContentItemId != part.ContentItem.ContentItemId).CountAsync();
            if(matches > 0)
            {
                yield return new ValidationResult(S["The Webpage name '{0}' is already in use on another page banner.", part.WebPageName]);

            }
            matches = await session.QueryIndex<BannerPartIndex>(b =>
                b.WebPageURL == part.WebPageURL && b.ContentItemId != part.ContentItem.ContentItemId).CountAsync();
            if (matches > 0) 
            {
                yield return new ValidationResult(S["The webpage location '{0}' is already in use on another page banner.", part.WebPageURL]);

            }
        }

    }
}
