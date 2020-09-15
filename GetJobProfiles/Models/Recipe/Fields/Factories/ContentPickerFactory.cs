using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Entities;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class ContentPickerFactory
    {
        private static readonly DefaultIdGenerator _idGenerator = new DefaultIdGenerator();

        public readonly ConcurrentDictionary<string,string> IdLookup = new ConcurrentDictionary<string, string>();

        public ContentPicker CreateContentPicker(string sourceContent)
        {
            if (string.IsNullOrEmpty(sourceContent))
                return new ContentPicker {ContentItemIds = Enumerable.Empty<string>()};

            return CreateContentPicker(new[] {sourceContent});
        }

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

        public ContentPicker CreateContentPickerFromContent(string contentType, IEnumerable<string> displayTexts)
        {
            return new ContentPicker
            {
                ContentItemIds = displayTexts?.Select(it => $"«c#: await Content.GetContentItemIdByDisplayText(\"{contentType}\", \"{it}\")»")
            };
        }
    }
}
