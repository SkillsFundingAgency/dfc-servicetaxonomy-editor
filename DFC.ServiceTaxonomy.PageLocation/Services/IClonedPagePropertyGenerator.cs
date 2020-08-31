using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Services
{
    public interface IClonedPagePropertyGenerator
    {
        string GenerateUrlSearchFragment(string url);
        PageCloneResult GenerateClonedPageProperties(string title, string urlName, string fullUrl, IEnumerable<ContentItem>? existingClones);
    }

    public class ClonedPagePropertyGenerator : IClonedPagePropertyGenerator
    {
        public string GenerateUrlSearchFragment(string url)
        {
            return url.EndsWith("-clone") ?
                url :
                url.Contains("-clone") ?
                    url.Substring(0, url.LastIndexOf("-")) :
                    $"{url}-clone";
        }

        public PageCloneResult GenerateClonedPageProperties(string title, string urlName, string fullUrl, IEnumerable<ContentItem>? existingClones)
        {
            string urlPostfix = "-clone";

            if (existingClones?.Any() ?? false)
            {
                string latestClonedUrl = existingClones
                    .OrderBy(x => x.CreatedUtc)
                    .Select(x => (string)x.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)])
                    .Last();

                if (latestClonedUrl.EndsWith("-clone"))
                {
                    urlPostfix += "-2";
                }
                else
                {
                    urlPostfix += $"-{Int32.Parse(latestClonedUrl.Substring(latestClonedUrl.LastIndexOf("-") + 1)) + 1}";
                }
            }

            string newUrlName = string.Empty;
            string newFullUrl = string.Empty;

            if (fullUrl.Contains("-clone"))
            {
                newUrlName = $"{urlName.Substring(0, urlName.IndexOf("-clone"))}{urlPostfix}";
                newFullUrl = $"{fullUrl.Substring(0, fullUrl.IndexOf("-clone"))}{urlPostfix}";
            }
            else
            {
                newUrlName = $"{urlName}{urlPostfix}";
                newFullUrl = $"{fullUrl}{urlPostfix}";
            }

            string newTitle = title.StartsWith("CLONE - ") ? title : $"CLONE - {title}";

            return new PageCloneResult(newTitle, newUrlName, newFullUrl);
        }
    }
}
