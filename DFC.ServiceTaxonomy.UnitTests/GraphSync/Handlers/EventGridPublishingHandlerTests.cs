using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Models;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Handlers;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.Handlers
{
    public class EventGridPublishingHandlerTests
    {
        public EventGridPublishingHandler EventGridPublishingHandler { get; set; }
        public EventGridConfiguration EventGridConfiguration { get; set; }
        public IOptionsMonitor<EventGridConfiguration> EventGridConfigurationOptionsMonitor { get; set; }
        public IEventGridContentClient EventGridContentClient { get; set; }
        public string UserId { get; set; }
        public ISyncNameProvider SyncNameProvider { get; set; }
        public IPublishedContentItemVersion PublishedContentItemVersion { get; set; }
        public IPreviewContentItemVersion PreviewContentItemVersion { get; set; }
        public INeutralEventContentItemVersion NeutralEventContentItemVersion { get; set; }
        public ILogger<EventGridPublishingHandler> Logger { get; set; }
        public IOrchestrationContext OrchestrationContext { get; set; }
        public ContentItem ContentItem { get; set; }

        public EventGridPublishingHandlerTests()
        {
            EventGridConfigurationOptionsMonitor = A.Fake<IOptionsMonitor<EventGridConfiguration>>();

            EventGridConfiguration = new EventGridConfiguration();
            A.CallTo(() => EventGridConfigurationOptionsMonitor.CurrentValue)
                .Returns(EventGridConfiguration);

            EventGridContentClient = A.Fake<IEventGridContentClient>();

            SyncNameProvider = A.Fake<ISyncNameProvider>();
            UserId = "C7FE81FB-CF2B-4897-ABE9-1EB931A31371";
            A.CallTo(() => SyncNameProvider.GetEventIdPropertyValue(
                A<JObject>._,
                A<IContentItemVersion>._)).Returns(UserId);

            PublishedContentItemVersion = A.Fake<IPublishedContentItemVersion>();
            PreviewContentItemVersion = A.Fake<IPreviewContentItemVersion>();
            NeutralEventContentItemVersion = A.Fake<INeutralEventContentItemVersion>();
            Logger = A.Fake<ILogger<EventGridPublishingHandler>>();

            EventGridPublishingHandler = new EventGridPublishingHandler(
                EventGridConfigurationOptionsMonitor,
                EventGridContentClient,
                SyncNameProvider,
                PublishedContentItemVersion,
                PreviewContentItemVersion,
                NeutralEventContentItemVersion,
                Logger);

            OrchestrationContext = A.Fake<IOrchestrationContext>();
            ContentItem = new ContentItem
            {
                ContentType = "ContentType",
                ContentItemVersionId = "ContentItemVersionId",
                DisplayText = "DisplayText",
                Author = "Author"
            };
            this.ContentItem.Content.GraphSyncPart = new JObject();

            A.CallTo(() => OrchestrationContext.ContentItem)
                .Returns(ContentItem);

        }

        [Fact]
        public async Task DraftSaved_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.DraftSaved(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task DraftSaved_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.DraftSaved(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Published_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.Published(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Published_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.Published(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Cloned_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.Cloned(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Cloned_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.Cloned(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Deleted_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.Deleted(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Deleted_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.Deleted(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Unpublished_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.Unpublished(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Unpublished_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.Unpublished(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DraftDiscarded_PublishEventsDisabled_NoEventPublished()
        {
            await EventGridPublishingHandler.DraftDiscarded(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task DraftDiscarded_PublishEventsEnabled_EventPublished()
        {
            EventGridConfiguration.PublishEvents = true;

            await EventGridPublishingHandler.DraftDiscarded(OrchestrationContext);

            A.CallTo(() => EventGridContentClient.Publish(A<ContentEvent>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        //todo: more tests required
    }
}
