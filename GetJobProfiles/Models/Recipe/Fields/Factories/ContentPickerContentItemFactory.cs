using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
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

    public class ContentPickerContentItemFactory<T> where T : ContentItem
    {
        private readonly EqualityComparer<T> _comparer;
        public readonly ConcurrentDictionary<T, string> ItemToUsers;

        public ContentPickerContentItemFactory(EqualityComparer<T> comparer)
        {
            _comparer = comparer;
            ItemToUsers = new ConcurrentDictionary<T, string>(comparer);
        }

        public ContentPicker CreateContentPicker(T contentItem)
        {
            return CreateContentPicker(new[] {contentItem});
        }

        public ContentPicker CreateContentPicker(IEnumerable<T> contentItems)
        {
            List<string> contentItemIds = new List<string>();

            foreach (T contentItem in contentItems ?? Enumerable.Empty<T>())
            {
                // route will be named after first job profile it's found in
                //todo: when it already exists, should we expand the name to include all the jp's its used for as a starter for ten?
                // this will never happen, as each entry route will be named after the jp
                // hashset of content items?
                string newName = ItemToUsers.AddOrUpdate(contentItem, contentItem.DisplayText,
                    (key, oldValue) => $"{oldValue}, {contentItem.DisplayText}");
                if (newName != contentItem.DisplayText)
                {
                    // ctor with name?
                    ColorConsole.WriteLine($"{typeof(T).Name} already encountered, share list '{newName}'", ConsoleColor.Magenta);
                }

                //thread-safe??
                T existingItem = ItemToUsers.Keys.First(i => _comparer.Equals(i, contentItem));
                //todo: need to set displaytext and title to the new name
                // either do that outside here, or we'll need a way to gt at the title from T
                contentItemIds.Add(existingItem.ContentItemId);
            }

            return new ContentPicker
            {
                ContentItemIds = contentItemIds
            };
        }
    }
}
