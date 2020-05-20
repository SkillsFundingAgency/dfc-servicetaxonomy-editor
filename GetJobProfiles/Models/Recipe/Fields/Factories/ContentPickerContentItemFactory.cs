using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
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
                string newName = ItemToUsers.AddOrUpdate(contentItem, contentItem.DisplayText,
                    (key, oldValue) => $"{oldValue}, {contentItem.DisplayText}");
                if (newName != contentItem.DisplayText)
                {
                    // ctor with name?
                    ColorConsole.WriteLine($"{typeof(T).Name} already encountered, share list '{newName}'", ConsoleColor.Magenta);
                }

                //thread-safe??
                T sharedItem = ItemToUsers.Keys.First(i => _comparer.Equals(i, contentItem));
                contentItemIds.Add(sharedItem.ContentItemId);
            }

            return new ContentPicker
            {
                ContentItemIds = contentItemIds
            };
        }
    }
}
