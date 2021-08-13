using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Entities;
using YesSql;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public class AuditTrailQueryService : IAuditTrailQueryService
    {
        private readonly ISession _session;
        private const string ContentCategory = "Content";

        public AuditTrailQueryService(ISession session)
        {
            _session = session;
        }

        public async Task<List<AuditTrailContentEvent>> GetVersions(string contentItemId)
        {
            var allAuditVersions = await _session
                .Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index =>
                    index.Category == ContentCategory &&
                    index.CorrelationId == contentItemId)
                .OrderByDescending(index => index.Id)
                .ListAsync();

            if (allAuditVersions == null || !allAuditVersions.Any())
            {
                throw new DataException(
                    $"No audit trail version information was found in the audit trail index for {contentItemId}");
            }

            var auditTrailContentEventsList = new List<AuditTrailContentEvent>();

            foreach (var auditTrailEvent in allAuditVersions)
            {
                var auditTrailContentEvent = auditTrailEvent.As<AuditTrailContentEvent>();
                if (auditTrailContentEventsList.All(k => k.VersionNumber != auditTrailContentEvent.VersionNumber))
                {
                    auditTrailContentEventsList.Add(auditTrailContentEvent);
                }
            }

            return auditTrailContentEventsList;
        }
    }
}
