using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.TitlePartGraphSyncerTests
{
    public class TitlePartGraphSyncerAddSyncComponentsTestsBase : PartGraphSyncer_AddSyncComponentsTestsBase
    {
        public const string NodeTitlePropertyName = "skos__prefLabel";

        public TitlePartGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentPartGraphSyncer = new TitlePartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string title = "title";

            Content = JObject.Parse($"{{\"Title\": \"{title}\"}}");

            await CallAddSyncComponents();

            IDictionary<string,object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, title}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullTitleInContent_DisplayTextAddedToMergeNodeCommandsProperties()
        {
            const string displayText = "DisplayText";
            Content = JObject.Parse("{\"Title\": null}");
            ContentItem.DisplayText = displayText;

            await CallAddSyncComponents();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>
                {{NodeTitlePropertyName, displayText}};
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
