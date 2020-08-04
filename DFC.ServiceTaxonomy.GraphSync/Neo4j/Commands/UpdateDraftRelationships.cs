using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands
{
    public class UpdateDraftRelationships : IUpdateDraftRelationships
    {
        // prob need to piggy off mergenodecommand
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public object? SourceIdPropertyValue { get; set; }
        public string? IdPropertyName { get; set; }
        public object? IdPropertyValue { get; set; }

        // need also properties

        public List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                return new Query("");
            }
        }

        public static implicit operator Query(UpdateDraftRelationships c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (true)
                throw new CommandValidationException("");
        }
    }
}
