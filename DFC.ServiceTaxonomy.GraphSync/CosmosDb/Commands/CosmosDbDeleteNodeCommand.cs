using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands
{
    //todo: rename - it deletes the outgoing relationships and optionally deletes the node, and incoming relationships with given properties
    public class CosmosDbDeleteNodeCommand : IDeleteNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        public object? IdPropertyValue { get; set; }
        //todo: rename command : DeleteNodeAndOutgoingRelationshipsCommand
        public bool DeleteNode { get; set; }
        public IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; set; }

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
                var parameters = new Dictionary<string, object>
                {
                    { "uri", IdPropertyValue! }
                };

                return new Query("DeleteNodeCommand", parameters);
            }
        }

        public static implicit operator Query(CosmosDbDeleteNodeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (resultSummary.Counters.NodesDeleted != 1)
                throw new CommandValidationException($"Expecting 1 node to be deleted, but {resultSummary.Counters.NodesDeleted} were actually deleted.");

            //todo: check number of relationships deleted?
        }
    }
}
