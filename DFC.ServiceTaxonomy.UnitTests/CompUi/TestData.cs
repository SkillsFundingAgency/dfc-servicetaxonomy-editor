using System.Collections.Generic;
using DFC.ServiceTaxonomy.CompUi.Model;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnitTests.CompUi
{
    public partial class CompuiHandlerTests
    {
        private ContentItem _socCodeContentitem = new ContentItem
        {
            ContentItemId = "48n2h43p729ve15yfhhr3ccgmd",
            ContentType = "SocCode",
            Latest = true,
            Published = true,
            Id = 1,
        };

        private readonly List<NodeItem> _emptyNodeList = new List<NodeItem>();

        private readonly List<NodeItem> _oneItemPageNodeList = new List<NodeItem>
            {
                new NodeItem()
                {
                    NodeId = "<<contentapiprefix>>/sharedcontent/bfee0a93-cd0d-40eb-a72d-7bb0c0cced3e",
                    Content = "{\r\n  \"ContentItemId\": \"4yhev1xmryp1mvjkd9693kwqc9\",\r\n  \"ContentItemVersionId\": \"4d320x99yhatnws0yxdwx05bdm\",\r\n  \"ContentType\": \"Page\",\r\n  \"PageLocationPart\": {\r\n    \"UrlName\": \"send-us-a-letter\",\r\n    \"DefaultPageForLocation\": false,\r\n    \"RedirectLocations\": \"/contact-us/send\\r\\n/contact-us/letter\",\r\n    \"FullUrl\": \"/contact-us/send-us-a-letter\"\r\n  },\r\n}"}
            };
    }
}
