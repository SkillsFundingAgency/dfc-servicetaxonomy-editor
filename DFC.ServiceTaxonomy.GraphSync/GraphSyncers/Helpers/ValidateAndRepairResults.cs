using System;
using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ValidateAndRepairResults
    {
        public DateTime LastSync { get; }
        //rename this to contatingraph
        public List<ValidateAndRepairResult> GraphInstanceResults { get; }

        public ValidateAndRepairResults(DateTime lastSync)
        {
            LastSync = lastSync;
            GraphInstanceResults = new List<ValidateAndRepairResult>();
        }
    }
}
