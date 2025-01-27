using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using AutoMapper;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Handlers;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public partial class CompuiHandlerTests
    {
        private readonly IDbConnectionAccessor _fakeDbaAccessor;
        private readonly ILogger<CacheHandler> _fakeLogger;
        private readonly IDapperWrapper _fakeDapperWrapper;
        private readonly ICacheHandler _fakeCacheHandler;
        private readonly ISharedContentRedisInterface _fakeSharedContentRedisInterface;
        private readonly IEventGridHandler _fakeEventHandler;
        public readonly IMapper _mapper;
        public readonly IBuilder _fakeBuilder;
        public readonly IDirector _fakeDirector;
        public readonly IConfiguration _configuration;
        private readonly IDataService _relatedContentItemIndexRepository;
        private readonly IEventGridHandler _eventGridHandler;

        public CompuiHandlerTests()
        {
            _fakeDbaAccessor = A.Fake<IDbConnectionAccessor>();
            _fakeLogger = A.Fake<ILogger<CacheHandler>>();
            _fakeDapperWrapper = A.Fake<IDapperWrapper>();
            _fakeSharedContentRedisInterface = A.Fake<ISharedContentRedisInterface>();
            _fakeBuilder = A.Fake<IBuilder>();
            _mapper = A.Fake<IMapper>();
            _fakeDirector = A.Fake<IDirector>();
            _fakeEventHandler = A.Fake<IEventGridHandler>();
            _configuration = A.Fake<IConfiguration>();
            _relatedContentItemIndexRepository = A.Fake<IDataService>();
            _eventGridHandler = A.Fake<IEventGridHandler>();
            _fakeCacheHandler = new CacheHandler(_fakeLogger, _mapper, _fakeDirector, _fakeBuilder, _fakeEventHandler, _configuration, _relatedContentItemIndexRepository);
        }

        #region Publish Tests       
        [Fact(Skip = "AutoMapper isn't correctly mapping the object and this causes issues further down.  Needs further investigation.  Poss. solution don't fake the Automapper for this call.")]
        public async Task EnsureOnlySpecifiedPublishContentTypesAreProcess()
        {
            //Arrange 
            var _publishContentContext = new PublishContentContext(_socCodeContentitem, _socCodeContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_oneItemPageNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessPublishedAsync(_publishContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PublishNoNodeIdsFoundInDatabase()
        {
            //Arrange 
            var nodeList = new List<NodeItem>();
            var fakeConnection = _fakeDbaAccessor.CreateConnection();

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).Returns(nodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            //var result =
            await _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, "SELECT * FROM TABLE");

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(fakeConnection, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }
        #endregion

        [Fact]
        public async Task SendEventGridMessage_Page_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.Page, false, false);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_PageUrl_DeleteCreate()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.Page, true, false);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxDelete)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxCreate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_NewPageAdded_Create()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.Page, false, true);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxCreate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxCreate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_SharedContentAdded_Create()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.SharedContent, false, true);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxCreate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxCreate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_SharedContentUpdated_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.SharedContent, false, false);
            var cacheHandler = ConfigureCacheHandler();
            var contentData = GetRelatedContentData(ContentTypes.Page);

            A.CallTo(() => _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(A<Processing>.Ignored)).Returns(contentData);

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_ProcessJobProfilesLinkingtoSectorLandingPages_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.SectorLandingPage, false, false);
            var cacheHandler = ConfigureCacheHandler();
            var contentData = GetRelatedContentData(ContentTypes.JobProfile);

            A.CallTo(() => _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(A<Processing>.Ignored)).Returns(contentData);

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_SendSkillsEventGridMessage_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.Skill, false, false);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_SendJobProfileSectorEventGridMessage_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.JobProfileSector, false, false);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEventGridMessage_SendJobProfileCategoryEventGridMessage_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.JobProfileCategory, false, false);
            var cacheHandler = ConfigureCacheHandler();
            var contentData = GetRelatedContentData(ContentTypes.JobProfile);

            A.CallTo(() => _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(A<Processing>.Ignored)).Returns(contentData);

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedTwiceOrMore();
        }

        [Fact]
        public async Task SendEventGridMessage_SendJobProfileEventGridMessage_Update()
        {
            //Arrange
            var processing = GetProcessingObj(ContentTypes.JobProfile, false, false);
            var cacheHandler = ConfigureCacheHandler();

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(ContentTypes.ApprenticeshipEntryRequirements)]
        [InlineData(ContentTypes.ApprenticeshipLink)]
        [InlineData(ContentTypes.ApprenticeshipRequirements)]
        [InlineData(ContentTypes.CollegeEntryRequirements)]
        [InlineData(ContentTypes.CollegeLink)]
        [InlineData(ContentTypes.CollegeRequirements)]
        [InlineData(ContentTypes.DigitalSkills)]
        [InlineData(ContentTypes.DynamicTitlePrefix)]
        [InlineData(ContentTypes.Environment)]
        [InlineData(ContentTypes.HiddenAlternativeTitle)]
        [InlineData(ContentTypes.JobProfileSpecialism)]
        [InlineData(ContentTypes.Location)]
        [InlineData(ContentTypes.PersonalityFilteringQuestion)]
        [InlineData(ContentTypes.PersonalityQuestionSet)]
        [InlineData(ContentTypes.PersonalityShortQuestion)]
        [InlineData(ContentTypes.PersonalityTrait)]
        [InlineData(ContentTypes.RealStory)]
        [InlineData(ContentTypes.Registration)]
        [InlineData(ContentTypes.Restriction)]
        [InlineData(ContentTypes.SOCCode)]
        [InlineData(ContentTypes.Uniform)]
        [InlineData(ContentTypes.UniversityEntryRequirements)]
        [InlineData(ContentTypes.UniversityLink)]
        [InlineData(ContentTypes.UniversityRequirements)]
        [InlineData(ContentTypes.WorkingHoursDetail)]
        [InlineData(ContentTypes.WorkingPatternDetail)]
        [InlineData(ContentTypes.WorkingPatterns)]
        public async Task SendEventGridMessage_SendRelatedJobProfileItems_Update(ContentTypes contentType)
        {
            //Arrange
            var processing = GetProcessingObj(contentType, false, false);
            var cacheHandler = ConfigureCacheHandler();
            var contentData = GetRelatedContentData(ContentTypes.JobProfile);

            A.CallTo(() => _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(A<Processing>.Ignored)).Returns(contentData);

            //Act
            await cacheHandler.ProcessEventGridMessage(processing, ContentEventType.StaxUpdate);

            //Assert
            A.CallTo(() => _eventGridHandler.SendEventMessageAsync(A<RelatedContentData>.Ignored, ContentEventType.StaxUpdate)).MustHaveHappenedOnceOrMore();
        }

        private ICacheHandler ConfigureCacheHandler()
        {
            var inMemoryConfigSettings = new Dictionary<string, string> {
                { "EventGridAllowedContentList:0", "ApprenticeshipEntryRequirements" },
                { "EventGridAllowedContentList:1", "ApprenticeshipLink" },
                { "EventGridAllowedContentList:2", "ApprenticeshipRequirements" },
                { "EventGridAllowedContentList:3", "Banner" },
                { "EventGridAllowedContentList:4", "CollegeEntryRequirements" },
                { "EventGridAllowedContentList:5", "CollegeLink" },
                { "EventGridAllowedContentList:6", "CollegeRequirements" },
                { "EventGridAllowedContentList:7", "DigitalSkills" },
                { "EventGridAllowedContentList:8", "DynamicTitlePrefix" },
                { "EventGridAllowedContentList:9", "Environment" },
                { "EventGridAllowedContentList:10", "Footer" },
                { "EventGridAllowedContentList:11", "Header" },
                { "EventGridAllowedContentList:12", "HiddenAlternativeTitle" },
                { "EventGridAllowedContentList:13", "JobProfile" },
                { "EventGridAllowedContentList:14", "JobProfileCategory" },
                { "EventGridAllowedContentList:15", "JobProfileSector" },
                { "EventGridAllowedContentList:16", "JobProfileSpecialism" },
                { "EventGridAllowedContentList:17", "Location" },
                { "EventGridAllowedContentList:18", "Page" },
                { "EventGridAllowedContentList:19", "Pagebanner" },
                { "EventGridAllowedContentList:20", "PersonalityFilteringQuestion" },
                { "EventGridAllowedContentList:21", "PersonalityQuestionSet" },
                { "EventGridAllowedContentList:22", "PersonalityShortQuestion" },
                { "EventGridAllowedContentList:23", "PersonalityTrait" },
                { "EventGridAllowedContentList:24", "RealStory" },
                { "EventGridAllowedContentList:25", "Registration" },
                { "EventGridAllowedContentList:26", "Restriction" },
                { "EventGridAllowedContentList:27", "SectorLandingPage" },
                { "EventGridAllowedContentList:28", "SharedContent" },
                { "EventGridAllowedContentList:29", "Skill" },
                { "EventGridAllowedContentList:30", "SOCCode" },
                { "EventGridAllowedContentList:31", "Taxonomy" },
                { "EventGridAllowedContentList:32", "Uniform" },
                { "EventGridAllowedContentList:33", "UniversityEntryRequirements" },
                { "EventGridAllowedContentList:34", "UniversityLink" },
                { "EventGridAllowedContentList:35", "UniversityRequirements" },
                { "EventGridAllowedContentList:36", "WorkingHoursDetail" },
                { "EventGridAllowedContentList:37", "WorkingPatternDetail" },
                { "EventGridAllowedContentList:38", "WorkingPatterns" },
                { "EventGridAllowedPagesList:0", "find-a-course" },
                { "EventGridAllowedPagesList:1", "404" }
            };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection(inMemoryConfigSettings)
                                .Build();
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            ICacheHandler eventGridSendMessageCacheHandler = new CacheHandler(_fakeLogger, _mapper, _fakeDirector, _fakeBuilder, _eventGridHandler, configuration, _relatedContentItemIndexRepository);

            return eventGridSendMessageCacheHandler;
        }

        private static Processing GetProcessingObj(ContentTypes contentType, bool previousUrl, bool created)
        {
            var processing = new Processing();

            processing.CurrentContent = "{\r\n  \"TitlePart\": {\r\n    \"Title\": \"Thank you for contacting us\"\r\n  },\r\n  \"Page\": {\r\n    \"PageLocations\": {\r\n      \"TaxonomyContentItemId\": \"4eembshqzx66drajtdten34tc8\",\r\n      \"TermContentItemIds\": [\r\n        \"4pksnz9106ngbwq74w66snan5x\"\r\n      ]\r\n    },\r\n    \"Description\": {\r\n      \"Text\": null\r\n    },\r\n    \"Herobanner\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"ShowHeroBanner\": {\r\n      \"Value\": false\r\n    },\r\n    \"ShowBreadcrumb\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolSummary\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"UseInTriageTool\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolFilters\": {\r\n      \"ContentItemIds\": []\r\n    }\r\n  },\r\n  \"SitemapPart\": {\r\n    \"Priority\": 5,\r\n    \"OverrideSitemapConfig\": false,\r\n    \"ChangeFrequency\": 0,\r\n    \"Exclude\": false\r\n  },\r\n  \"FlowPart\": {\r\n    \"Widgets\": [\r\n      {\r\n        \"ContentItemId\": \"4nkxzsea11sf1xj3hasvjfzax1\",\r\n        \"ContentItemVersionId\": null,\r\n        \"ContentType\": \"HTMLShared\",\r\n        \"DisplayText\": \"\",\r\n        \"Latest\": false,\r\n        \"Published\": false,\r\n        \"ModifiedUtc\": \"2024-09-04T11:01:17.2280141Z\",\r\n        \"PublishedUtc\": null,\r\n        \"CreatedUtc\": null,\r\n        \"Owner\": \"\",\r\n        \"Author\": \"JPrior\",\r\n        \"HTMLShared\": {\r\n          \"SharedContent\": {\r\n            \"ContentItemIds\": [\r\n              \"48n2h43p729ve15yfhhr3ccgmd\"\r\n            ]\r\n          }\r\n        },\r\n        \"GraphSyncPart\": {\r\n          \"Text\": \"<<contentapiprefix>>/htmlshared/7e8cc04c-7b6c-4ece-9be3-b95c10bebc36\"\r\n        },\r\n        \"FlowMetadata\": {\r\n          \"Alignment\": 3,\r\n          \"Size\": 100\r\n        }\r\n      }\r\n    ]\r\n  },\r\n  \"GraphSyncPart\": {\r\n    \"Text\": \"<<contentapiprefix>>/page/07664e63-deed-4d34-8f28-61c81dbd5310\"\r\n  },\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"thank-you-for-contacting-us\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"contact-us/thanks\",\r\n    \"FullUrl\": \"/find-a-course/thank-you-for-contacting-us\",\r\n    \"UseInTriageTool\": false\r\n  },\r\n  \"ContentApprovalPart\": {\r\n    \"ReviewStatus\": 0,\r\n    \"ReviewType\": 0,\r\n    \"IsForcePublished\": false\r\n  },\r\n  \"AuditTrailPart\": {\r\n    \"Comment\": \"d\",\r\n    \"ShowComment\": false\r\n  }\r\n}";
            if (previousUrl)
            {
                processing.PreviousContent = "{\r\n  \"TitlePart\": {\r\n    \"Title\": \"Thank you for contacting us\"\r\n  },\r\n  \"Page\": {\r\n    \"PageLocations\": {\r\n      \"TaxonomyContentItemId\": \"4eembshqzx66drajtdten34tc8\",\r\n      \"TermContentItemIds\": [\r\n        \"4pksnz9106ngbwq74w66snan5x\"\r\n      ]\r\n    },\r\n    \"Description\": {\r\n      \"Text\": null\r\n    },\r\n    \"Herobanner\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"ShowHeroBanner\": {\r\n      \"Value\": false\r\n    },\r\n    \"ShowBreadcrumb\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolSummary\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"UseInTriageTool\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolFilters\": {\r\n      \"ContentItemIds\": []\r\n    }\r\n  },\r\n  \"SitemapPart\": {\r\n    \"Priority\": 5,\r\n    \"OverrideSitemapConfig\": false,\r\n    \"ChangeFrequency\": 0,\r\n    \"Exclude\": false\r\n  },\r\n  \"FlowPart\": {\r\n    \"Widgets\": [\r\n      {\r\n        \"ContentItemId\": \"4nkxzsea11sf1xj3hasvjfzax1\",\r\n        \"ContentItemVersionId\": null,\r\n        \"ContentType\": \"HTMLShared\",\r\n        \"DisplayText\": \"\",\r\n        \"Latest\": false,\r\n        \"Published\": false,\r\n        \"ModifiedUtc\": \"2024-09-04T11:01:17.2280141Z\",\r\n        \"PublishedUtc\": null,\r\n        \"CreatedUtc\": null,\r\n        \"Owner\": \"\",\r\n        \"Author\": \"JPrior\",\r\n        \"HTMLShared\": {\r\n          \"SharedContent\": {\r\n            \"ContentItemIds\": [\r\n              \"48n2h43p729ve15yfhhr3ccgmd\"\r\n            ]\r\n          }\r\n        },\r\n        \"GraphSyncPart\": {\r\n          \"Text\": \"<<contentapiprefix>>/htmlshared/7e8cc04c-7b6c-4ece-9be3-b95c10bebc36\"\r\n        },\r\n        \"FlowMetadata\": {\r\n          \"Alignment\": 3,\r\n          \"Size\": 100\r\n        }\r\n      }\r\n    ]\r\n  },\r\n  \"GraphSyncPart\": {\r\n    \"Text\": \"<<contentapiprefix>>/page/07664e63-deed-4d34-8f28-61c81dbd5310\"\r\n  },\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"thank-you-for-contacting-us_FA\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"contact-us/thanks\",\r\n    \"FullUrl\": \"/find-a-course/newUrl\",\r\n    \"UseInTriageTool\": false\r\n  },\r\n  \"ContentApprovalPart\": {\r\n    \"ReviewStatus\": 0,\r\n    \"ReviewType\": 0,\r\n    \"IsForcePublished\": false\r\n  },\r\n  \"AuditTrailPart\": {\r\n    \"Comment\": \"d\",\r\n    \"ShowComment\": false\r\n  }\r\n}";
            }
            else
            {
                processing.PreviousContent = "{\r\n  \"TitlePart\": {\r\n    \"Title\": \"Thank you for contacting us\"\r\n  },\r\n  \"Page\": {\r\n    \"PageLocations\": {\r\n      \"TaxonomyContentItemId\": \"4eembshqzx66drajtdten34tc8\",\r\n      \"TermContentItemIds\": [\r\n        \"4pksnz9106ngbwq74w66snan5x\"\r\n      ]\r\n    },\r\n    \"Description\": {\r\n      \"Text\": null\r\n    },\r\n    \"Herobanner\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"ShowHeroBanner\": {\r\n      \"Value\": false\r\n    },\r\n    \"ShowBreadcrumb\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolSummary\": {\r\n      \"Html\": \"\"\r\n    },\r\n    \"UseInTriageTool\": {\r\n      \"Value\": false\r\n    },\r\n    \"TriageToolFilters\": {\r\n      \"ContentItemIds\": []\r\n    }\r\n  },\r\n  \"SitemapPart\": {\r\n    \"Priority\": 5,\r\n    \"OverrideSitemapConfig\": false,\r\n    \"ChangeFrequency\": 0,\r\n    \"Exclude\": false\r\n  },\r\n  \"FlowPart\": {\r\n    \"Widgets\": [\r\n      {\r\n        \"ContentItemId\": \"4nkxzsea11sf1xj3hasvjfzax1\",\r\n        \"ContentItemVersionId\": null,\r\n        \"ContentType\": \"HTMLShared\",\r\n        \"DisplayText\": \"\",\r\n        \"Latest\": false,\r\n        \"Published\": false,\r\n        \"ModifiedUtc\": \"2024-09-04T11:01:17.2280141Z\",\r\n        \"PublishedUtc\": null,\r\n        \"CreatedUtc\": null,\r\n        \"Owner\": \"\",\r\n        \"Author\": \"JPrior\",\r\n        \"HTMLShared\": {\r\n          \"SharedContent\": {\r\n            \"ContentItemIds\": [\r\n              \"48n2h43p729ve15yfhhr3ccgmd\"\r\n            ]\r\n          }\r\n        },\r\n        \"GraphSyncPart\": {\r\n          \"Text\": \"<<contentapiprefix>>/htmlshared/7e8cc04c-7b6c-4ece-9be3-b95c10bebc36\"\r\n        },\r\n        \"FlowMetadata\": {\r\n          \"Alignment\": 3,\r\n          \"Size\": 100\r\n        }\r\n      }\r\n    ]\r\n  },\r\n  \"GraphSyncPart\": {\r\n    \"Text\": \"<<contentapiprefix>>/page/07664e63-deed-4d34-8f28-61c81dbd5310\"\r\n  },\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"thank-you-for-contacting-us_FA\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"contact-us/thanks\",\r\n    \"FullUrl\": \"/find-a-course/thank-you-for-contacting-us\",\r\n    \"UseInTriageTool\": false\r\n  },\r\n  \"ContentApprovalPart\": {\r\n    \"ReviewStatus\": 0,\r\n    \"ReviewType\": 0,\r\n    \"IsForcePublished\": false\r\n  },\r\n  \"AuditTrailPart\": {\r\n    \"Comment\": \"d\",\r\n    \"ShowComment\": false\r\n  }\r\n}";
            }
            processing.DisplayText = "TestHeader";
            processing.Author = "UnitTest";
            processing.ContentItemId = "6482381238adsdad";
            processing.ContentType = contentType.ToString();
            processing.EventType = created ? ProcessingEvents.Created : ProcessingEvents.Published;

            return processing;
        }

        private List<RelatedContentData> GetRelatedContentData(ContentTypes contentType)
        {
            var contentDataList = new List<RelatedContentData>{
                new RelatedContentData
                {
                    Author = "Test",
                    ContentItemId = "123",
                    ContentType = contentType.ToString(),
                    DisplayText = "Test Page",
                    FullPageUrl = "somepageurl",
                    GraphSyncId = "789",
                }};

            return contentDataList;
        }

        #region Draft Tests
        [Fact(Skip = "AutoMapper isn't correctly mapping the object and this causes issues further down.  Needs further investigation.  Poss. solution don't fake the Automapper for this call.")]
        public async Task EnsureOnlySpecifiedDraftContentTypesAreProcess()
        {
            //Arrange
            var _saveDraftContentContext = new SaveDraftContentContext(_socCodeContentitem);

            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).Returns(_emptyNodeList);
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).Returns(true);

            //Act 
            await _fakeCacheHandler.ProcessDraftSavedAsync(_saveDraftContentContext);

            //Assert
            A.CallTo(() => _fakeDapperWrapper.QueryAsync<NodeItem>(A<DbConnection>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeSharedContentRedisInterface.InvalidateEntityAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        #endregion
    }
}
