using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Helpers
{
    public static class ContentDefinitionHelper
    {
        private static readonly ConcurrentDictionary<string, string> s_cache = new ConcurrentDictionary<string, string>();

        public static ContentTypeDefinition? GetTypeDefinitionCaseInsensitive(
            string contentType,
            IContentDefinitionManager contentDefinitionManager,
            bool exceptionIfNull = true)
        {
            try
            {
                ContentTypeDefinition? contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(
                    GetCorrectlyCasedContentType(contentType, contentDefinitionManager));

                if (contentTypeDefinition != null)
                {
                    return contentTypeDefinition;
                }

                // There wasn't a match, could be because the type definition was updated - so clear our cache
                s_cache.Clear();

                if (exceptionIfNull)
                {
                    throw new KeyNotFoundException("Content type not found");
                }

                return null;
            }
            catch
            {
                // There wasn't a match, could be because the type definition was updated - so clear our cache
                s_cache.Clear();
                throw;
            }
        }

        private static string? GetCorrectlyCasedContentType(string? contentType, IContentDefinitionManager contentDefinitionManager)
        {
            if (contentType == null)
            {
                return null;
            }

            if (s_cache.TryGetValue(contentType, out string? typeFromCache))
            {
                return typeFromCache;
            }

            IEnumerable<ContentTypeDefinition>? contentTypeDefinitions = contentDefinitionManager.ListTypeDefinitions();
            string correctlyCasedContentType = contentTypeDefinitions
                .FirstOrDefault(item => item.Name.Equals(contentType, StringComparison.InvariantCultureIgnoreCase))?.Name ?? contentType;

            s_cache.TryAdd(contentType, correctlyCasedContentType);
            return correctlyCasedContentType;
        }
    }
}
