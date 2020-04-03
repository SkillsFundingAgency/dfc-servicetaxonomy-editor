using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Accordions.Models
{
    public class AccordionPart : ContentPart
    {
        [BindNever]
        public List<ContentField> Fields { get; set; } = new List<ContentField>
        {
            new TextField(),
            new TextField()
        };
    }
}
