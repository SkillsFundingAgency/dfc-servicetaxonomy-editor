using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public interface IAuditTrailQueryService
    {
        Task<List<AuditTrailContentEvent>> GetVersions(string contentItemId);
    }
}
