using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
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
        public IGraphReplicaSet GraphReplicaSet { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IGraphMergeContext GraphMergeContext { get; set; }

        public IContentFieldGraphSyncer? ContentFieldGraphSyncer { get; set; }

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

            GraphReplicaSet = A.Fake<IGraphReplicaSet>();

            //todo: this isn't going to work!
            ContentItem = A.Fake<ContentItem>();
            ContentManager = A.Fake<IContentManager>();
            ContentItemVersion = A.Fake<IContentItemVersion>();

            GraphMergeContext = A.Fake<IGraphMergeContext>();
            A.CallTo(() => GraphMergeContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => GraphMergeContext.GraphReplicaSet).Returns(GraphReplicaSet);
            A.CallTo(() => GraphMergeContext.MergeNodeCommand).Returns(MergeNodeCommand);
            A.CallTo(() => GraphMergeContext.ReplaceRelationshipsCommand).Returns(ReplaceRelationshipsCommand);
            A.CallTo(() => GraphMergeContext.ContentItem).Returns(ContentItem);
            A.CallTo(() => GraphMergeContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => GraphMergeContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => GraphMergeContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
            A.CallTo(() => GraphMergeContext.ContentPartFieldDefinition).Returns(ContentPartFieldDefinition);
        }

        public Task CallAddSyncComponents()
        {
            if (ContentFieldGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentFieldGraphSyncer to the IContentFieldGraphSyncer you want to test dummy.");

            return ContentFieldGraphSyncer.AddSyncComponents(ContentItemField!, GraphMergeContext);
        }
    }
}
