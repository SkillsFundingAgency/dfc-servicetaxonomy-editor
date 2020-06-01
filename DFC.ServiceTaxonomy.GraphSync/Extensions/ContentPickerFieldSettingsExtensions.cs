using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class ContentPickerFieldSettingsExtensions
    {
        private static readonly Regex _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);

        public static async Task<string> RelationshipType(
            this ContentPickerFieldSettings contentPickerFieldSettings,
            IGraphSyncHelper graphSyncHelper)
        {
            //todo: handle multiple types
            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];

            string? relationshipType = null;
            if (contentPickerFieldSettings.Hint != null)
            {
                Match match = _relationshipTypeRegex.Match(contentPickerFieldSettings.Hint);
                if (match.Success)
                {
                    relationshipType = $"{match.Groups[1].Value}";
                }
            }

            if (relationshipType == null)
                relationshipType = await graphSyncHelper!.RelationshipTypeDefault(pickedContentType);

            return relationshipType;
        }
    }
}
