using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentItemsApprovalCounts
    {
        public int Count { get; set; }
        public int[]? SubCounts { get; set; }
        public List<ContentItem>? MyItems { get; set; }
    }
}
