using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval.Indexes
{
    public class ContentApprovalPartIndex : MapIndex
    {
        public int ReviewStatus { get; set; }
        public int ReviewType { get; set; }
    }
}
