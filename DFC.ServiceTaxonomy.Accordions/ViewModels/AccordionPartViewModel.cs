using System.Collections.Generic;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.Accordions.Models;
using DFC.ServiceTaxonomy.Accordions.Settings;

namespace DFC.ServiceTaxonomy.Accordions.ViewModels
{
    public class AccordionPartViewModel
    {
        public AccordionPart AccordionPart { get; set; }
        public IEnumerable<ContentField> Fields => AccordionPart.Fields;
        public AccordionPartSettings Settings { get; set; }
    }
}
