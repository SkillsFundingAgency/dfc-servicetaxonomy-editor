using DFC.ServiceTaxonomy.ContentApproval.Models;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval.Indexes
{
    public class ContentApprovalPartIndex : MapIndex
    {
        public ContentReviewStatus? ReviewStatus { get; set; }
        // there isn't an example in the orchard core code where an enum is used, so we may need...
        //public string? ReviewStatus { get; set; }
    }
}
