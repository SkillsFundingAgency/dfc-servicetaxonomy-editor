using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    public class QueryDetail : IEqualityComparer<QueryDetail>
    {
        public string Text { get; set; } = string.Empty;

        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public List<string> ContentTypes { get; set; } = new List<string>();

        public bool Equals([AllowNull] QueryDetail x, [AllowNull] QueryDetail y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.ToString().Equals(y.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public int GetHashCode([DisallowNull] QueryDetail obj) => throw new System.NotImplementedException();

        public override string ToString()
        {
            var dictionaryStrings = Parameters.OrderBy(k => k.Key)
                .Select(kv => $"{kv.Key.Trim()}{kv.Value?.ToString()?.Trim()}");
            var listStrings = ContentTypes.OrderBy(c => c).Select(c => c.Trim());
            return $"{Text.Trim()}{string.Join(',', dictionaryStrings)}{string.Join(',', listStrings)}";
        }
    }
}
