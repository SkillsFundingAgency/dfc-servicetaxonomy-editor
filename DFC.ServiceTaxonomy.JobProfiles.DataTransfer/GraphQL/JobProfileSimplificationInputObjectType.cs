using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.GraphQL
{
    public class JobProfileSimplificationInputObjectType : WhereInputObjectGraphType<JobProfileSimplificationPart>
    {
        public JobProfileSimplificationInputObjectType(IStringLocalizer<JobProfileSimplificationPart> S)
        {
            Name = $"{nameof(JobProfileSimplificationPart)}Input";
            Description = S["the custom URL part of the content item"];

            AddScalarFilterFields<StringGraphType>("JobProfileCategory", S["JobProfileCategory"]);
        }
    }
}
