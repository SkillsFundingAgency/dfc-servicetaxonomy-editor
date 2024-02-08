using DFC.ServiceTaxonomy.PageLocation.GraphQL;
using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.TriageTool.GraphQL
{
    public class PagePartWhereInputObjectGraphType : WhereInputObjectGraphType<PagePart>
    {
        public PagePartWhereInputObjectGraphType()
        {
            /* Name = $"{nameof(PagePart)}Input";
             Description = s["The custom URL part of the content item"];

             AddScalarFilterFields<BooleanGraphType>("useInTriageTool", s["useInTriageTool"]);

           *//*  AddScalarFilterFields<StringGraphType>("url", S["full Url"]);
             AddScalarFilterFields<StringGraphType>("urlName", S["Url Name"]);*/
            
            AddScalarFilterFields<BooleanGraphType>(nameof(PageItemsPartIndex.UseInTriageTool), "blah");
        }
    }
  
}
