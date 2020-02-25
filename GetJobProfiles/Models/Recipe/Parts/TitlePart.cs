using System.Text.RegularExpressions;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class TitlePart
    {
        public TitlePart(string title) => Title = ConvertLinks(title);

        public string Title { get; set; }

        // same as regex in HtmlField
        private static readonly Regex LinkRegex = new Regex(@"([^\[]*)\[([^\|]*)\s\|\s([^\]\s]*)\s*\]([^\[]*)", RegexOptions.Compiled);

        private string ConvertLinks(string sitefinityString)
        {
            const string replacement = "$1[$2]$4";
            return LinkRegex.Replace(sitefinityString, replacement);
        }
    }
}
