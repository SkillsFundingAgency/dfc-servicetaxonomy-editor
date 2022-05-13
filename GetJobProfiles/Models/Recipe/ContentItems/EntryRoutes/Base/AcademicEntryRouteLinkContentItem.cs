using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base
{
    public class AcademicEntryRouteLinkContentItem : ContentItem
    {
        public AcademicEntryRouteLinkContentItem(string contentType, string title, string sitefinityUrl, string timestamp, string contentItemId)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            DisplayText = TitlePart.Title;

            GraphSyncPart = new GraphSyncPart(contentType);

            EponymousPart = new AcademicEntryRouteLinkPart
            {
                Link = new LinkField(sitefinityUrl)
            };
        }

        public AcademicEntryRouteLinkPart EponymousPart { get; set; }
        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class AcademicEntryRouteLinkPart
    {
        public LinkField Link { get; set; }
    }
}
