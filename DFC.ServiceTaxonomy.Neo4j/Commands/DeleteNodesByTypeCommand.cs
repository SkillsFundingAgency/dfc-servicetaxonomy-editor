using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class DeleteNodesByTypeCommand : IDeleteNodesByTypeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();

        public List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (!NodeLabels.Any())
                validationErrors.Add($"Missing {nameof(NodeLabels)}.");

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                //todo: optionally delete node

                return new Query(
$@"MATCH (n:{string.Join(':', NodeLabels)})
OPTIONAL MATCH (n)-[r]->()
DELETE n, r");
            }
        }

        public static implicit operator Query(DeleteNodesByTypeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: What validation is possible here?
        }
    }
}
