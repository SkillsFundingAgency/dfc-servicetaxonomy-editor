using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class JobProfileSimplificationQueryObjectType : ObjectGraphType<JobProfileSimplificationPart>
    {
        public JobProfileSimplificationQueryObjectType()
        {
            Name = "JobProfileSimplificationPart";

            Field(x => x.VideoThumbnailText, nullable: true);

        }
    }
        
    
}
