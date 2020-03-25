using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
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
            ILogger<AuditSyncIssuesTask> logger)
        {
            _session = session;
            _mergeGraphSyncer = mergeGraphSyncer;
            _contentDefinitionManager = contentDefinitionManager;
            _graphSyncValidator = graphSyncValidator;
            _logger = logger;
            T = localizer;
            _syncFailedContentItems = new List<ContentItem>();
        }

        private readonly ISession _session;
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncValidator _graphSyncValidator;
        private readonly ILogger _logger;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and retry"];
        public override LocalizedString Category => T["Graph"];

        private readonly List<ContentItem> _syncFailedContentItems;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _logger.LogInformation($"{nameof(AuditSyncIssuesTask)} triggered");

             var contentTypes = _contentDefinitionManager
                 .ListTypeDefinitions()
                 .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                 .ToDictionary(x => x.Name);

             var timestamp = DateTime.UtcNow;
             var log = await _session.Query<AuditSyncLog>().FirstOrDefaultAsync() ??
                       new AuditSyncLog {LastSynced = SqlDateTime.MinValue.Value};

             //todo: do we want to load 10s thousands at once?? load content type at a time?? batch them up??
             var contentItems = await _session
                 //do we only care about the latest published items?
                 .Query<ContentItem, ContentItemIndex>(x =>
                     x.ContentType.IsIn(contentTypes.Keys) && x.Latest && x.Published && (x.CreatedUtc >= log.LastSynced ||  x.ModifiedUtc >= log.LastSynced))
                 .ListAsync();

             foreach (var contentItem in contentItems)
             {
                 //todo: do we need a new _graphSyncValidator each time?
                 if (!await _graphSyncValidator.CheckIfContentItemSynced(contentItem))
                 {
                     _syncFailedContentItems.Add(contentItem);
                 }
             }

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

             //anything left in the syncFailedContentItems collection is still failing to sync - report this somehow

             log.LastSynced = timestamp;
             _session.Save(log);
             await _session.CommitAsync();

            return Outcomes("Done");
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
