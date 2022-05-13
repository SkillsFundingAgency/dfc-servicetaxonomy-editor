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

                var contentTypes = string.Join(",",
                    NodeLabels.Where(nodeLabel => !nodeLabel.Equals("Resource", System.StringComparison.InvariantCultureIgnoreCase)).ToArray());

                var parameters = new Dictionary<string, object>
                {
                    {"ContentType", contentTypes }
                };

                return new Query("DeleteNodesByType", parameters);
            }
        }

        public static implicit operator Query(CosmosDbDeleteNodesByTypeCommand c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //todo: What validation is possible here?
        }
    }
}
