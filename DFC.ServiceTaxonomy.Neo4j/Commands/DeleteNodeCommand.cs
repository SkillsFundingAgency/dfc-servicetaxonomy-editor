using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        public object? IdPropertyValue { get; set; }

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

        private Query CreateQuery()
        {
            CheckIsValid();

            return new Query(
$@"MATCH (n:{string.Join(':',NodeLabels)} {{{IdPropertyName}:'{IdPropertyValue}'}})
OPTIONAL MATCH (n)-[r]->()
DELETE n, r");
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (resultSummary.Counters.NodesDeleted != 1)
                throw new CommandValidationException($"Expecting 1 node to be deleted, but {resultSummary.Counters.NodesDeleted} were actually deleted.");

            //todo: check number of relationships deleted?
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;
    }
}
