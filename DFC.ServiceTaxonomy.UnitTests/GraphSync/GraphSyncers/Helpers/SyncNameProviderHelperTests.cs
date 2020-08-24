using System;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers
{
    public class SyncNameProviderHelperTests
    {
        public ISyncNameProviderCSharpScriptGlobals SyncNameProviderCSharpScriptGlobals { get; set; }
        public IContentDefinitionManager ContentDefinitionManager { get; set; }
        public ISuperpositionContentItemVersion SuperpositionContentItemVersion { get; set; }
        // using a real one is not ideal
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        public SyncNameProvider SyncNameProvider { get; set; }

        private const string _contentType = "ContentType";

        public SyncNameProviderHelperTests()
        {
            SyncNameProviderCSharpScriptGlobals = A.Fake<ISyncNameProviderCSharpScriptGlobals>();

            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            ContentTypeDefinition = new ContentTypeDefinition("name", "displayName",
                new []
                {
                    new ContentTypePartDefinition(nameof(GraphSyncPart), new ContentPartDefinition(nameof(GraphSyncPart)), null)
                },
                new JObject());
            A.CallTo(() => ContentDefinitionManager.GetTypeDefinition(_contentType)).Returns(ContentTypeDefinition);

            SuperpositionContentItemVersion = A.Fake<ISuperpositionContentItemVersion>();

            SyncNameProvider = new SyncNameProvider(
                SyncNameProviderCSharpScriptGlobals,
                ContentDefinitionManager,
                SuperpositionContentItemVersion);
        }

        [Fact]
        public void SyncNameProvider_ContentTypeSetToNull_ArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => SyncNameProvider.ContentType = null);
        }

        [Fact]
        public void SyncNameProvider_ContentTypeSet_GraphSyncPartSettingsReturned()
        {
            SyncNameProvider.ContentType = _contentType;

            GraphSyncPartSettings graphSyncPartSettings = SyncNameProvider.GraphSyncPartSettings;

            Assert.NotNull(graphSyncPartSettings);
        }
    }
}
