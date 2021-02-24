using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalPartViewModel
    {
        public ContentReviewStatus? ReviewStatus { get; set; }
        public string? Comment { get; set; }
        public string? ContentItemId { get; set; }

        public IEnumerable<KeyValuePair<string, string>> ReviewTypes => EnumExtensions.GetEnumNameAndDisplayNameDictionary( typeof(ReviewType))
            .Where(rt => rt.Key != "None");
    }
}
