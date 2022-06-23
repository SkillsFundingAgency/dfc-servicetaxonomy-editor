﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.AliasPartGraphSyncerTests
{
    public class AliasPartGraphSyncerAddSyncComponentsTestsBase : PartGraphSyncer_AddSyncComponentsTestsBase
    {
        public const string NodeTitlePropertyName = "alias_alias";

        public AliasPartGraphSyncerAddSyncComponentsTestsBase()
        {
            ContentPartGraphSyncer = new AliasPartGraphSyncer();
        }

        [Fact]
        public async Task AddSyncComponents_TitleInContent_TitleAddedToMergeNodeCommandsProperties()
        {
            const string alias = "alias";

            Content = JObject.Parse($"{{\"Alias\": \"{alias}\"}}");

            await CallAddSyncComponents();

            IDictionary<string, object?> expectedProperties = new Dictionary<string, object?>
                {{NodeTitlePropertyName, alias}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }

        [Fact]
        public async Task AddSyncComponents_NullTitleInContent_TitleNotAddedToMergeNodeCommandsProperties()
        {
            Content = JObject.Parse("{\"Alias\": null}");

            await CallAddSyncComponents();

            IDictionary<string, object?> expectedProperties = new Dictionary<string, object?>
                {{NodeTitlePropertyName, null}};

            Assert.Equal(expectedProperties, MergeNodeCommand.Properties);
        }
    }
}
