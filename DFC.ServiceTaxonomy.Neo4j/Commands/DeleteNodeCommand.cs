using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class DeleteNodeCommand : IDeleteNodeCommand
    {
        //todo: needs to be         public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? ContentType { get; set; }
        //todo: needs to be IdPropertyName & IdPropertyValue
        public string? Uri { get; set; }

        private Query CreateQuery()
        {
            CheckIsValid();

            return new Query($"MATCH (n:ncs__{ContentType} {{ uri:'{Uri}' }})-[r]->() DELETE n, r");
        }

        public void CheckIsValid()
        {
            List<string> validationErrors = new List<string>();

            if (ContentType == null)
                validationErrors.Add($"{nameof(ContentType)} is null.");

            if (Uri == null)
                validationErrors.Add($"{nameof(Uri)} is null.");

            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{nameof(DeleteNodeCommand)} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary) => throw new NotImplementedException();

        public Query Query => CreateQuery();

        public static implicit operator Query(DeleteNodeCommand c) => c.Query;
    }
}
