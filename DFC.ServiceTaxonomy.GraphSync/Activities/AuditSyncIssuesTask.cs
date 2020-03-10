using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Localization;
using Neo4j.Driver;
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
            IGraphDatabase graphDatabase,
            IMergeGraphSyncer mergeGraphSyncer,
            IStringLocalizer<AuditSyncIssuesTask> localizer,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers)
        {
            _session = session;
            _graphDatabase = graphDatabase;
            _mergeGraphSyncer = mergeGraphSyncer;
            T = localizer;
            _partSyncers = partSyncers.ToDictionary(x => x.PartName ?? "Eponymous");
            _syncFailedContentItems = new List<ContentItem>();
            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == "GraphSyncPart"))
                .ToDictionary(x => x.Name);
        }

        private readonly ISession _session;
        private readonly IGraphDatabase _graphDatabase;
        private readonly Dictionary<string, IContentPartGraphSyncer> _partSyncers;
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private IStringLocalizer T { get; }

        public override string Name => nameof(AuditSyncIssuesTask);
        public override LocalizedString DisplayText => T["Identify sync issues and retry"];
        public override LocalizedString Category => T["National Careers Service"];

        private readonly List<ContentItem> _syncFailedContentItems;
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

            var contentItems = await _session
                //do we only care about the latest published items?
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType.IsIn(_contentTypes.Keys) && x.Latest && x.Published && (x.CreatedUtc >= log.LastSynced ||  x.ModifiedUtc >= log.LastSynced))
                .ListAsync();

            foreach (var contentItem in contentItems)
            {
                await CheckIfContentItemSynced(contentItem);
            }

            foreach (var contentItem in _syncFailedContentItems)
            {
                try
                {
                    await _mergeGraphSyncer.SyncToGraph(contentItem.ContentType, contentItem.Content,
                        contentItem.CreatedUtc, contentItem.ModifiedUtc);

                    _syncFailedContentItems.Remove(contentItem);

                    await CheckIfContentItemSynced(contentItem);
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

        private async Task CheckIfContentItemSynced(ContentItem contentItem)
        {
            var results = await _graphDatabase.RunReadQuery(new MatchNodeWithAllOutgoingRelationshipsQuery(contentItem.ContentType, (string)contentItem.Content.GraphSyncPart.Text));

            if (results == null || !results.Any())
            {
                _syncFailedContentItems.Add(contentItem);
            }
            else
            {
                var contentDefinition = _contentTypes[contentItem.ContentType];

                var sourceNode = results.Select(x => x[0]).Cast<INode>().First();
                var relationships = results.Select(x => x[1]).Cast<IRelationship>().ToList();
                var destNodes = results.Select(x => x[2]).Cast<INode>().ToList();

                foreach (var part in contentDefinition.Parts)
                {
                    if (!_partSyncers.TryGetValue(part.PartDefinition.Name, out var partSyncer))
                    {
                        partSyncer = _partSyncers["Eponymous"];
                    }

                    if (!await partSyncer.VerifySyncComponent(contentItem, sourceNode, part, relationships, destNodes))
                    {
                        _syncFailedContentItems.Add(contentItem);
                        break;
                    }
                }
            }
        }
    }

    public class AuditSyncLog
    {
        public DateTime LastSynced { get; set; }
    }
}
