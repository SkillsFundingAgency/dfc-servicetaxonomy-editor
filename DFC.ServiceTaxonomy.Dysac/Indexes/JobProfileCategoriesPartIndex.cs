using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Dysac.Indexes
{
    public class JobProfileCategoriesPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }

        public string? RelatedJobProfileContentItemIds { get; set; }
    }
}
