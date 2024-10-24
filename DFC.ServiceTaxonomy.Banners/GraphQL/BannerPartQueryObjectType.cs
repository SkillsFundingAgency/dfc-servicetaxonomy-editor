using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Banners.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.Banners.GraphQL
{
    public class BannerPartQueryObjectType : ObjectGraphType<BannerPart>
    {
        public BannerPartQueryObjectType()
        {
            Name = "BannerPart";

            Field(x => x.WebPageURL, nullable: true);
            Field(x => x.WebPageName, nullable: true);
        }
    }
}
