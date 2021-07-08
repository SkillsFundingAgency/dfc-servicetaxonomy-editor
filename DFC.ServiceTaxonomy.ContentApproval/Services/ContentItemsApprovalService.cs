using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using ISession = YesSql.ISession;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public class ContentItemsApprovalService : IContentItemsApprovalService
    {
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentItemsApprovalService(
            ISession session,
            IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ContentItemsApprovalCounts> GetManageContentItemCounts(DashboardItemsStatusCard card)
        {
            ContentItemsApprovalCounts counts = new ContentItemsApprovalCounts();

            switch (card)
            {
                case DashboardItemsStatusCard.InDraft:
                    counts.Count = GetManageContentItemCount(card);
                    break;
                case DashboardItemsStatusCard.Published:
                    counts.Count =  GetManageContentItemCount(card);
                    var forcePublishedCount = GetManageContentItemCount(card, null, true);
                    counts.SubCounts = new[] { counts.Count, forcePublishedCount };
                    break;
                case DashboardItemsStatusCard.WaitingForReview:
                case DashboardItemsStatusCard.InReview:
                    var reviewTypes = Enum.GetValues(typeof(ReviewType)).Cast<ReviewType>();
                    var reviewTypeCounts = reviewTypes.Select(rt => GetManageContentItemCount(card, rt==ReviewType.None?(ReviewType?)null:rt)).ToArray();
                    counts.Count = reviewTypeCounts[0];
                    counts.SubCounts = reviewTypeCounts;
                    break;
                case DashboardItemsStatusCard.MyWorkItems:
                    counts.MyItems = await GetMyContent();
                    break;
                case DashboardItemsStatusCard.Deleted:
                    counts.Count = GetManageContentItemCount(card);
                    break;
            }

            return counts;
        }

        private async Task<List<ContentItem>> GetMyContent()
        {
            var myContent = new List<ContentItem>();
            var user = _httpContextAccessor.HttpContext?.User.Identity.Name ?? string.Empty;

            var query = _session.Query<ContentItem>().With<ContentItemIndex>(i => i.Author == user && i.Latest);
            await foreach (var item in  query.OrderByDescending(c => c.ModifiedUtc).ToAsyncEnumerable())
            {
                // For some reason can't filter DisplayText in the query so having to check here and enforce the count limit
                if (!string.IsNullOrWhiteSpace(item.DisplayText))
                {
                    myContent.Add(item);
                }

                if (myContent.Count == 10)
                {
                    break;
                }
            }
            return myContent;
        }

        private int GetManageContentItemCount(
            DashboardItemsStatusCard card,
            ReviewType? reviewType = null,
            bool isForcePublished = false)
        {
            if (card == DashboardItemsStatusCard.Deleted)
            {
                // Deleted content item data is requested from the Audit Trail indexes so separate code for this.
                var auditQuerySession = _session.Query<AuditTrailEvent>(AuditTrailEvent.Collection);
                auditQuerySession.With<AuditTrailEventIndex>(i => i.Name == "Removed");
                return auditQuerySession.CountAsync().Result;
            }

            var querySession =  _session.Query<ContentItem>();
            switch (card)
            {
                case DashboardItemsStatusCard.WaitingForReview:
                    querySession.With<ContentItemIndex>(i => i.Latest); // Equivalent to Orchard Core All Versions filter; this excludes deleted content items
                    querySession.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ReviewStatus.ReadyForReview);
                    if (reviewType != null)
                    {
                        querySession.With<ContentApprovalPartIndex>(i => i.ReviewType == (int)reviewType);
                    }
                    break;
                case DashboardItemsStatusCard.InReview:
                    querySession.With<ContentItemIndex>(i => i.Latest); // Equivalent to Orchard Core All Versions filter; this excludes deleted content items
                    querySession.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ReviewStatus.InReview);
                    if (reviewType != null)
                    {
                        querySession.With<ContentApprovalPartIndex>(i => i.ReviewType == (int)reviewType);
                    }
                    break;
                case DashboardItemsStatusCard.InDraft:
                    querySession.With<ContentItemIndex>(i => i.Latest && !i.Published); // Equivalent to Orchard Core Draft filter
                    break;
                case DashboardItemsStatusCard.Published:
                    querySession.With<ContentItemIndex>(i => i.Published); // Equivalent to Orchard Core Published filter
                    if (isForcePublished)
                    {
                        querySession.With<ContentApprovalPartIndex>(i => i.IsForcePublished);
                    }
                    break;
                default:
                    querySession.With<ContentItemIndex>(i => i.Latest);
                    break;
            }

            return querySession.CountAsync().Result;
        }
    }
}
