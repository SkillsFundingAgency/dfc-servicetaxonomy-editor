using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.DataSync.Models;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.DataSync.Services
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
