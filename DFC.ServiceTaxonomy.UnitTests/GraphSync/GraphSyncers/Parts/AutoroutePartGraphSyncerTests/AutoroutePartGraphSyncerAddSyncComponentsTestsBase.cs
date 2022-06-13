﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.AutoroutePartGraphSyncerTests
{
    public class AutoroutePartGraphSyncerAddSyncComponentsTestsBase : PartGraphSyncer_AddSyncComponentsTestsBase
    {
        public const string NodeTitlePropertyName = "autoroute_path";

        public AutoroutePartGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentPartGraphSyncer = new AutoroutePartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string path = "path";

            Content = JObject.Parse($"{{\"Path\": \"{path}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object?> expectedProperties = new Dictionary<string, object?>
                {{NodeTitlePropertyName, path}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            Content = JObject.Parse("{\"Path\": null}");

            await CallAddSyncComponents();


            IDictionary<string, object?> expectedProperties = new Dictionary<string, object?>
                {{NodeTitlePropertyName, null}};
            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
