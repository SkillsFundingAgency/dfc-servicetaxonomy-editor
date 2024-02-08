using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.TriageTool.GraphQL
{
    public class PageInputObjectType : WhereInputObjectGraphType<PagePart>
    {
        public PageInputObjectType(IStringLocalizer<PageInputObjectType> S)
        {
            Name = $"{nameof(PagePart)}Input";
            Description = S["The custom URL part of the content item"];
            AddScalarFilterFields<BooleanGraphType>("useInTriageTool", S["useInTriageTool"]);
        }
    }
}
