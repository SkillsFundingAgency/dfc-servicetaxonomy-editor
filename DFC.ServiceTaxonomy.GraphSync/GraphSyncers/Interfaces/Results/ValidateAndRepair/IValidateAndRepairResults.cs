using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair
{
    public interface IValidateAndRepairResults
    {
        bool Cancelled { get; }
        DateTime LastSync { get; }
        List<ValidateAndRepairResult> GraphInstanceResults { get; }
        public bool AnyRepairFailures { get; }
    }
}
