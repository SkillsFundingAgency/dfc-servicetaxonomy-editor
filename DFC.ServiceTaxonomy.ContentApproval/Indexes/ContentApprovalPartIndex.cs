using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval.Indexes
{
    public class ContentApprovalPartIndex : MapIndex
    {
        public string? ReviewStatus { get; set; }
        public string? ReviewType { get; set; }
    }
}
