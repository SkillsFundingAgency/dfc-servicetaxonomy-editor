using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.GraphQL;
using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using YesSql;

namespace DFC.ServiceTaxonomy.TriageTool.GraphQL
{
    public class PagePartGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, ResolveFieldContext context)
        {
            if (!context.HasArgument("PagePart"))
            {
                return Task.FromResult(query);
            }

            var part = context.GetArgument<PagePart>("PagePart");

            if (part == null)
            {
                return Task.FromResult(query);
            }

            var pageQuery = query.With<PageItemsPartIndex>();

            if (!string.IsNullOrWhiteSpace(part.UseInTriageTool.ToString()))
            {
                return Task.FromResult(pageQuery.Where(index => index.UseInTriageTool == part.UseInTriageTool).All());
            }
            return Task.FromResult(query);
        }
    }
}
