using System.Text.RegularExpressions;

namespace GetJobProfiles.Models.Recipe.Fields
{
    public class LinkField
    {
        private static Regex _sitefinityLinkRegex = new Regex(@"\[(.*)\|(.*)\]", RegexOptions.Compiled);

        public LinkField(string sitefinityString)
        {
            var match = _sitefinityLinkRegex.Match(sitefinityString);
            Url = match.Captures[1].Value;
            Text = match.Captures[2].Value;
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
