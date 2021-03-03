using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalPartViewModel
    {
        public ContentApprovalPartViewModel()
        {
            ReviewTypes = new Dictionary<string, string>();
        }

        public ContentReviewStatus? ReviewStatus { get; set; }

        [Required(ErrorMessage = "Please enter a comment")]
        public string? Comment { get; set; }

        public string? ContentItemId { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ReviewTypes { get; set; }
    }
}
