// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
// using FakeItEasy;
// using Newtonsoft.Json.Linq;
// using OrchardCore.ContentFields.Settings;
// using Xunit;
//
// namespace DFC.ServiceTaxonomy.UnitTests.GraphSync.GraphSyncers.Fields.NumericFieldGraphSyncerTests
// {
//     public class NumericFieldGraphSyncerValidateSyncComponentTestsBase : FieldGraphSyncer_ValidateSyncComponentTestsBase
//     {
//         public Dictionary<string, object> SourceNodeProperties { get; set; }
//         public NumericFieldSettings NumericFieldSettings { get; set; }
//
//         public NumericFieldGraphSyncerValidateSyncComponentTestsBase()
//         {
//             SourceNodeProperties = new Dictionary<string, object>();
//             A.CallTo(() => SourceNode.Properties).Returns(SourceNodeProperties);
//
//             NumericFieldSettings = new NumericFieldSettings();
//             A.CallTo(() => ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()).Returns(NumericFieldSettings);
//
//             ContentFieldGraphSyncer = new NumericFieldGraphSyncer();
//         }
//
//         //todo: should we be strict when types mismatch, i.e. scale has changed? probably yes
//
//         [Fact]
//         public async Task ValidateSyncComponent_Scale0PropertyCorrect_ReturnsTrue()
//         {
//             const string valueContent = "123.0";
//             const long valueProperty = 123;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 0;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.True(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_ScaleMoreThan0PropertyCorrect_ReturnsTrue()
//         {
//             const string valueContent = "123.0";
//             const double valueProperty = 123d;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 1;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.True(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_ContentNull_ReturnsFalse()
//         {
//             const long valueProperty = 123;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": null}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_PropertyMissing_ReturnsFalse()
//         {
//             const string valueContent = "123.0";
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             NumericFieldSettings.Scale = 0;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_PropertySameScaleButValueDifferent_ReturnsFalse()
//         {
//             const string valueContent = "123.0";
//             const long valueProperty = 321;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 0;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         // even though values are equivalent, types are different, so we fail validation
//         [Fact]
//         public async Task ValidateSyncComponent_PropertyDecimalValueScale0ValueEquivalent_ReturnsFalse()
//         {
//             const string valueContent = "123.0";
//             const double valueProperty = 123d;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 0;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_PropertyDecimalValueScale0PropertyValueMorePrecise_ReturnsFalse()
//         {
//             const string valueContent = "123.0";
//             const double valueProperty = 123.4d;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 0;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         [Fact]
//         public async Task ValidateSyncComponent_PropertyLongValueScaleMoreThan0ValueEquivalent_ReturnsFalse()
//         {
//             const string valueContent = "123.0";
//             const long valueProperty = 123;
//
//             ContentItemField = JObject.Parse($"{{\"Value\": {valueContent}}}");
//
//             SourceNodeProperties.Add(FieldNameTransformed, valueProperty);
//
//             NumericFieldSettings.Scale = 1;
//
//             (bool validated, _) = await CallValidateSyncComponent();
//
//             Assert.False(validated);
//         }
//
//         //todo: test that verifies that failure reason is returned
//     }
// }
