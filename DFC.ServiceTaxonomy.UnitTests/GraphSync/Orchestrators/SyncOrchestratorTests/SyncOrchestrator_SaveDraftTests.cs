﻿using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using FakeItEasy;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Orchestrators.SyncOrchestratorTests
{
    public class SyncOrchestrator_SaveDraftTests : SyncOrchestratorTestsBase
    {
        public SyncOrchestrator_SaveDraftTests()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.Matches(
                    t => t.Name == (nameof(IMergeGraphSyncer)))))
                .Returns(PreviewMergeGraphSyncer)
                .Once();
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.NotRequired, true)]
        public async Task SaveDraft_SyncAllowedMatrix_ReturnsBool(
            AllowSyncResult allowSyncAllowedResult,
            bool expectedSuccess)
        {
            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(allowSyncAllowedResult);

            bool success = await SyncOrchestrator.SaveDraft(ContentItem);

            Assert.Equal(expectedSuccess, success);
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, true)]
        [InlineData(AllowSyncResult.Blocked, false)]
        [InlineData(AllowSyncResult.NotRequired, false)]
        public async Task SaveDraft_SyncAllowedMatrix_SyncCalled(
            AllowSyncResult allowSyncAllowedResult,
            bool expectedSyncCalled)
        {
            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(allowSyncAllowedResult);

            await SyncOrchestrator.SaveDraft(ContentItem);

            A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                .MustHaveHappened(expectedSyncCalled?1:0, Times.Exactly);
        }

        [Fact]
        public async Task SaveDraft_MergeGraphSyncerThrows_ExceptionPropagates()
        {
            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(AllowSyncResult.Allowed);

            A.CallTo(() => PreviewMergeGraphSyncer.SyncToGraphReplicaSet())
                .Throws(() => new Exception());

            await Assert.ThrowsAsync<Exception>(() => SyncOrchestrator.SaveDraft(ContentItem));
        }

        [Theory]
        [InlineData(AllowSyncResult.Allowed, 1)]
        [InlineData(AllowSyncResult.Blocked, 0)]
        [InlineData(AllowSyncResult.NotRequired, 0)]
        public async Task SaveDraft_EventGridPublishingHandlerCalled(AllowSyncResult previewAllowSyncResult, int draftSavedCalled)
        {
            A.CallTo(() => PreviewAllowSync.Result)
                .Returns(previewAllowSyncResult);

            await SyncOrchestrator.SaveDraft(ContentItem);

            A.CallTo(() => EventGridPublishingHandler.DraftSaved(
                    A<IOrchestrationContext>.That.Matches(ctx => Equals(ctx.ContentItem, ContentItem))))
                .MustHaveHappened(draftSavedCalled, Times.Exactly);
        }
    }
}
