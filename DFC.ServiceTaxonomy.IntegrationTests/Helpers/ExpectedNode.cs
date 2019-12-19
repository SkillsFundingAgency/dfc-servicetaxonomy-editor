using System;
using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public sealed class ExpectedNode : INode
    {
        public long Id { get; set; }
        public IReadOnlyList<string>? Labels { get; set; }
        public IReadOnlyDictionary<string, object>? Properties { get; set; }

        public object this[string key] => throw new NotImplementedException();
        public bool Equals(INode other) => throw new NotImplementedException();
    }
}
