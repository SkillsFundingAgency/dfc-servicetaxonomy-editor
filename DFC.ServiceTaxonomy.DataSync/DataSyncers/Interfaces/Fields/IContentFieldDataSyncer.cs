using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields
{
    public interface IContentFieldDataSyncer
    {
        string FieldTypeName {get;}

        Task AddSyncComponents(JObject contentItemField, IDataMergeContext context);

        Task AddSyncComponentsDetaching(IDataMergeContext context)
        {
            return Task.CompletedTask;
        }

        Task AddRelationship(JObject contentItemField, IDescribeRelationshipsContext itemContext)
        {
            return Task.CompletedTask;
        }

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context);
    }
}
