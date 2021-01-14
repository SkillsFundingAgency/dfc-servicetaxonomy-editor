using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Services
{
    public interface IPageLocationClonePropertyGenerator
    {
        string GenerateUrlSearchFragment(string url);
        PageLocationPartCloneResult GenerateClonedPageLocationProperties(string urlName, string fullUrl, IEnumerable<ContentItem>? existingClones);
    }

    public class PageLocationClonePropertyGenerator : IPageLocationClonePropertyGenerator
    {
        public string GenerateUrlSearchFragment(string url)
        {
            #pragma warning disable S3358
            return url.EndsWith("-clone") ?
                url :
                url.Contains("-clone") ?
                    url.Substring(0, url.LastIndexOf("-")) :
                    $"{url}-clone";
            #pragma warning restore S3358
        }

        public PageLocationPartCloneResult GenerateClonedPageLocationProperties(string urlName, string fullUrl, IEnumerable<ContentItem>? existingClones)
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

            return new PageLocationPartCloneResult(newUrlName, newFullUrl);
        }
    }
}
