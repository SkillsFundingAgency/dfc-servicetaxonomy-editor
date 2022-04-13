using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries
{
    public class CosmosDbGetIncomingContentPickerRelationshipsQuery : IGetIncomingContentPickerRelationshipsQuery
    {
        public IEnumerable<string> NodeLabels { get; set; } = Enumerable.Empty<string>();
        public string? IdPropertyName { get; set; }
        public object? IdPropertyValue { get; set; }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (IdPropertyName == null)
                validationErrors.Add($"{nameof(IdPropertyName)} is null.");

            if (IdPropertyValue == null)
                validationErrors.Add($"{nameof(IdPropertyValue)} is null.");

            if(!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                // TODO: This is currently pretty much identical to CosmosDbSubgraphQuery.cs and CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery.cs, do we need all?

                this.CheckIsValid();

                var contentType = NodeLabels.FirstOrDefault(nodeLabel => !nodeLabel.Equals("Resource", StringComparison.InvariantCultureIgnoreCase));

#pragma warning disable CS8602  // Dereference of a possibly null reference.
                var id = IdPropertyValue!.ToString().Split('/')[IdPropertyValue.ToString().Split('/').Length - 1];
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var queryDetail = new QueryDetail
                {
                    Text = "SELECT * FROM c WHERE c.id = @id",
                    Parameters = new Dictionary<string, object> { { "@id", id } },
                    ContentTypes = new List<string> { contentType }
                };

                return new Query(queryDetail);
            }
        }

        public INodeWithOutgoingRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["sourceNodeWithOutgoingRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(CosmosDbGetIncomingContentPickerRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            IEnumerable<(IRelationship, INode)> outgoingRelationships =
                ((IEnumerable<object>)results["outgoingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"], (INode)or["destinationNode"]));

            if (outgoingRelationships.Count() == 1 && outgoingRelationships.First().Item1 == null)
                outgoingRelationships = Enumerable.Empty<(IRelationship, INode)>();

            CosmosDbNodeWithOutgoingRelationships nodeWithOutgoingRelationships =
                //todo: check all combos of missing data
                new CosmosDbNodeWithOutgoingRelationships(sourceNode, outgoingRelationships);

            return nodeWithOutgoingRelationships;
        }
    }
}
