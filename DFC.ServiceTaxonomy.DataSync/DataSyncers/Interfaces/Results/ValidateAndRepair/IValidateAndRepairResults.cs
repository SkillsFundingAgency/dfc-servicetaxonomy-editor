using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair
{
    public interface IValidateAndRepairResults
    {
        bool Cancelled { get; }
        DateTime LastSync { get; }
        List<ValidateAndRepairResult> DataSyncInstanceResults { get; }
        public bool AnyRepairFailures { get; }
    }
}
