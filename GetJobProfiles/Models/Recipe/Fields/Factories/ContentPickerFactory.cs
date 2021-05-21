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

        public readonly ConcurrentDictionary<string, string> keyValuePair = new ConcurrentDictionary<string, string>();

        public ContentPicker CreateContentPicker(string item)
        {
            if (string.IsNullOrEmpty(item))
                return new ContentPicker { ContentItemIds = Enumerable.Empty<string>() };

            return CreateContentPicker(new[] { item });
        }

        public ContentPicker CreateContentPicker(IDictionary<string, string> itemDictionary)
        {
            if (itemDictionary != null && itemDictionary.Any())
            {
                foreach(var item in itemDictionary)
                {
                    if (!keyValuePair.TryAdd(item.Key, _idGenerator.GenerateUniqueId()))
                    {
                        // ctor with name?
                        ColorConsole.WriteLine($"Content '{item}' already saved", ConsoleColor.Magenta);
                    }
                }
            }

            return new ContentPicker
            {
                ContentItemIds = itemDictionary?.Select(ci => keyValuePair[ci.Value]) ?? new string[0]
            };
        }

        public ContentPicker CreateContentPicker(IEnumerable<string> itemList)
        {
            foreach (string item in itemList ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!keyValuePair.TryAdd(item, _idGenerator.GenerateUniqueId()))
                {
                    // ctor with name?
                    ColorConsole.WriteLine($"Content '{item}' already saved", ConsoleColor.Magenta);
                }
            }

            return new ContentPicker
            {
                ContentItemIds = itemList?.Select(ci => keyValuePair[ci]) ?? new string[0]
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
