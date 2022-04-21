using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    //todo: should IDeleteDataSyncer implement IDataDeleteItemSyncContext??
    public class DataDeleteContext : DataSyncContext, IDataDeleteItemSyncContext
    {
        public new IDataDeleteContext? ParentContext { get; }
        public new IEnumerable<IDataDeleteContext> ChildContexts => _childContexts.Cast<IDataDeleteContext>();

        public IDeleteDataSyncer DeleteDataSyncer { get; }
        public IDeleteNodeCommand DeleteNodeCommand { get; }
        public SyncOperation SyncOperation { get; }
        public IEnumerable<KeyValuePair<string, object>>? DeleteIncomingRelationshipsProperties { get; }

        public DataDeleteContext(ContentItem contentItem,
            IDeleteNodeCommand deleteNodeCommand,
            IDeleteDataSyncer deleteDataSyncer,
            SyncOperation syncOperation,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties,
            IDataDeleteContext? parentDataSyncDeleteContext,
            IServiceProvider serviceProvider)
            : base(
                contentItem,
                syncNameProvider,
                contentManager,
                contentItemVersion,
                parentDataSyncDeleteContext,
                serviceProvider.GetRequiredService<ILogger<DataDeleteContext>>())
        {
            DeleteDataSyncer = deleteDataSyncer;
            DeleteNodeCommand = deleteNodeCommand;
            SyncOperation = syncOperation;
            DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;
        }
    }
}
