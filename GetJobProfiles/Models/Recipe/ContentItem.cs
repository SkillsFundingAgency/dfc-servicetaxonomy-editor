using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GetJobProfiles.Models.Recipe
{
    public class ContentItem
    {
        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }
        public string DisplayText { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
        public string ModifiedUtc { get; set; }
        public string PublishedUtc { get; set; }
        public string CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
    }

    public class JobProfileContentItem : ContentItem
    {
        public TitlePart TitlePart { get; set; }
        public HtmlField HtbBodies { get; set; }
        public TextField HtbTitleOptions { get; set; }
        public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRestrictions { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
        public GraphLookupPart GraphLookupPart { get; set; }    // todo: multiple?
        public GraphSyncPart GraphSyncPart { get; set; }    // todo: multiple?
        public BagPart BagPart { get; set; }
    }

    public class TitlePart
    {
        public string Title { get; set; }
    }

    public class HtmlField
    {
        public HtmlField(string html) => Html = WrapInParagraph(ConvertLinks(html));
        //todo: correct array to <p>??
        //todo: how does sitefinity store ul etc?
        public HtmlField(IEnumerable<string> html) => Html = html.Aggregate(string.Empty, (h, p) =>
            ConvertLinks($"{h}{WrapInParagraph(ConvertLinks(p))}"));

        public string Html { get; set; }

        private static readonly Regex LinkRegex = new Regex(@"([^\]]*)\[([^\|]*)\s\|\s([^\]]*)\]([^\[]*)", RegexOptions.Compiled);

        private static string WrapInParagraph(string source)
        {
            return $"<p>{source}</p>";
        }

        private string ConvertLinks(string sitefinityString)
        {
            const string replacement = "$1<a href=\"$3\">$2</a>$4";
            return LinkRegex.Replace(sitefinityString, replacement);
        }
    }

    public class TextField
    {
        public string Text { get; set; }
    }

    public class ContentPicker
    {
        public ContentPicker(Dictionary<string, (string id, string text)> currentContentItems, IEnumerable<string> contentItems)
        {
            ContentItemIds = contentItems.Select(ci => currentContentItems[ci].id);
        }

        public IEnumerable<string> ContentItemIds { get; }
    }

    public class GraphLookupPart
    {
        public Node[] Nodes { get; set; }
    }

    public class Node
    {
        public string Id { get; set; }
        public string DisplayText { get; set; }
    }

    public class GraphSyncPart
    {
        public string Text { get; set; }
    }

    public class BagPart    //todo: multiples??
    {
        public ContentItem[] ContentItems { get; set; }
    }
}
