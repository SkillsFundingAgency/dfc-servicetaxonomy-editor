using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
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
    public class PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public JObject Content { get; set; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public IContentItemVersion ContentItemVersion { get; set; }
        public ISubDataSync NodeWithRelationships { get; set; }
        public INode SourceNode { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IDataSyncValidationHelper DataSyncValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public IValidateAndRepairData ValidateAndRepairData { get; set; }
        public IValidateAndRepairContext ValidateAndRepairContext { get; set; }
        public IContentPartDataSyncer? ContentPartGraphSyncer { get; set; }

        public PartGraphSyncer_ValidateSyncComponentTestsBase()
        {
            Content = JObject.Parse("{}");

            ContentTypePartDefinition = A.Fake<ContentTypePartDefinition>();

            ContentManager = A.Fake<IContentManager>();
            ContentItemVersion = A.Fake<IContentItemVersion>();

            SourceNode = A.Fake<INode>();
            NodeWithRelationships = A.Fake<ISubDataSync>();
            A.CallTo(() => NodeWithRelationships.SourceNode).Returns(SourceNode);

            SyncNameProvider = A.Fake<ISyncNameProvider>();

            DataSyncValidationHelper = A.Fake<IDataSyncValidationHelper>();

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            ValidateAndRepairData = A.Fake<IValidateAndRepairData>();

            ValidateAndRepairContext = A.Fake<IValidateAndRepairContext>();
            A.CallTo(() => ValidateAndRepairContext.ContentManager).Returns(ContentManager);
            A.CallTo(() => ValidateAndRepairContext.ContentItemVersion).Returns(ContentItemVersion);
            A.CallTo(() => ValidateAndRepairContext.ContentTypePartDefinition).Returns(ContentTypePartDefinition);
            A.CallTo(() => ValidateAndRepairContext.NodeWithRelationships).Returns(NodeWithRelationships);
            A.CallTo(() => ValidateAndRepairContext.SyncNameProvider).Returns(SyncNameProvider);
            A.CallTo(() => ValidateAndRepairContext.DataSyncValidationHelper).Returns(DataSyncValidationHelper);
            A.CallTo(() => ValidateAndRepairContext.ExpectedRelationshipCounts).Returns(ExpectedRelationshipCounts);
            A.CallTo(() => ValidateAndRepairContext.ValidateAndRepairData).Returns(ValidateAndRepairData);
        }

        public Task<(bool validated, string failureReason)> CallValidateSyncComponent()
        {
            if (ContentPartGraphSyncer == null)
                throw new InvalidOperationException("You must set ContentPartDataSyncer to the IContentPartDataSyncer you want to test dummy.");

            return ContentPartGraphSyncer.ValidateSyncComponent(
                Content,
                ValidateAndRepairContext);
        }
    }
}
