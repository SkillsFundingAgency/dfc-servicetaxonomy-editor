using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
    public class StandardRelationship : IRelationship
#pragma warning restore S4035 // Classes implementing "IEquatable<T>" should be sealed
    {
#pragma warning disable CS8603 // Possible null reference return.
        public object this[string key] => null;
#pragma warning restore CS8603 // Possible null reference return.

        public string Type { get; set; } = string.Empty;

        public long StartNodeId => -1;

        public long EndNodeId => -1;

        public IReadOnlyDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public long Id => -1;

        public bool Equals([AllowNull] IRelationship other) => true;
    }
}
