using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public sealed class ExpectedRelationship : IRelationship
    {
        public IReadOnlyDictionary<string, object>? Properties { get; set; }
        public long Id { get; set; }
        public string? Type { get; set; }
        public long StartNodeId { get; set; }
        public long EndNodeId { get; set; }

        public object this[string key] => throw new System.NotImplementedException();
        public bool Equals(IRelationship other) => throw new System.NotImplementedException();
    }
}
