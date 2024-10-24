using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Banners.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.Banners.GraphQL
{
    public class BannerPartInputObjectType : WhereInputObjectGraphType<BannerPart>
    {
        public BannerPartInputObjectType(IStringLocalizer<BannerPartInputObjectType> S)
        {
            Name = $"{nameof(BannerPart)}Input";
            Description = S["the custom URL part of the content item"];

            AddScalarFilterFields<StringGraphType>("webPageName", S["Web Page Name"]);
            AddScalarFilterFields<StringGraphType>("webPageURL", S["Web Page URL"]);
        }
    }
}
