using System.Text.RegularExpressions;
using GetJobProfiles.Models.Recipe.ContentItems.Base;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class TitlePart
    {
        public TitlePart(string title) => Title = Truncate(ConvertLinks(title));

        public string Title { get; }

        // same as regex in HtmlField
        private static readonly Regex LinkRegex = new Regex(@"([^\[]*)\[([^\|]*)\s\|\s([^\]\s]*)\s*\]([^\[]*)", RegexOptions.Compiled);

        private string Truncate(string title)
        {
            // display text is stored in the oc database in a column that has a fixed size
            // even though there's no limitation on title, we want title and displaytext to be the same
            return title.Substring(0, ContentItem.MaxDisplayTextLength);
        }

        private string ConvertLinks(string sitefinityString)
        {
            const string replacement = "$1[$2]$4";
            return LinkRegex.Replace(sitefinityString, replacement);
        }
    }
}
