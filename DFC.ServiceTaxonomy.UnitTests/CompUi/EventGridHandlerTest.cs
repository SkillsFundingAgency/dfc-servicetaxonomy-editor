using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Intefrace;
using DfE.NCS.Framework.Event.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public class EventGridHandlerTest
    {
        private readonly Mock<INcsEventGridClient> _mockClient;
        private readonly Mock<ILogger<EventGridHandler>> _mockLogger;
        private readonly EventGridHandler _eventGridHandler;

        public EventGridHandlerTest()
        {
            _mockClient = new Mock<INcsEventGridClient>();
            _mockLogger = new Mock<ILogger<EventGridHandler>>();
            _eventGridHandler = new EventGridHandler(_mockClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Publish_CreateAndSendMessageSuccessAsync()
        {
            var processing = new Processing
            {
                CurrentContent = "{\r\n  \"TitlePart\": {\r\n    \"Title\": \"Thank you for contacting us\"\r\n  },\r\n  \"Page\": {\r\n    \"PageLocations\": {\r\n      \"TaxonomyContentItemId\": \"4eembshqzx66drajtdten34tc8\",\r\n      \"TermContentItemIds\": [\r\n        \"4pksnz9106ngbwq74w66snan5x\"\r\n      ]\r\n    },\r\n    \"Description\": {\r\n      \"Text\": null\r\n    },\r\n    \"Herobanner\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"ShowHeroBanner\": {\r\n      \"Value\": false\r\n    },\r\n    \"ShowBreadcrumb\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolSummary\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"UseInTriageTool\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolFilters\": {\r\n      \"ContentItemIds\": []\r\n    }\r\n  },\r\n  \"SitemapPart\": {\r\n    \"Priority\": 5,\r\n    \"OverrideSitemapConfig\": false,\r\n    \"ChangeFrequency\": 0,\r\n    \"Exclude\": false\r\n  },\r\n  \"FlowPart\": {\r\n    \"Widgets\": [\r\n      {\r\n        \"ContentItemId\": \"4nkxzsea11sf1xj3hasvjfzax1\",\r\n        \"ContentItemVersionId\": null,\r\n        \"ContentType\": \"HTMLShared\",\r\n        \"DisplayText\": \"\",\r\n        \"Latest\": false,\r\n        \"Published\": false,\r\n        \"ModifiedUtc\": \"2024-09-04T11:01:17.2280141Z\",\r\n        \"PublishedUtc\": null,\r\n        \"CreatedUtc\": null,\r\n        \"Owner\": \"\",\r\n        \"Author\": \"JPrior\",\r\n        \"HTMLShared\": {\r\n          \"SharedContent\": {\r\n            \"ContentItemIds\": [\r\n              \"48n2h43p729ve15yfhhr3ccgmd\"\r\n            ]\r\n          }\r\n        },\r\n        \"GraphSyncPart\": {\r\n          \"Text\": \"<<contentapiprefix>>/htmlshared/7e8cc04c-7b6c-4ece-9be3-b95c10bebc36\"\r\n        },\r\n        \"FlowMetadata\": {\r\n          \"Alignment\": 3,\r\n          \"Size\": 100\r\n        }\r\n      }\r\n    ]\r\n  },\r\n  \"GraphSyncPart\": {\r\n    \"Text\": \"<<contentapiprefix>>/page/07664e63-deed-4d34-8f28-61c81dbd5310\"\r\n  },\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"thank-you-for-contacting-us\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"contact-us/thanks\",\r\n    \"FullUrl\": \"/contact-us/thank-you-for-contacting-us\",\r\n    \"UseInTriageTool\": false\r\n  },\r\n  \"ContentApprovalPart\": {\r\n    \"ReviewStatus\": 0,\r\n    \"ReviewType\": 0,\r\n    \"IsForcePublished\": false\r\n  },\r\n  \"AuditTrailPart\": {\r\n    \"Comment\": \"d\",\r\n    \"ShowComment\": false\r\n  }\r\n}",
                DisplayText = "TestHeader",
                Author = "UnitTest",
                ContentItemId = "6482381238adsdad",
                ContentType = "Header",
            };

            await _eventGridHandler.SendEventMessageAsync(processing, ContentEventType.StaxCreate);

            _mockClient.Verify(c => c.Publish(It.IsAny<ContentEvent>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Publish_HandlesNullContentAsync()
        {
            var processing = new Processing
            {
                DisplayText = "TestHeader",
                Author = "UnitTest",
                ContentItemId = "6482381238adsdad",
                ContentType = "Header",
            };

            await _eventGridHandler.SendEventMessageAsync(processing, ContentEventType.StaxCreate);

            _mockClient.Verify(c => c.Publish(It.IsAny<ContentEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
