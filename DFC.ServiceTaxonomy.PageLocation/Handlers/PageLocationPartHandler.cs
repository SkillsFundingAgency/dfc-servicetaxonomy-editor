using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Title.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    public class PageLocationPartHandler : ContentPartHandler<PageLocationPart>
    {
        private readonly ISession _session;

        public PageLocationPartHandler(ISession session)
        {
            _session = session;
        }

        public override Task InitializingAsync(InitializingContentContext context, PageLocationPart part)
        {
            part.Apply();
            return Task.CompletedTask;
        }

        public override async Task CloningAsync(CloneContentContext context, PageLocationPart part)
        {
            string title = context.CloneContentItem.Content[nameof(TitlePart)][nameof(TitlePart.Title)];
            string urlName = context.CloneContentItem.Content[nameof(PageLocationPart)][nameof(PageLocationPart.UrlName)];
            string fullUrl = context.CloneContentItem.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)];

            IEnumerable<ContentItem> pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();

            string urlSearchFragment = fullUrl.EndsWith("-clone") ?
                fullUrl :
                fullUrl.Contains("-clone") ?
                    fullUrl.Substring(0, fullUrl.LastIndexOf("-")) :
                    $"{fullUrl}-clone";

            IEnumerable<ContentItem> existingClones = pages.Where(x => ((string)x.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)]).Contains(urlSearchFragment));

            string urlPostfix = "-clone";                

            if (existingClones.Any())
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

            context.CloneContentItem.DisplayText = title.StartsWith("CLONE - ") ? title : $"CLONE - {title}";
            context.CloneContentItem.Alter<TitlePart>(part => part.Title = title.StartsWith("CLONE - ") ? title : $"CLONE - {title}");
            context.CloneContentItem.Alter<PageLocationPart>(part =>
            {
                part.UrlName = newUrlName;
                part.FullUrl = newFullUrl;
                part.DefaultPageForLocation = false;
                part.RedirectLocations = null;
            });
        }
    }
}
