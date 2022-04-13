using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands
{
    public class CosmosDbDeleteNodesByTypeCommand : IDeleteNodesByTypeCommand
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

                var queryDetail = new QueryDetail
                {
                    Text = "DeleteNodesByType",
                    ContentTypes = NodeLabels.Where(nl => !string.IsNullOrWhiteSpace(nl) && !nl.Equals("Resource", System.StringComparison.InvariantCultureIgnoreCase)).ToList()
                };

                return new Query(queryDetail);
            }
        }

        public static implicit operator Query(CosmosDbDeleteNodesByTypeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: What validation is possible here?
        }
    }
}
