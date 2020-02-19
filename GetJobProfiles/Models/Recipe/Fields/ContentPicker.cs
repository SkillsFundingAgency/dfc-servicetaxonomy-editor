using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Entities;

namespace GetJobProfiles.Models.Recipe.Fields
{
    public class ContentPickerFactory
    {
        private static readonly DefaultIdGenerator _idGenerator = new DefaultIdGenerator();

        public readonly ConcurrentDictionary<string,string> IdLookup = new ConcurrentDictionary<string, string>();

        public ContentPicker CreateContentPicker(IEnumerable<string> sourceContent)
        {
            foreach (string content in sourceContent ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!IdLookup.TryAdd(content, _idGenerator.GenerateUniqueId()))
                {
                    // ctor with name?
                    ColorConsole.WriteLine($"Content '{content}' already saved", ConsoleColor.Magenta);
                }
            }

            return new ContentPicker
            {
                ContentItemIds = sourceContent?.Select(ci => IdLookup[ci]) ?? new string[0]
            };
        }
    }

    public class ContentPicker
    {
        public ContentPicker()
        {}

        //todo: this will go
        public ContentPicker(ConcurrentDictionary<string, (string id, string text)> currentContentItems, IEnumerable<string> contentItems)
        {
            ContentItemIds = contentItems?.Select(ci => currentContentItems[ci].id) ?? new string[0];
        }

        public IEnumerable<string> ContentItemIds { get; set; }
    }
}
