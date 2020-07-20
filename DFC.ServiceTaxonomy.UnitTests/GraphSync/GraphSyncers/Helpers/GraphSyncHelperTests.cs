using System;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Helpers
{
    public class GraphSyncHelperTests
    {
        public IGraphSyncHelperCSharpScriptGlobals GraphSyncHelperCSharpScriptGlobals { get; set; }
        public IContentDefinitionManager ContentDefinitionManager { get; set; }
        // using a real one is not ideal
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        public GraphSyncHelper GraphSyncHelper { get; set; }

        private const string _contentType = "ContentType";

        public GraphSyncHelperTests()
        {
            GraphSyncHelperCSharpScriptGlobals = A.Fake<IGraphSyncHelperCSharpScriptGlobals>();

            ContentDefinitionManager = A.Fake<IContentDefinitionManager>();
            ContentTypeDefinition = new ContentTypeDefinition("name", "displayName",
                new []
                {
                    new ContentTypePartDefinition(nameof(GraphSyncPart), new ContentPartDefinition(nameof(GraphSyncPart)), null)
                },
                new JObject());
            A.CallTo(() => ContentDefinitionManager.GetTypeDefinition(_contentType)).Returns(ContentTypeDefinition);

            GraphSyncHelper = new GraphSyncHelper(GraphSyncHelperCSharpScriptGlobals, ContentDefinitionManager);
        }

        [Fact]
        public void GraphSyncHelper_ContentTypeSetToNull_ArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => GraphSyncHelper.ContentType = null);
        }

        [Fact]
        public void GraphSyncHelper_ContentTypeSet_GraphSyncPartSettingsReturned()
        {
            GraphSyncHelper.ContentType = _contentType;

            GraphSyncPartSettings graphSyncPartSettings = GraphSyncHelper.GraphSyncPartSettings;

            Assert.NotNull(graphSyncPartSettings);
        }
    }
}
