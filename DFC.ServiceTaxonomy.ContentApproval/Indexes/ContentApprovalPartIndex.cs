using DFC.ServiceTaxonomy.ContentApproval.Models;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval.Indexes
{
    public class ContentApprovalPartIndex : MapIndex
    {
        public ContentApprovalStatus ApprovalStatus { get; set; }
    }
}
