﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
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
        public IGraphReplicaSet GraphReplicaSet { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IGraphMergeContext GraphMergeContext { get; set; }

        public IContentPartGraphSyncer? ContentPartGraphSyncer { get; set; }

        public PartGraphSyncer_AddSyncComponentsTestsBase()
        {
            Content = JObject.Parse("{}");

            MergeNodeCommand = A.Fake<IMergeNodeCommand>();
            //todo: best way to do this?
            MergeNodeCommand.Properties = new Dictionary<string, object>();

            ReplaceRelationshipsCommand = A.Fake<IReplaceRelationshipsCommand>();

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            SyncNameProvider = A.Fake<ISyncNameProvider>();

            GraphReplicaSet = A.Fake<IGraphReplicaSet>();

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
        }

        public Task CallAddSyncComponents()
        {
            if (ContentPartGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentPartGraphSyncer to the IContentPartGraphSyncer you want to test dummy.");

            return ContentPartGraphSyncer.AddSyncComponents(Content!, GraphMergeContext);
        }
    }
}
