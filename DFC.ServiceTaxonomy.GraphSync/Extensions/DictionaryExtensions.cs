using System.Collections.Generic;
using System.Text;

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

        public static string ToCypherPropertiesString(this IEnumerable<KeyValuePair<string, object>>? properties)
        {
            if (properties == null)
                return "";

            StringBuilder builder = new StringBuilder("{");
            foreach (var kvp in properties)
            {
                builder.Append($"{kvp.Key}: ");
                switch (kvp.Value)
                {
                    case string s:
                        builder.Append($"'{s}', ");
                        break;
                    default:
                        builder.Append($"{kvp.Value}, ");
                        break;
                }
            }

            builder.Append("}");
            return builder.ToString();
        }
    }
}
