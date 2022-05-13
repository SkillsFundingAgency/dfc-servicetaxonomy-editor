// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentManagement;
// using OrchardCore.ContentManagement.Metadata;
// using OrchardCore.ContentManagement.Metadata.Settings;
// using OrchardCore.ContentManagement.Records;
// using YesSql;
// using YesSql.Services;

using OrchardCore.ContentFields.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class PreviewContentPickerResultProvider : DefaultContentPickerResultProvider, IContentPickerResultProvider
    {
        public PreviewContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
            : base(contentManager, contentDefinitionManager, session)
        {
        }

        public new string Name => "Preview";
    }

    // this version shows the picked preferred title, but the search still searches displaytest

    //todo: need to order /search by picked content preferred label
    // ^^ the default picker doesn't do any ordering!
    //todo: could try setting displaytest when import, but then how to update it when user chooses different preferred label?
    // public class PreviewContentPickerResultProvider : IContentPickerResultProvider
    // {
    //     private readonly IContentManager _contentManager;
    //     private readonly IContentDefinitionManager _contentDefinitionManager;
    //     private readonly ISession _session;
    //
    //     public PreviewContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
    //     {
    //         _contentManager = contentManager;
    //         _contentDefinitionManager = contentDefinitionManager;
    //         _session = session;
    //     }
    //
    //     public string Name => "Preview";
    //
    //     public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
    //     {
    //         var contentTypes = searchContext.ContentTypes;
    //         if (searchContext.DisplayAllContentTypes)
    //         {
    //             contentTypes = _contentDefinitionManager
    //                 .ListTypeDefinitions()
    //                 .Where(x => string.IsNullOrEmpty(x.GetSettings<ContentTypeSettings>().Stereotype))
    //                 .Select(x => x.Name)
    //                 .AsEnumerable();
    //         }
    //
    //         var query = _session.Query<ContentItem, ContentItemIndex>()
    //             .With<ContentItemIndex>(x => x.ContentType.IsIn(contentTypes) && x.Latest);
    //
    //         if (!string.IsNullOrEmpty(searchContext.Query))
    //         {
    //             query.With<ContentItemIndex>(x => x.DisplayText.Contains(searchContext.Query) || x.ContentType.Contains(searchContext.Query));
    //         }
    //
    //         var contentItems = await query.Take(50).ListAsync();
    //
    //         var results = new List<ContentPickerResult>();
    //
    //         foreach (var contentItem in contentItems)
    //         {
    //             // is this gonna be too slow? might be better to have title, but then would need to keep in sync
    //             // query session for all at once
    //             // if we want occupations listable, they will still have a bad title!
    //             string? preferredLabelContentItemId = ((JArray)contentItem.Content.Occupation.PreferredLabel.ContentItemIds).FirstOrDefault()?.ToString();
    //             //todo: pub/draft?
    //             var preferredLabelContentItem = await _contentManager.GetAsync(preferredLabelContentItemId, VersionOptions.Latest);
    //             string displayText = preferredLabelContentItem.Content.TitlePart.Title.ToString();
    //             results.Add(new ContentPickerResult
    //             {
    //                 ContentItemId = contentItem.ContentItemId,
    //                 //DisplayText = contentItem.ToString(),
    //                 //todo: need this configurable in settings (if we can override settings for content picker!)
    //                 // suppose could add to graph sync settings ContentPickerPreviewDisplayText
    //                 // have c# with optimisations? or name of field? can't be too slow! global to get name from contentpicker
    //                 DisplayText = displayText,
    //                 HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem)
    //             });
    //         }
    //
    //         return results.OrderBy(x => x.DisplayText);
    //     }
    //}
}
