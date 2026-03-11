using DfE.NCS.Framework.Core.Constants;
using DfE.NCS.Framework.Core.Preview.Interfaces;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Services
{
    public class CmsPreviewHandler : ICmsPreviewHandler
    {
        /// <summary>
        /// Gets the graph ql status.
        /// </summary>
        /// <returns>string.</returns>
        public string GetGraphQLStatus()
        {
            return NcsGraphQLStatus.Published;
        }

        /// <summary>
        /// Determines whether [is preivew mode].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is preivew mode]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPreivewMode()
        {
            return false;
        }
    }
}
