using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
    public class StandardNode : INode
#pragma warning restore S4035 // Classes implementing "IEquatable<T>" should be sealed
    {
#pragma warning disable CS8603 // Possible null reference return.
        public object this[string key] => null;
#pragma warning restore CS8603 // Possible null reference return.

        public IReadOnlyList<string> Labels { get; set; } = new List<string>();

        public IReadOnlyDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public long Id => -1;

        public bool Equals([AllowNull] INode other) => true;
    }
}
