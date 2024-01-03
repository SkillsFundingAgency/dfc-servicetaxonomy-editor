using DFC.ServiceTaxonomy.GraphSync.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.GraphSync.GraphQL
{
    public class GraphSyncPartInputObjectType : WhereInputObjectGraphType<GraphSyncPart>
    {
        public GraphSyncPartInputObjectType(IStringLocalizer<GraphSyncPartInputObjectType> stringLocalizer)
        {
            Name = $"{nameof(GraphSyncPart)}Input";
            Description = stringLocalizer["CompUI Id"];
            AddScalarFilterFields<StringGraphType>("NodeId", stringLocalizer["Id"]);
        }
    }
}




