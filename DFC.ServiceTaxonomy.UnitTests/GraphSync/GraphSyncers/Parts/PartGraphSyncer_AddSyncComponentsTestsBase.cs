using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts
{
    public class PartGraphSyncer_AddSyncComponentsTestsBase
    {
        public JObject Content { get; set; }
        public ContentItem ContentItem { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        //todo: do we need to introduce a IContentTypePartDefinition (like ContentTypePartDefinition)
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IDataSyncReplicaSet DataSyncReplicaSet { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IDataMergeContext DataMergeContext { get; set; }

        public IContentPartDataSyncer? ContentPartGraphSyncer { get; set; }

        public PartGraphSyncer_AddSyncComponentsTestsBase()
        {
            Content = JObject.Parse("{}");

            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            SyncNameProvider = A.Fake<ISyncNameProvider>();

            DataSyncReplicaSet = A.Fake<IDataSyncReplicaSet>();

            ContentItem = A.Fake<ContentItem>();
            ContentManager = A.Fake<IContentManager>();
            ContentItemVersion = A.Fake<IContentItemVersion>();

            DataMergeContext = A.Fake<IDataMergeContext>();
            A.CallTo(() => DataMergeContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => DataMergeContext.DataSyncReplicaSet).Returns(DataSyncReplicaSet);
            A.CallTo(() => DataMergeContext.MergeNodeCommand).Returns(MergeNodeCommand);
            A.CallTo(() => DataMergeContext.ReplaceRelationshipsCommand).Returns(ReplaceRelationshipsCommand);
            A.CallTo(() => DataMergeContext.ContentItem).Returns(ContentItem);
            A.CallTo(() => DataMergeContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => DataMergeContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => DataMergeContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
        }

        public Task CallAddSyncComponents()
        {
            if (ContentPartGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentPartDataSyncer to the IContentPartDataSyncer you want to test dummy.");

            return ContentPartGraphSyncer.AddSyncComponents(Content!, DataMergeContext);
        }
    }
}
