using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair;
using DFC.ServiceTaxonomy.DataSync.Enums;

namespace DFC.ServiceTaxonomy.DataSync.ViewModels
{
    public class TriggerSyncValidationViewModel
    {
        public IValidateAndRepairResults? ValidateAndRepairResults { get; set; }
        public ValidationScope? Scope { get; set; }
    }
}
