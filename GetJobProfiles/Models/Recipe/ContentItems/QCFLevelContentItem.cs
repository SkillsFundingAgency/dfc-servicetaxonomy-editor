using System;
using System.Collections.Generic;
using System.Text;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems
{
    public class QCFLevelContentItem : ContentItem
    {
        public QCFLevelContentItem(string title, string timestamp, string contentItemId = null)
            : base("QCFLevel", title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("QCFLevel");
            DisplayText = TitlePart.Title;
        }

        public virtual QCFLevelPart EponymousPart { get; set; }
        public TitlePart TitlePart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }
}
