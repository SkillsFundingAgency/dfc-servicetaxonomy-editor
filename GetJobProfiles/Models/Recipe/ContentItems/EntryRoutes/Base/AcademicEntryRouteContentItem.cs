using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base
{
    public class AcademicEntryRouteContentItem : ContentItem
    {
        public AcademicEntryRouteContentItem(string contentType, string title, AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base(contentType, null, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            DisplayText = TitlePart.Title;

            EponymousPart = new AcademicEntryRoutePart
            {
                RelevantSubjects = new HtmlField(entryRoute.RelevantSubjects),
                FurtherInfo = new HtmlField(entryRoute.FurtherInformation)
            };
            GraphSyncPart = new GraphSyncPart(contentType);
        }

        public TitlePart TitlePart { get; set; }
        public AcademicEntryRoutePart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class AcademicEntryRoutePart
    {
        public HtmlField RelevantSubjects { get; set; }
        public HtmlField FurtherInfo { get; set; }
        public ContentPicker RequirementsPrefix { get; set; }
        public ContentPicker Requirements { get; set; }
        public ContentPicker Links { get; set; }
    }
}
