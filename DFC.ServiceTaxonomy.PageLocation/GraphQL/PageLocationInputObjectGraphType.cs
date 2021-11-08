using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageLocationInputObjectType : InputObjectGraphType<PageLocationPart>
    {
        public PageLocationInputObjectType()
        {
            Name = $"{nameof(PageLocationPart)}Input";

            Field(x => x.FullUrl, nullable: true).Description("the path of the content item to filter");
        }
    }
}
