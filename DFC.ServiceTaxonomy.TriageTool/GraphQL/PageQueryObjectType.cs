using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.TriageTool.GraphQL
{
    public class PageQueryObjectType : ObjectGraphType<PagePart>
    {
       public PageQueryObjectType()
        {
            Name = "PagePart";

            Field(x => x.UseInTriageTool, nullable: true);
        }
    }
}
