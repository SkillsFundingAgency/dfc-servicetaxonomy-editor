using System.Text.RegularExpressions;

namespace GetJobProfiles.Models.Recipe.Fields
{
    public class LinkField
    {
        private static Regex _sitefinityLinkRegex = new Regex(@"\[(.*)\|(.*)\]", RegexOptions.Compiled);

        public LinkField(string sitefinityString)
        {
            var match = _sitefinityLinkRegex.Match(sitefinityString);
            Url = match.Groups[2].Value.Trim();
            Text = match.Groups[1].Value.Trim();
        }

        public LinkField(string url, string text)
        {
            Url = url;
            Text = text;
        }

        public string Url { get; set; }
        public string Text { get; set; }
    }
}
