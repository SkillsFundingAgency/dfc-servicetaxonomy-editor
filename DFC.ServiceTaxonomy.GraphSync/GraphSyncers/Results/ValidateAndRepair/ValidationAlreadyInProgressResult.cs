using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidationAlreadyInProgressResult : IValidateAndRepairResults
    {
        public static readonly IValidateAndRepairResults Instance = new ValidationAlreadyInProgressResult();

        public bool Cancelled => true;
        public DateTime LastSync => throw new NotImplementedException();
        public List<ValidateAndRepairResult> GraphInstanceResults => throw new NotImplementedException();
        public bool AnyRepairFailures => throw new NotImplementedException();
    }
}
