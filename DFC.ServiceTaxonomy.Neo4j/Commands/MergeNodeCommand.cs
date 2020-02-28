using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: inject database and add execute?
    public class MergeNodeCommand : IMergeNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public void CheckIsValid()
        {
            if (NodeLabels == null)
                throw new InvalidOperationException($"{nameof(NodeLabels)} is null");

            if (!NodeLabels.Any())
                throw new InvalidOperationException($"No labels");

            if (IdPropertyName == null)
                throw new InvalidOperationException($"{nameof(IdPropertyName)} is null");
        }

        private Query CreateQuery()
        {
            CheckIsValid();

            return new Query(
                $"MERGE (n:{string.Join(':',NodeLabels)} {{ {IdPropertyName}:'{Properties[IdPropertyName!]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", Properties}});
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(MergeNodeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // nothing yet
            // validation can check return from query and/or counters are in range in result summary and/or notifications
        }
    }
}
