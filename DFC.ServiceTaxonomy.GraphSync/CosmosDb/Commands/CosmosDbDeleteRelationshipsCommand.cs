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
            // todo: What validation is possible here?
        }
    }
}
