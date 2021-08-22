using System;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentApprovalPart : ContentPart
    {
        public ReviewStatus ReviewStatus { get; set; }
        public ReviewType ReviewType { get; set; }
        public bool IsForcePublished { get; set; }
        public Tuple<string, string>? FormSubmitAction { get; set; }
    }
}
