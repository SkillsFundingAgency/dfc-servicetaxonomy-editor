namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentItemsApprovalCounts
    {
        public int Count { get; set; }

        // reviewTypeCounts[ReviewType.None] currently contains the total count, rather than the count of None
        public int[]? ReviewTypeCounts { get; set; }
    }
}
