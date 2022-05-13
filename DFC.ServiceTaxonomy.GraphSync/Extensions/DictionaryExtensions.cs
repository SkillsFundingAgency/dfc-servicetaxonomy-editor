using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class DictionaryExtensions
    {
        public static int IncreaseCount(this IDictionary<string, int> dictionary, string key)
        {
            dictionary.TryGetValue(key, out int currentCount);
            dictionary[key] = ++currentCount;
            return currentCount;
        }
    }
}
