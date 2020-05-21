using System.Collections.Generic;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EqualityComparers
{
    public class ImportedTitleHtmlDescriptionEqualityComparer : EqualityComparer<TitleHtmlDescriptionContentItem>
    {
        public override bool Equals(TitleHtmlDescriptionContentItem i1, TitleHtmlDescriptionContentItem i2)
        {
            if (i1 == null && i2 == null)
                return true;

            if (i1 == null || i2 == null)
                return false;

            return i1.EponymousPart.Description.Html == i2.EponymousPart.Description.Html;
        }

        public override int GetHashCode(TitleHtmlDescriptionContentItem item)
        {
            return item.EponymousPart.Description.Html.GetHashCode();
        }
    }
}
