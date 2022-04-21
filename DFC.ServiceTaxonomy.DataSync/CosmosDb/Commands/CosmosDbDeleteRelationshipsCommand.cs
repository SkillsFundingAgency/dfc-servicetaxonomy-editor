using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.Commands
{
    // todo: abstract RelationshipCommand or have in builder??
    public class CosmosDbDeleteRelationshipsCommand : CosmosDbNodeWithOutgoingRelationshipsCommand, IDeleteRelationshipsCommand
    {
        public bool DeleteDestinationNodes { get; set; }

        public override Query Query
        {
            get
            {
                this.CheckIsValid();

                var parameters = new Dictionary<string, object> { { "sourceIdPropertyValue", SourceIdPropertyValue! } };
                return new Query("DeleteRelationships", parameters);
            }
        }

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // todo: What validation is possible here?
        }
    }
}
