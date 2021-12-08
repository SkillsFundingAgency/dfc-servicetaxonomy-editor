using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries
{
    //todo: common base class with NoteWithOutgoingRelationships?
    //todo: move generic queries into neo4j
    public class CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue)
        {
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if(!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            //todo: needs to validate id property name and value too

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                var contentTypes = string.Join(",",
                    NodeLabels.Where(nodeLabel => !nodeLabel.Equals("Resource", System.StringComparison.InvariantCultureIgnoreCase)).ToArray());

                var parameters = new Dictionary<string, object?>
                {
                    {"ContentType", contentTypes }
                };

#pragma warning disable IDE0008 // Use explicit type
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var id = IdPropertyValue.ToString().Split('/')[IdPropertyValue.ToString().Split('/').Length - 1];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore IDE0008 // Use explicit type

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                return new Query(
                    @$"select * from c where c.id = '{id}'",
                    parameters);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            }
        }

        public INodeAndOutRelationshipsAndTheirInRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["nodeAndOutRelationshipsAndTheirInRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(CosmosDbNodeAndOutRelationshipsAndTheirInRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            //todo: return as IOutgoingRelationships, rather than tuple then convert

            IEnumerable<(IRelationship, INode, IEnumerable<(IRelationship, INode)>)> outgoingRelationships =
                ((IEnumerable<object>)results["outgoingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"],
                        (INode)or["destNode"],
                        ((IEnumerable<object>)or["destinationIncomingRelationships"])
                        .Cast<IDictionary<string, object>>()
                        .Select(ir =>
                            ((IRelationship)ir["destIncomingRelationship"],
                                (INode)ir["destIncomingRelSource"]))
                        .Where(t => t.Item1 != null)))
                .Where(t => t.Item1 != null);

            return new NodeAndOutRelationshipsAndTheirInRelationships(sourceNode, outgoingRelationships);
        }
    }
}
