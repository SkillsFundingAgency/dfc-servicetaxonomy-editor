using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.Neo4j.Queries
{
    public static class QueryExtensions
    {
        public static void CheckIsValid<TRecord>(this IQuery<TRecord> query)
        {
            List<string> validationErrors = query.ValidationErrors();
            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{query.GetType().Name} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }
    }
}
