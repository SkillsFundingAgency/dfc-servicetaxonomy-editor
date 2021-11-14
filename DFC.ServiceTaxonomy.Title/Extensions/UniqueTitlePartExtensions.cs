using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFC.ServiceTaxonomy.Title.Indexes;
using Microsoft.Extensions.Localization;
using DFC.ServiceTaxonomy.Title.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.Extensions
{
    public static class UniqueTitlePartExtensions
    {
        public static async IAsyncEnumerable<ValidationResult> ValidateAsync(this UniqueTitlePart part, IStringLocalizer S,
            ISession session)
        {
            if (string.IsNullOrWhiteSpace(part.Title))
            {
                yield return new ValidationResult(S["A value is required for Title field."]);
            }

            var matches = await session.QueryIndex<UniqueTitlePartIndex>(b =>
                b.Title == part.Title && b.ContentItemId != part.ContentItem.ContentItemId).CountAsync();
            if (matches > 0)
            {
                yield return new ValidationResult(S["The Title '{0}' is already in use on another content item", part.Title]);

            }
        }

    }
}
