using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
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
    //todo: some tests are basically common - move into here
    public class FieldGraphSyncer_ValidateSyncComponentTestsBase
    {
        public JObject ContentItemField { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition ContentPartFieldDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public ISubDataSync NodeWithRelationships { get; set; }
        public INode SourceNode { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IDataSyncValidationHelper DataSyncValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public IContentFieldDataSyncer? ContentFieldGraphSyncer { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public IValidateAndRepairData ValidateAndRepairData { get; set; }
        public IValidateAndRepairContext ValidateAndRepairContext { get; set; }

        public const string FieldNameBase = "baseFieldName";
        public const string FieldNameTransformed = "transformedFieldName";

        public FieldGraphSyncer_ValidateSyncComponentTestsBase()
        {
            ContentItemField = JObject.Parse("{}");

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            ContentPartFieldDefinition = A.Fake<IContentPartFieldDefinition>();
            A.CallTo(() => ContentPartFieldDefinition.Name).Returns(FieldNameBase);

            ContentManager = A.Fake<IContentManager>();

            SourceNode = A.Fake<INode>();
            NodeWithRelationships = A.Fake<ISubDataSync>();
            A.CallTo(() => NodeWithRelationships.SourceNode).Returns(SourceNode);

            SyncNameProvider = A.Fake<ISyncNameProvider>();
            A.CallTo(() => SyncNameProvider.PropertyName(FieldNameBase)).Returns(FieldNameTransformed);

            DataSyncValidationHelper = A.Fake<IDataSyncValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            ContentItemVersion = A.Fake<IContentItemVersion>();

            ValidateAndRepairData = A.Fake<IValidateAndRepairData>();

            ValidateAndRepairContext = A.Fake<IValidateAndRepairContext>();
            A.CallTo(() => ValidateAndRepairContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => ValidateAndRepairContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => ValidateAndRepairContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => ValidateAndRepairContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
            A.CallTo(() => ValidateAndRepairContext.ContentPartFieldDefinition).Returns(ContentPartFieldDefinition);
            A.CallTo(() => ValidateAndRepairContext.NodeWithRelationships).Returns(NodeWithRelationships);
            A.CallTo(() => ValidateAndRepairContext.DataSyncValidationHelper).Returns(DataSyncValidationHelper);
            A.CallTo(() => ValidateAndRepairContext.ExpectedRelationshipCounts).Returns(ExpectedRelationshipCounts);
            A.CallTo(() => ValidateAndRepairContext.ValidateAndRepairData).Returns(ValidateAndRepairData);
        }

        public Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            if (ContentFieldGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentFieldGraphSyncer to the IContentFieldDataSyncer you want to test dummy.");

            return ContentFieldGraphSyncer.ValidateSyncComponent(ContentItemField, ValidateAndRepairContext);
        }
    }
}
