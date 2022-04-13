using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands
{
    public class CosmosDbDeleteRelationshipsCommand : CosmosDbNodeWithOutgoingRelationshipsCommand, IDeleteRelationshipsCommand
    {
        public bool DeleteDestinationNodes { get; set; }

        public override Query Query
        {
            get
            {
                this.CheckIsValid();

                var queryDetail = new QueryDetail
                {
                    Text = "DeleteRelationships",
                    Parameters =
                        new Dictionary<string, object> { { "sourceIdPropertyValue", SourceIdPropertyValue! } }
                };

                return new Query(queryDetail);
            }
        }

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            //TODO: do nothing
            //if (DeleteDestinationNodes)
            //{
            //    if (resultSummary.Counters.NodesDeleted > _expectedDeleted)
            //        throw CreateValidationException(resultSummary,
            //            $"Expected no more than {_expectedDeleted} nodes to be deleted, but {resultSummary.Counters.NodesDeleted} were deleted.");

            //    // we don't know (without querying) how many relationships are deleted, if DeleteDestinationNodes is true
            //    // (due to not knowing how many outgoing relationships are on the destination nodes)
            //}
            //else
            //{
            //    if (resultSummary.Counters.RelationshipsDeleted != _expectedDeleted)
            //        throw CreateValidationException(resultSummary,
            //            $"Expected {_expectedDeleted} relationships to be deleted, but {resultSummary.Counters.RelationshipsDeleted} were deleted.");
            //}
        }
    }
}
