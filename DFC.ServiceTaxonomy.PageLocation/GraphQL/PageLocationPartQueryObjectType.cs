using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageLocationPartQueryObjectType : ObjectGraphType<PageLocationPart>
    {
        public PageLocationPartQueryObjectType()
        {
            Name = "PageLocationPart";

            Field(x => x.UrlName, nullable: true);
            Field(x => x.DefaultPageForLocation, nullable: true);
            Field(x => x.RedirectLocations, nullable: true);
            Field(x => x.FullUrl, nullable: true);
            Field(x => x.UseInTriageTool, nullable: true);
        }
    }
}
