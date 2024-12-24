using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries
{
    public class CosmosDbMatchSynonymsQuery : IQuery<IRecord>
    {
        private string FirstNodeLabel { get; }
        private string SecondNodeLabel { get; }
        private string PropertyValue { get; }

        private string[] RelationshipTypes { get; }

        public CosmosDbMatchSynonymsQuery(string firstNodeLabel, string secondNodeLabel, string propertyValue, params string[] relationshipTypes)
        {
            FirstNodeLabel = firstNodeLabel;
            SecondNodeLabel = secondNodeLabel;
            PropertyValue = propertyValue;
            RelationshipTypes = relationshipTypes;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (RelationshipTypes.Length == 0)
            {
                validationErrors.Add("At least one RelationshipType must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                throw new NotSupportedException("Synonyms functionality is no longer used, so hasn't been ported into Cosmos Db.");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
