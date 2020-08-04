using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: doesn't seem to be used (and it deletes the relationship and optionally deletes the node)
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        public object? IdPropertyValue { get; set; }
        //todo: rename command : DeleteNodeAndOutgoingRelationshipsCommand
        public bool DeleteNode { get; set; }

        public List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (!NodeLabels.Any())
                validationErrors.Add($"Missing {nameof(NodeLabels)}.");

            if (IdPropertyName == null)
                validationErrors.Add($"{nameof(IdPropertyName)} is null.");

            if (IdPropertyValue == null)
                validationErrors.Add($"{nameof(IdPropertyValue)} is null.");

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                return new Query(
$@"MATCH (n:{string.Join(':',NodeLabels)} {{{IdPropertyName}:'{IdPropertyValue}'}})
OPTIONAL MATCH (n)-[r]->()
DELETE {(DeleteNode ? "n, r" : "r")}");
            }
        }

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (resultSummary.Counters.NodesDeleted != 1)
                throw new CommandValidationException($"Expecting 1 node to be deleted, but {resultSummary.Counters.NodesDeleted} were actually deleted.");

            //todo: check number of relationships deleted?
        }
    }
}
