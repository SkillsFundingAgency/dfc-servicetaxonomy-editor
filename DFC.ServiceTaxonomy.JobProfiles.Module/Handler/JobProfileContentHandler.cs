using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handler
{
    internal class JobProfileContentHandler : ContentHandlerBase
    {
        public override Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (context.ContentItem.ContentType == "SOCCode")
            {
                var socCode = (string)context.ContentItem.Content.SOCCode.SOCCodeTextField.Text;
                if(!string.IsNullOrEmpty(socCode))
                {
                    // Need to do our stuff

                    // Get the ONET code

                    // Create the SOC Skills Matrix Content items
                }
            }
            return Task.CompletedTask;
        }
    }
}
