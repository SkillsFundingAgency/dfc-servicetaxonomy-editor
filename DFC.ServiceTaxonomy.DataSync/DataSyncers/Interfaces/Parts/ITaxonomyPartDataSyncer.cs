using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts
{
    public interface ITaxonomyPartDataSyncer : IContentPartDataSyncer
    {
        Task AddSyncComponentsForNonLeafEmbeddedTerm(JObject content, IDataMergeContext context);
        Task DeleteComponentsForNonLeafEmbeddedTerm(JObject content, IDataDeleteContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponentForNonLeafEmbeddedTerm(
            JObject content,
            IValidateAndRepairContext context);
    }
}
