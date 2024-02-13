using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageLocationInputObjectType : WhereInputObjectGraphType<PageLocationPart>
    {
        public PageLocationInputObjectType(IStringLocalizer<PageLocationInputObjectType> S)
        {
            Name = $"{nameof(PageLocationPart)}Input";
            Description = S["The custom URL part of the content item"];

            AddScalarFilterFields<StringGraphType>("url", S["full Url"]);
            AddScalarFilterFields<StringGraphType>("urlName", S["Url Name"]);
            AddScalarFilterFields<BooleanGraphType>("useInTriageTool", S["Use In triage tool"]);

        }
    }
}
