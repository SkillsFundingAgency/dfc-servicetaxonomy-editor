
using System.Collections.Generic;
using DFC.ServiceTaxonomy.CompUi.Model;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public partial class CompuiHandlerTests
    {
        private readonly ContentItem pageContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Page"
        };
        private readonly ContentItem _sharedContentContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "SharedContent"
        };
        private readonly ContentItem _jobProfileCategoryContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "JobProfileCategory"
        };
        private readonly ContentItem _jobProfileContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "JobProfile"
        };
        private readonly ContentItem _bannerContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Banner"
        };
        private readonly ContentItem _pagebannerContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "Pagebanner"
        };
        private readonly ContentItem _socCodeContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "SocCode"
        };

        private readonly List<NodeItem> _emptyNodeList = new List<NodeItem>();

        private readonly NodeItem _pageNode = new NodeItem()
        {
            NodeId = "<<contentapiprefix>>/page/f680eadb-be22-4f4d-bfc2-c6c82ad04981",
            Content = "{\"ContentItemId\":\"4yhev1xmryp1mvjkd9693kwqc9\",\"ContentItemVersionId\":\"4d320x99yhatnws0yxdwx05bdm\",\"ContentType\":\"Page\",\"DisplayText\":\"Send us a letter x\",\"Latest\":true,\"Published\":false,\"ModifiedUtc\":\"2024-01-22T17:47:15.0351653Z\",\"PublishedUtc\":\"2024-01-16T22:06:13.8672169Z\",\"CreatedUtc\":\"2020-07-24T10:59:54.3235574Z\",\"Owner\":\"StaxEditorAdmin\",\"Author\":\"Gavin\",\"TitlePart\":{\"Title\":\"Send us a letter x\"},\"Page\":{\"PageLocations\":{\"TaxonomyContentItemId\":\"4eembshqzx66drajtdten34tc8\",\"TermContentItemIds\":[\"4pksnz9106ngbwq74w66snan5x\"]},\"Description\":{\"Text\":\"Send us a letter page\"},\"Herobanner\":{\"Html\":\"\"},\"ShowHeroBanner\":{\"Value\":false},\"ShowBreadcrumb\":{\"Value\":false},\"TriageToolSummary\":{\"Html\":\"\"},\"UseInTriageTool\":{\"Value\":false},\"TriageToolFilters\":{\"ContentItemIds\":[]}},\"SitemapPart\":{\"Priority\":5,\"OverrideSitemapConfig\":false,\"ChangeFrequency\":0,\"Exclude\":false},\"FlowPart\":{\"Widgets\":[{\"ContentItemId\":\"4n6fn5zwwp06jsqtypnwn1x7wz\",\"ContentItemVersionId\":null,\"ContentType\":\"HTML\",\"DisplayText\":\"\",\"Latest\":false,\"Published\":false,\"ModifiedUtc\":\"2024-01-22T17:47:15.4607733Z\",\"PublishedUtc\":null,\"CreatedUtc\":null,\"Owner\":\"\",\"Author\":\"Gavin\",\"HTML\":{},\"HtmlBodyPart\":{\"Html\":\"<h1>Send us a letter</h1><p>Please post your letter to:</p><p>National Careers Service<br>PO Box 1331<br>Newcastle Upon Tyne<br>NE99 5EB</p><p>Please allow up to 14 days for a response to your letter. If you would prefer us to respond by phone or email, please include those details in your letter.</p><p>If you would like a response sooner, you can <a href=\\\"/contact-us\\\" class=\\\"govuk-link\\\">contact us online</a></p><p><a href=\\\"/contact-us\\\" class=\\\"govuk-button\\\" data-module=\\\"govuk-button\\\">Back</a></p>\"},\"GraphSyncPart\":{\"Text\":\"<<contentapiprefix>>/html/9fa92779-0a72-430d-a2a9-16d1c0b24f99\"},\"FlowMetadata\":{\"Alignment\":0,\"Size\":100}}]},\"GraphSyncPart\":{\"Text\":\"<<contentapiprefix>>/page/f680eadb-be22-4f4d-bfc2-c6c82ad04981\"},\"PageLocationPart\":{\"UrlName\":\"send-us-a-letter\",\"DefaultPageForLocation\":false,\"RedirectLocations\":\"/contact-us/send\\r\\n/contact-us/letter\",\"FullUrl\":\"/contact-us/send-us-a-letter\"},\"UnpublishLaterPart\":{\"ScheduledUnpublishUtc\":null},\"ContentApprovalPart\":{\"ReviewStatus\":0,\"ReviewType\":0,\"IsForcePublished\":true},\"AuditTrailPart\":{\"Comment\":null,\"ShowComment\":false}}"
        };

        private readonly List<NodeItem> _oneItemPageNodeList = new List<NodeItem>
            {
                new NodeItem()
                {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"}
            };

        private readonly List<NodeItem> _twoItemPageNodeList = new List<NodeItem>
            {
                new NodeItem() {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"},
                new NodeItem() {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"}
            };
    }
}
