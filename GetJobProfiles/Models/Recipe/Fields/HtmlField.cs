using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GetJobProfiles.Models.Recipe.Fields
{
    public class HtmlField
    {
        public HtmlField() => Html = null;
        public HtmlField(string html) => Html = ConvertLists(WrapInParagraph(ConvertLinks(html)));
        //todo: correct array to <p>??
        public HtmlField(IEnumerable<string> html) => Html = html.Aggregate(string.Empty, (h, p) =>
            ConvertLinks($"{h}{ConvertLists(WrapInParagraph(ConvertLinks(p)))}"));

        public string Html { get; set; }
        private static readonly Regex LinkRegex = new Regex(@"([^\[]*)\[([^\|]*)\s\|\s([^\]\s]*)\s*\]([^\[]*)", RegexOptions.Compiled);
        private static readonly Regex ListRegex = new Regex(@"(?<!https:)(?<!http:)(?<=:)(?!.*:).*;.*?(?=</p>)", RegexOptions.Compiled);

        private static string WrapInParagraph(string source)
        {
            return $"<p>{source}</p>";
        }

        public static string ConvertLinks(string sitefinityString)
        {
            const string replacement = "$1<a href=\"$3\">$2</a>$4";
            return LinkRegex.Replace(sitefinityString, replacement);
        }

        public static string ConvertLists(string source)
        {
            var match = ListRegex.Match(source);

            if (match.Success)
            {
                var listItems = match.Value.Split(";").Select(x => x.Trim().TrimEnd('.')).ToList();
                string replacement = $"<ul><li>{string.Join("</li><li>", listItems)}</li></ul>";

                return source.Replace(match.Value, replacement);
            }

            return source;
        }
    }
}
