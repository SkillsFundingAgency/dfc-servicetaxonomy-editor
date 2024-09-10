using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFC.ServiceTaxonomy.Title.Indexes;
using Microsoft.Extensions.Localization;
using DFC.ServiceTaxonomy.Title.Models;
using DFC.ServiceTaxonomy.DataAccess.Repositories;

namespace DFC.ServiceTaxonomy.Extensions
{
    public static class UniqueTitlePartExtensions
    {
        public static async IAsyncEnumerable<ValidationResult> ValidateAsync(this UniqueTitlePart part, IStringLocalizer S,IGenericIndexRepository<UniqueTitlePartIndex> uniqueTitleIndexRepository)
        {
            if (string.IsNullOrWhiteSpace(part.Title))
            {
                yield return new ValidationResult(S["A value is required for Title field."]);
            }

            var matches = await uniqueTitleIndexRepository.GetCount(b =>
                b.Title == part.Title && b.ContentItemId != part.ContentItem.ContentItemId && b.ContentType == part.ContentItem.ContentType);
            if (matches > 0)
            {
                yield return new ValidationResult(S["The Title '{0}' is already in use on another content item", part.Title]);

            }
        }

    }
}
