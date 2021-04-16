using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalPartViewModel
    {
        public ContentApprovalPartViewModel()
        {
            ReviewTypes = new Dictionary<string, string>();
        }

        public ReviewStatus? ReviewStatus { get; set; }
        public string? Comment { get; set; }
        public string? ContentItemId { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ReviewTypes { get; set; }
    }
}
