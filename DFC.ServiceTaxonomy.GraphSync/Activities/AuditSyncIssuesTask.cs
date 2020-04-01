using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class AuditSyncIssuesTask : TaskActivity
    {
        public AuditSyncIssuesTask(
            ISession session,
            IStringLocalizer<AuditSyncIssuesTask> localizer,
            IContentDefinitionManager contentDefinitionManager,
            IValidateGraphSync validateGraphSync,
            IServiceProvider serviceProvider,
            INotifier notifier,
            ILogger<AuditSyncIssuesTask> logger)
        {
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
            _validateGraphSync = validateGraphSync;
            _serviceProvider = serviceProvider;
            _notifier = notifier;
            _logger = logger;
            T = localizer;
        }

        private readonly ISession _session;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IValidateGraphSync _validateGraphSync;
        private readonly IServiceProvider _serviceProvider;
        private readonly INotifier _notifier;
        private readonly ILogger _logger;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and retry"];
        public override LocalizedString Category => T["Graph"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        // this pr should fix the halted issue: https://github.com/OrchardCMS/OrchardCore/pull/5830
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
            #pragma warning disable CS0162
            if (false)
            {
                _logger.LogInformation($"{nameof(AuditSyncIssuesTask)} triggered");

                IEnumerable<ContentTypeDefinition> syncableContentTypeDefinitions = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)));

                DateTime timestamp = DateTime.UtcNow;
                AuditSyncLog auditSyncLog = await _session.Query<AuditSyncLog>().FirstOrDefaultAsync() ??
                                            new AuditSyncLog {LastSynced = SqlDateTime.MinValue.Value};

                foreach (ContentTypeDefinition contentTypeDefinition in syncableContentTypeDefinitions)
                {
                    //todo: do we want to batch up content items of type?
                    IEnumerable<ContentItem> contentTypeContentItems = await _session
                        //do we only care about the latest published items?
                        .Query<ContentItem, ContentItemIndex>(x =>
                            x.ContentType == contentTypeDefinition.Name
                            && x.Latest && x.Published
                            && (x.CreatedUtc >= auditSyncLog.LastSynced || x.ModifiedUtc >= auditSyncLog.LastSynced))
                        .ListAsync();

                    if (!contentTypeContentItems.Any())
                        continue;

                    List<ContentItem> syncFailedContentItems = new List<ContentItem>();

                    foreach (ContentItem contentItem in contentTypeContentItems)
                    {
                        //todo: do we need a new _validateGraphSync each time? don't think we do
                        if (!await _validateGraphSync.CheckIfContentItemSynced(contentItem))
                        {
                            syncFailedContentItems.Add(contentItem);
                        }
                    }

                    _logger.LogWarning(
                        $"{syncFailedContentItems} content items of type {contentTypeDefinition.Name} failed validation. Attempting to resync them");

                    // if this throws should we carry on?
                    foreach (ContentItem failedSyncContentItem in syncFailedContentItems)
                    {
                        var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                        await mergeGraphSyncer.SyncToGraph(
                            failedSyncContentItem.ContentType,
                            failedSyncContentItem.ContentItemId,
                            failedSyncContentItem.ContentItemVersionId,
                            failedSyncContentItem.Content,
                            failedSyncContentItem.CreatedUtc,
                            failedSyncContentItem.ModifiedUtc);

                        // do we want to double check sync was ok?
                        //if (!await _validateGraphSync.CheckIfContentItemSynced(contentItem))
                    }
                }

                auditSyncLog.LastSynced = timestamp;
                _session.Save(auditSyncLog);
                await _session.CommitAsync();
            }
            #pragma warning restore CS0162

                return Outcomes("Done");
            }
            catch
            {
                // this task will (at least initially) be triggered by a timer, so there won't be anywhere for the notification to be displayed
                // but add a notification in case we allow triggering from the ui
                //todo: check doesn't break timer triggered
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(SyncToGraphTask), $"Unable to verify graph sync."));
                throw;
            }

        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
