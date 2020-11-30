﻿using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.GraphSync.ViewModels
{
    public class TriggerSyncValidationViewModel
    {
        public IValidateAndRepairResults? ValidateAndRepairResults { get; set; }
        public ValidationScope? Scope { get; set; }
    }
}
