using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        //todo: object
        public string? IdPropertyValue { get; set; }

        private Query CreateQuery()
        {
            CheckIsValid();

            return new Query($"MATCH (n:{string.Join(':',NodeLabels)} {{{IdPropertyName}:'{IdPropertyValue}'}})-[r]->() DELETE n, r");
        }

        public void CheckIsValid()
        {
            List<string> validationErrors = new List<string>();

            if (!NodeLabels.Any())
                validationErrors.Add($"Missing {nameof(NodeLabels)}.");

            if (IdPropertyName == null)
                validationErrors.Add($"{nameof(IdPropertyName)} is null.");

            if (IdPropertyValue == null)
                validationErrors.Add($"{nameof(IdPropertyValue)} is null.");

            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{nameof(DeleteNodeCommand)} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary) => throw new NotImplementedException();

        public Query Query => CreateQuery();

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;
    }
}
