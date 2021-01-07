using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OrchardCore.Sitemaps.Models;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Parts.SitemapPartGraphSyncerTests
{
    public class SitemapPartGraphSyncerValidateSyncComponentTestsBase : PartGraphSyncer_ValidateSyncComponentTestsBase
    {
        public SitemapPartGraphSyncerValidateSyncComponentTestsBase()
        {
            A.CallTo(() => SyncNameProvider.PropertyName("OverrideSitemapConfig")).Returns("sitemap_OverrideSitemapConfig");
            A.CallTo(() => SyncNameProvider.PropertyName("ChangeFrequency")).Returns("sitemap_ChangeFrequency");
            A.CallTo(() => SyncNameProvider.PropertyName("Priority")).Returns("sitemap_Priority");
            A.CallTo(() => SyncNameProvider.PropertyName("Exclude")).Returns("sitemap_Exclude");

            ContentPartGraphSyncer = new SitemapPartGraphSyncer();
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(false, true, true, true, false)]
        [InlineData(false, true, true, false, true)]
        [InlineData(false, true, false, true, true)]
        [InlineData(false, false, true, true, true)]
        [InlineData(false, false, false, false, false)]
        public async Task ValidateSyncComponentTests(bool expected, bool overrideSitemapConfigMatches, bool changeFrequencyMatches, bool priorityMatches, bool exludeMatches)
        {
            A.CallTo(() => GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                "OverrideSitemapConfig",
                A<JObject>._,
                "sitemap_OverrideSitemapConfig",
                SourceNode)).Returns((overrideSitemapConfigMatches, ""));

            A.CallTo(() => GraphValidationHelper.EnumContentPropertyMatchesNodeProperty<ChangeFrequency>(
                "ChangeFrequency",
                A<JObject>._,
                "sitemap_ChangeFrequency",
                SourceNode)).Returns((changeFrequencyMatches, ""));

            A.CallTo(() => GraphValidationHelper.LongContentPropertyMatchesNodeProperty(
                "Priority",
                A<JObject>._,
                "sitemap_Priority",
                SourceNode)).Returns((priorityMatches, ""));

            A.CallTo(() => GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                "Exclude",
                A<JObject>._,
                "sitemap_Exclude",
                SourceNode)).Returns((exludeMatches, ""));

            (bool validated, _) = await CallValidateSyncComponent();

            Assert.Equal(expected, validated);
        }

        //todo: test that verifies that failure reason is returned
        //todo: test to check nothing added to ExpectedRelationshipCounts
    }
}
