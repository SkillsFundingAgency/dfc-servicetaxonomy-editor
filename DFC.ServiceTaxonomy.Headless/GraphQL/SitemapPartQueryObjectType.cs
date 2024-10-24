using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Types;
using OrchardCore.Sitemaps.Models;

namespace DFC.ServiceTaxonomy.Headless.GraphQL
{
    public class SitemapPartQueryObjectType : ObjectGraphType<SitemapPart>
    {
        public SitemapPartQueryObjectType()
        {
            Name = "SitemapPart";

            Field(x => x.ChangeFrequency, nullable: true);
            Field(x => x.OverrideSitemapConfig, nullable: true);
            Field(x => x.Priority, nullable: true);
            Field(x => x.Exclude, nullable: true);
        }
    }
}
