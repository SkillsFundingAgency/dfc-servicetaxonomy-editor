using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;


//Custom Media Field object type overriding existing graphql object type to expose the media field alt text field
namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.GraphQL
{
    public class SocSkillsMatrixQueryObjectType : ObjectGraphType<SocSkillMatrixContentItem>
    {
        public SocSkillsMatrixQueryObjectType()
        {
            Name = "NCSMediaField";

            Field<StringGraphType>()
                .Name("Contextualised")
                .Description("Custom Contextualised String")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Contextualised is null)
                    {
                        return string.Empty;
                    }
                    return x.Page(x.Source.Contextualised);
                });
        }
    }
}
