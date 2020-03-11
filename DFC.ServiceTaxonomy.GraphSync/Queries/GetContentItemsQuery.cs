using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using Neo4j.Driver;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    class GetContentItemsQuery : IGetContentItemsQuery
    {
        public string? QueryStatement { get; set; }

        public List<string> ValidationErrors()
        {
            var errors = new List<string>();

            if (QueryStatement == null)
                errors.Add($"{nameof(QueryStatement)} is null.");

            return errors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                return new Query(QueryStatement);
            }
        }
        public ContentItem ProcessRecord(IRecord record)
        {
            throw new NotImplementedException();
        }
    }
}
