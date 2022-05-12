using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    class GetContentItemsAsJsonQuery : IGetContentItemsAsJsonQuery
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
                return new Query(QueryStatement ?? string.Empty);
            }
        }
        public string ProcessRecord(IRecord record)
        {
            return JsonConvert.SerializeObject(record.Values.Values);
        }
    }
}
