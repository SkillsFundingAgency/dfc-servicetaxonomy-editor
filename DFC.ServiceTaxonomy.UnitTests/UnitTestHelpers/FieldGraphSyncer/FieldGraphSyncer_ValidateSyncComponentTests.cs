﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using FakeItEasy;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.UnitTests.UnitTestHelpers.FieldGraphSyncer
{
    //todo: some tests are basically common - move into here
    public class FieldGraphSyncer_ValidateSyncComponentTests
    {
        public JObject ContentItemField { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public INode SourceNode { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public IContentFieldGraphSyncer? ContentFieldGraphSyncer { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; set; }
        public IValidateAndRepairContext ValidateAndRepairContext { get; set; }

        public const string FieldNameBase = "baseFieldName";
        public const string FieldNameTransformed = "transformedFieldName";

        public FieldGraphSyncer_ValidateSyncComponentTests()
        {
            ContentItemField = JObject.Parse("{}");

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(FieldNameBase);

            ContentManager = A.Fake<IContentManager>();

            SourceNode = A.Fake<INode>();
            NodeWithOutgoingRelationships = A.Fake<INodeWithOutgoingRelationships>();
            A.CallTo(() => NodeWithOutgoingRelationships.SourceNode).Returns(SourceNode);

            SyncNameProvider = A.Fake<ISyncNameProvider>();
            A.CallTo(() => SyncNameProvider.PropertyName(FieldNameBase)).Returns(FieldNameTransformed);

            GraphValidationHelper = A.Fake<IGraphValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            ContentItemVersion = A.Fake<IContentItemVersion>();

            ValidateAndRepairGraph = A.Fake<IValidateAndRepairGraph>();

            ValidateAndRepairContext = A.Fake<IValidateAndRepairContext>();
            A.CallTo(() => ValidateAndRepairContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => ValidateAndRepairContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => ValidateAndRepairContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => ValidateAndRepairContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
            A.CallTo(() => ValidateAndRepairContext.ContentPartFieldDefinition).Returns(ContentPartFieldDefinition);
            A.CallTo(() => ValidateAndRepairContext.NodeWithOutgoingRelationships).Returns(NodeWithOutgoingRelationships);
            A.CallTo(() => ValidateAndRepairContext.GraphValidationHelper).Returns(GraphValidationHelper);
            A.CallTo(() => ValidateAndRepairContext.ExpectedRelationshipCounts).Returns(ExpectedRelationshipCounts);
            A.CallTo(() => ValidateAndRepairContext.ValidateAndRepairGraph).Returns(ValidateAndRepairGraph);
        }

        public async Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            if (ContentFieldGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentFieldGraphSyncer to the IContentFieldGraphSyncer you want to test dummy.");

            return await ContentFieldGraphSyncer.ValidateSyncComponent(ContentItemField, ValidateAndRepairContext);
        }
    }
}
