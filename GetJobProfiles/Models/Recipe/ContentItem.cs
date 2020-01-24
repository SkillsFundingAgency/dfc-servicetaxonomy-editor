using System.Runtime.InteropServices;

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
        public string Html { get; set; }
    }

    public class TextField
    {
        public string Text { get; set; }
    }

    public class ContentPicker
    {
        public string ContentItemIds { get; set; }
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
