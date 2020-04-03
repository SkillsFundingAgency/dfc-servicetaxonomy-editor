using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Accordions.Settings
{
    public class AccordionPartSettingsViewModel
    {
        [BindNever]
        public AccordionPartSettings AccordionPartSettings { get; set; }
    }
}
