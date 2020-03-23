using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.Localization;
using MoreLinq.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;
using YesSql.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class AuditSyncIssuesTask : TaskActivity
    {
        public AuditSyncIssuesTask(
            ISession session,
            IMergeGraphSyncer mergeGraphSyncer,
            IStringLocalizer<AuditSyncIssuesTask> localizer,
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncValidator graphSyncValidator,
            IDeleteGraphSyncer deleteGraphSyncer)
        {
            _session = session;
            _mergeGraphSyncer = mergeGraphSyncer;
            _graphSyncValidator = graphSyncValidator;
            _deleteGraphSyncer = deleteGraphSyncer;
            T = localizer;
            _syncFailedContentItems = new List<ContentItem>();
            _deleteSyncFailedContentItems = new List<ContentItem>();
            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        private readonly ISession _session;
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private readonly IGraphSyncValidator _graphSyncValidator;
        private readonly IDeleteGraphSyncer _deleteGraphSyncer;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and retry"];
        public override LocalizedString Category => T["Graph"];

        private readonly List<ContentItem> _syncFailedContentItems;
        private readonly List<ContentItem> _deleteSyncFailedContentItems;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var timestamp = DateTime.UtcNow;
            var log = (await _session.Query<AuditSyncLog>().FirstOrDefaultAsync()) ??
                      new AuditSyncLog() {LastSynced = DateTime.MinValue};

            await CheckCreatedOrUpdatedContentItems(log);

            await ResyncCreateOrUpdateFailures();

            await CheckDeletedContentItems();

            await ResyncDeleteFailures();

            //TODO : anything left in the collections are still failing to sync - report this somehow

            if (!_syncFailedContentItems.Any())
            {
                log.LastSynced = timestamp;
                _session.Save(log);
                await _session.CommitAsync();
            }

            return Outcomes("Done");
        }

        private async Task ResyncCreateOrUpdateFailures()
        {
            for (int i = 0; i < _syncFailedContentItems.Count; i++)
            {
                try
                {
                    var contentItem = _syncFailedContentItems[i];

                    await _mergeGraphSyncer.SyncToGraph(
                        contentItem.ContentType,
                        contentItem.ContentItemId,
                        contentItem.ContentItemVersionId,
                        contentItem.Content,
                        contentItem.CreatedUtc,
                        contentItem.ModifiedUtc);

                    _syncFailedContentItems.RemoveAt(i);

                    if (!await _graphSyncValidator.CheckIfContentItemSynced(contentItem))
                    {
                        _syncFailedContentItems.Add(contentItem);
                    }
                }
                catch
                {
                    //I don't want to do anything here, why won't this build without this comment?
                }
            }
        }

        private async Task ResyncDeleteFailures()
        {
            for (int i = 0; i < _deleteSyncFailedContentItems.Count; i++)
            {
                try
                {
                    var contentItem = _deleteSyncFailedContentItems[i];

                    await _deleteGraphSyncer.DeleteFromGraph(contentItem);

                    _deleteSyncFailedContentItems.RemoveAt(i);

                    if (!await _deleteGraphSyncer.VerifyDeletion(contentItem))
                    {
                        _deleteSyncFailedContentItems.Add(contentItem);
                    }
                }
                catch
                {
                    //I don't want to do anything here, why won't this build without this comment?
                }
            }
        }

        private async Task CheckCreatedOrUpdatedContentItems(AuditSyncLog log)
        {
            var contentItems = await _session
                //do we only care about the latest published items?
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType.IsIn(_contentTypes.Keys) && x.Latest && x.Published &&
                    (x.CreatedUtc >= log.LastSynced || x.ModifiedUtc >= log.LastSynced))
                .ListAsync();

            foreach (var contentItem in contentItems)
            {
                if (!await _graphSyncValidator.CheckIfContentItemSynced(contentItem))
                {
                    _syncFailedContentItems.Add(contentItem);
                }
            }
        }

        private async Task CheckDeletedContentItems()
        {
            var contentItems = await _session
                .Query<ContentItem, ContentItemIndex>(x => !x.Published && !x.Latest)
                .ListAsync();

            foreach (var contentItem in contentItems.DistinctBy(x => x.ContentItemId).ToList())
            {
                var items = await _session
                    .Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == contentItem.ContentItemId)
                    .OrderBy(x => x.CreatedUtc)
                    .ListAsync();

                if (items.All(x => !x.Published && !x.Latest) && !await _deleteGraphSyncer.VerifyDeletion(contentItem))
                {
                    _deleteSyncFailedContentItems.Add(contentItem);
                }
            }
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
