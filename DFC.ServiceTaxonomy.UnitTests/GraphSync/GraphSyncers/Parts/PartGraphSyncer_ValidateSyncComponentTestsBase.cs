﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts
{
    public class PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public JObject Content { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public ISubgraph NodeWithRelationships { get; set; }
        public INode SourceNode { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; set; }
        public IValidateAndRepairContext ValidateAndRepairContext { get; set; }
        public IContentPartGraphSyncer? ContentPartGraphSyncer { get; set; }

        public PartGraphSyncer_ValidateSyncComponentTestsBase()
        {
            Content = JObject.Parse("{}");

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            ContentManager = A.Fake<IContentManager>();
            ContentItemVersion = A.Fake<IContentItemVersion>();

            SourceNode = A.Fake<INode>();
            NodeWithRelationships = A.Fake<ISubgraph>();
            A.CallTo(() => NodeWithRelationships.SourceNode).Returns(SourceNode);

            SyncNameProvider = A.Fake<ISyncNameProvider>();

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            ValidateAndRepairGraph = A.Fake<IValidateAndRepairGraph>();

            ValidateAndRepairContext = A.Fake<IValidateAndRepairContext>();
            A.CallTo(() => ValidateAndRepairContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => ValidateAndRepairContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => ValidateAndRepairContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
            A.CallTo(() => ValidateAndRepairContext.NodeWithRelationships).Returns(NodeWithRelationships);
            A.CallTo(() => ValidateAndRepairContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => ValidateAndRepairContext.GraphValidationHelper).Returns(GraphValidationHelper);
            A.CallTo(() => ValidateAndRepairContext.ExpectedRelationshipCounts).Returns(ExpectedRelationshipCounts);
            A.CallTo(() => ValidateAndRepairContext.ValidateAndRepairGraph).Returns(ValidateAndRepairGraph);
        }

        public Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            if (ContentPartGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentPartGraphSyncer to the IContentPartGraphSyncer you want to test dummy.");

            return ContentPartGraphSyncer.ValidateSyncComponent(
                Content,
                ValidateAndRepairContext);
        }
    }
}
