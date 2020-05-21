using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.EqualityComparers
{
    public class ImportedAcademicRouteEqualityComparer : EqualityComparer<AcademicEntryRouteContentItem>
    {
        public override bool Equals(AcademicEntryRouteContentItem r1, AcademicEntryRouteContentItem r2)
        {
            if (r1 == null && r2 == null)
                return true;

            if (r1 == null || r2 == null)
                return false;

            return r1.EponymousPart.FurtherInfo.Html == r2.EponymousPart.FurtherInfo.Html &&
                   r1.EponymousPart.RelevantSubjects.Html == r2.EponymousPart.RelevantSubjects.Html &&
                   r1.EponymousPart.Links.ContentItemIds.SequenceEqual(r2.EponymousPart.Links.ContentItemIds) &&
                   r1.EponymousPart.Requirements.ContentItemIds.SequenceEqual(r2.EponymousPart.Requirements.ContentItemIds) &&
                   r1.EponymousPart.RequirementsPrefix.ContentItemIds.SequenceEqual(r2.EponymousPart.RequirementsPrefix.ContentItemIds);
        }

        public override int GetHashCode(AcademicEntryRouteContentItem item)
        {
            return item.EponymousPart.FurtherInfo.Html.GetHashCode()
                   ^ item.EponymousPart.RelevantSubjects.Html.GetHashCode()
                   ^ item.EponymousPart.Links.ContentItemIds.GetHashCode()
                   ^ item.EponymousPart.Requirements.ContentItemIds.GetHashCode()
                   ^ item.EponymousPart.RequirementsPrefix.ContentItemIds.GetHashCode();

        }
    }
}
