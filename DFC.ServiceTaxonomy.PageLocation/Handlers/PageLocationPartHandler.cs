using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.Services;
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
        private readonly IPageLocationClonePropertyGenerator _generator;

        public PageLocationPartHandler(ISession session, IPageLocationClonePropertyGenerator generator)
        {
            _session = session;
            _generator = generator;
        }

        public override Task InitializingAsync(InitializingContentContext context, PageLocationPart part)
        {
            part.Apply();
            return Task.CompletedTask;
        }

        //public override async Task CloningAsync(CloneContentContext context, PageLocationPart part)
        //{
        //    string title = context.CloneContentItem.Content[nameof(TitlePart)][nameof(TitlePart.Title)];
        //    string urlName = context.CloneContentItem.Content[nameof(PageLocationPart)][nameof(PageLocationPart.UrlName)];
        //    string fullUrl = context.CloneContentItem.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)];

        //    IEnumerable<ContentItem> pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();

        //    string urlSearchFragment = _generator.GenerateUrlSearchFragment(fullUrl);

        //    IEnumerable<ContentItem> existingClones = pages.Where(x => ((string)x.Content[nameof(PageLocationPart)][nameof(PageLocationPart.FullUrl)]).Contains(urlSearchFragment));

        //    var result = _generator.GenerateClonedPageProperties(title, urlName, fullUrl, existingClones);

        //    context.CloneContentItem.DisplayText = result.Title;
        //    context.CloneContentItem.Alter<TitlePart>(part => part.Title = result.Title);
        //    context.CloneContentItem.Alter<PageLocationPart>(part =>
        //    {
        //        part.UrlName = result.UrlName;
        //        part.FullUrl = result.FullUrl;
        //        part.DefaultPageForLocation = false;
        //        part.RedirectLocations = null;
        //    });
        //}
    }
}
