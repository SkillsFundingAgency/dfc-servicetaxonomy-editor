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

        public void CheckIsValid()
        {
            List<string> validationErrors = new List<string>();

            if (!NodeLabels.Any())
                validationErrors.Add($"Missing {nameof(NodeLabels)}.");

            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{nameof(DeleteNodesByTypeCommand)} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }

        public Query Query
        {
            get
            {
                CheckIsValid();

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
