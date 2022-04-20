using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.OrchardCore.Interfaces;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields
{
    //todo: inheritance or composition?
    public class FieldGraphSyncer_AddSyncComponentsTestsBase
    {
        //todo: rename
        public JObject? ContentItemField { get; set; }
        public ContentItem ContentItem { get; set; }
        public IMergeNodeCommand MergeNodeCommand { get; set; }
        public IReplaceRelationshipsCommand ReplaceRelationshipsCommand { get; set; }
        //todo: do we need to introduce a IContentTypePartDefinition (like ContentTypePartDefinition)
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IDataSyncReplicaSet DataSyncReplicaSet { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IDataMergeContext DataMergeContext { get; set; }

        public IContentFieldDataSyncer? ContentFieldGraphSyncer { get; set; }

        //todo: rename (form and to NodePropertyName?
        public const string _fieldName = "TestField";

        public FieldGraphSyncer_AddSyncComponentsTestsBase()
        {
            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(_fieldName);

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            SyncNameProvider = A.Fake<ISyncNameProvider>();
            A.CallTo(() => SyncNameProvider.PropertyName(_fieldName)).Returns(_fieldName);

            DataSyncReplicaSet = A.Fake<IDataSyncReplicaSet>();

            //todo: this isn't going to work!
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
            A.CallTo(() => DataMergeContext.ContentPartFieldDefinition).Returns(ContentPartFieldDefinition);
        }

        public Task CallAddSyncComponents()
        {
            if (ContentFieldGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentFieldGraphSyncer to the IContentFieldDataSyncer you want to test dummy.");

            return ContentFieldGraphSyncer.AddSyncComponents(ContentItemField!, DataMergeContext);
        }
    }
}
