using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class DataMergeContext : DataSyncContext, IDataMergeItemSyncContext
    {
        public new IDataMergeContext? ParentContext { get; }
        public new IEnumerable<IDataMergeContext> ChildContexts => _childContexts.Cast<IDataMergeContext>();

        public IMergeDataSyncer MergeDataSyncer { get; }
        public IDataSyncReplicaSet DataSyncReplicaSet { get; }
        public IMergeNodeCommand MergeNodeCommand { get; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; }
        public List<ICommand> ExtraCommands { get; }

        public DataMergeContext(
            IMergeDataSyncer mergeDataSyncer,
            ISyncNameProvider syncNameProvider,
            IDataSyncReplicaSet dataSyncReplicaSet,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersionFactory contentItemVersionFactory,
            IDataMergeContext? parentDataSyncMergeContext,
            IServiceProvider serviceProvider)
            : base(
                contentItem,
                syncNameProvider,
                contentManager,
                contentItemVersionFactory.Get(dataSyncReplicaSet.Name),
                parentDataSyncMergeContext,
                serviceProvider.GetRequiredService<ILogger<DataMergeContext>>())
        {
            MergeDataSyncer = mergeDataSyncer;
            DataSyncReplicaSet = dataSyncReplicaSet;
            MergeNodeCommand = mergeNodeCommand;
            ReplaceRelationshipsCommand = replaceRelationshipsCommand;

            ExtraCommands = new List<ICommand>();
        }
    }
}
