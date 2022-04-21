using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers
{
    public interface IContentFieldsDataSyncer
    {
        Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync)
            => Task.CompletedTask;

        Task AddSyncComponents(JObject content, IDataMergeContext context);

        Task AddRelationship(JObject content, IDescribeRelationshipsContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
