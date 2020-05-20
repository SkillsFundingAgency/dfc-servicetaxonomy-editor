using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class ContentPickerContentItemFactory
    {
        public readonly ConcurrentDictionary<string, ContentItem> IdLookup = new ConcurrentDictionary<string, ContentItem>();

        public ContentPicker CreateContentPicker(ContentItem contentItem)
        {
            return CreateContentPicker(new[] {contentItem});
        }

        public ContentPicker CreateContentPicker(IEnumerable<ContentItem> contentItems)
        {
            foreach (ContentItem contentItem in contentItems ?? Enumerable.Empty<ContentItem>())
            {
                // route will be named after first job profile it's found in
                //todo: when it already exists, should we expand the name to include all the jp's its used for as a starter for ten?
                // this will never happen, as each entry route will be named after the jp
                // hashset of content items?
                if (!IdLookup.TryAdd(contentItem.DisplayText, contentItem))
                {
                    // ctor with name?
                    ColorConsole.WriteLine($"{contentItems.First().GetType().Name} item '{contentItem.DisplayText}' already saved", ConsoleColor.Magenta);
                }
            }

            return new ContentPicker
            {
                ContentItemIds = contentItems?.Select(ci => IdLookup[ci.DisplayText].ContentItemId) ?? new string[0]
            };
        }
    }
}
