using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair
{
    public class ValidationAlreadyInProgressResult : IValidateAndRepairResults
    {
        public static readonly IValidateAndRepairResults EmptyInstance = new ValidationAlreadyInProgressResult();

        public bool Cancelled => true;
        public DateTime LastSync => throw new NotImplementedException();
        public List<ValidateAndRepairResult> DataSyncInstanceResults => throw new NotImplementedException();
        public bool AnyRepairFailures => throw new NotImplementedException();
    }
}
