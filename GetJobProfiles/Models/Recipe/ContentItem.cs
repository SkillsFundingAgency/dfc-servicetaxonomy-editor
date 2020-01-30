using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OrchardCore.Entities;

namespace GetJobProfiles.Models.Recipe
{
    public class ContentItem
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        public ContentItem(string contentType, string title, string timestamp, string contentItemId = null)
        {
            ContentItemId = contentItemId ?? _generator.GenerateUniqueId(); //"[js:uuid()]";
            ContentItemVersionId = _generator.GenerateUniqueId(); //"[js:uuid()]";
            ContentType = contentType;
            DisplayText = title;
            Latest = true;
            Published = true;
            ModifiedUtc = timestamp;
            PublishedUtc = timestamp;
            CreatedUtc = timestamp;
            Owner = "[js: parameters('AdminUsername')]";
            Author = "[js: parameters('AdminUsername')]";
        }

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
        public JobProfileContentItem(string title, string timestamp)
            : base("JobProfile", title, timestamp)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart("JobProfile");
        }

        public TitlePart TitlePart { get; set; }
        public HtmlField Description { get; set; }
        public TextField JobProfileWebsiteUrl { get; set; }
        public HtmlField HtbBodies { get; set; }
        public TextField HtbTitleOptions { get; set; }
        public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
        public HtmlField WitDigitalSkillsLevel { get; set; }
        public ContentPicker WitRestrictions { get; set; }
        public ContentPicker WitOtherRequirements { get; set; }
        public ContentPicker SOCCode { get; set; }
        public GraphLookupPart GraphLookupPart { get; set; }    // todo: multiple?
        public GraphSyncPart GraphSyncPart { get; set; }
        public BagPart BagPart { get; set; }
        public ContentPicker DayToDayTasks { get; set; }
    }

    public class TitleTextDescriptionContentItem : ContentItem
    {
        public TitleTextDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            Description = new TextField(description);
        }

        public TitlePart TitlePart { get; set; }
        public TextField Description { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class TitleHtmlDescriptionContentItem : ContentItem
    {
        public TitleHtmlDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            Description = new HtmlField(description);
        }

        public TitlePart TitlePart { get; set; }
        public HtmlField Description { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class SocCodeContentItem : TitleTextDescriptionContentItem
    {
        public SocCodeContentItem(string title, string timestamp, string description) : base("SocCode", title, timestamp, description)
        {
        }
    }

    public class RegistrationContentItem : TitleHtmlDescriptionContentItem
    {
        public RegistrationContentItem(string title, string timestamp, string description, string contentItemId) : base("Registration", title, timestamp, description, contentItemId)
        {
        }
    }

    public class RestrictionContentItem : TitleHtmlDescriptionContentItem
    {
        public RestrictionContentItem(string title, string timestamp, string description, string contentItemId) : base("Restriction", title, timestamp, description, contentItemId)
        {
        }
    }

    public class DayToDayTaskContentItem : TitleHtmlDescriptionContentItem
    {
        public DayToDayTaskContentItem(string title, string timestamp, string description, string contentItemId) : base("DayToDayTask", title, timestamp, description, contentItemId)
        {
        }
    }

    public class OtherRequirementContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRequirementContentItem(string title, string timestamp, string description, string contentItemId) : base("OtherRequirement", title, timestamp, description, contentItemId)
        {
        }
    }

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

    public class HtmlField
    {
        public HtmlField(string html) => Html = WrapInParagraph(ConvertLinks(html));
        //todo: correct array to <p>??
        public HtmlField(IEnumerable<string> html) => Html = html.Aggregate(string.Empty, (h, p) =>
            ConvertLinks($"{h}{WrapInParagraph(ConvertLinks(p))}"));

        public string Html { get; set; }
        private static readonly Regex LinkRegex = new Regex(@"([^\[]*)\[([^\|]*)\s\|\s([^\]\s]*)\s*\]([^\[]*)", RegexOptions.Compiled);

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
        public TextField(string text) => Text = text;

        public string Text { get; set; }
    }

    public class ContentPicker
    {
        public ContentPicker()
        {}

        public ContentPicker(ConcurrentDictionary<string, (string id, string text)> currentContentItems, IEnumerable<string> contentItems)
        {
            ContentItemIds = contentItems?.Select(ci => currentContentItems[ci].id) ?? new string[0];
        }

        public IEnumerable<string> ContentItemIds { get; set; }
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
        public GraphSyncPart(string contentType) => Text = $"http://nationalcareers.service.gov.uk/{contentType.ToLowerInvariant()}/{Guid.NewGuid()}";

        public string Text { get; set; }
    }

    public class BagPart    //todo: multiples??
    {
        public ContentItem[] ContentItems { get; set; }
    }
}
