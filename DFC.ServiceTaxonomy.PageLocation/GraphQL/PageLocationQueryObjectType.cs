using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageLocationQueryObjectType : ObjectGraphType<PageLocationPart>
    {
        public PageLocationQueryObjectType()
        {
            Name = nameof(PageLocationPart);

            Field(x => x.FullUrl);
        }
    }
}
