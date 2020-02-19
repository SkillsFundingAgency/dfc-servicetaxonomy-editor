using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;
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
            // these parameters not available during non-setup recipe by the looks of it
            // Owner = "[js: parameters('AdminUsername')]";
            // Author = "[js: parameters('AdminUsername')]";
            // these need to match what's been used in the envs
            Owner = "admin";
            Author = "admin";
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

            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public JobProfilePart EponymousPart { get; set; }
        public GraphLookupPart GraphLookupPart { get; set; }    // todo: multiple?
        public GraphSyncPart GraphSyncPart { get; set; }
        //todo: need to insert routes here
        public BagPart BagPart { get; set; }
    }

    public class JobProfilePart
    {
        public HtmlField Description { get; set; }
        public TextField JobProfileWebsiteUrl { get; set; }
        public ContentPicker SOCCode { get; set; }

        #region How To Become
        public HtmlField HtbBodies { get; set; }
        //todo: this field
        public TextField HtbTitleOptions { get; set; }
        // public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
        #endregion How To Become

        #region What It Takes
        public HtmlField WitDigitalSkillsLevel { get; set; }
        public ContentPicker WitRestrictions { get; set; }
        public ContentPicker WitOtherRequirements { get; set; }
        #endregion What It Takes

        #region What You'll Do
        public ContentPicker DayToDayTasks { get; set; }
        #endregion What You'll Do
        public TextField SalaryStarter { get; set; }
        public TextField SalaryExperienced { get; set; }
        public NumericField MinimumHours { get; set; }
        public NumericField MaximumHours { get; set; }
        public TextField WorkingHoursDetails { get; set; }
        public TextField WorkingPattern { get; set; }
        public TextField WorkingPatternDetails { get; set; }
        public HtmlField CareerPathAndProgression { get; set; }
    }

    public class TitleTextDescriptionContentItem : ContentItem
    {
        public TitleTextDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            EponymousPart = new TitleTextDescriptionPart
            {
                Description = new TextField(description)
            };

            // update DisplayText with transformed title
            //todo: transform DisplayText and use that to initialize title?
            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public TitleTextDescriptionPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class TitleTextDescriptionPart
    {
        public TextField Description { get; set; }
    }

    public class TitleHtmlDescriptionContentItem : ContentItem
    {
        public TitleHtmlDescriptionContentItem(string contentType, string title, string timestamp, string description, string contentItemId = null)
            : base(contentType, title, timestamp, contentItemId)
        {
            TitlePart = new TitlePart(title);
            GraphSyncPart = new GraphSyncPart(contentType);
            EponymousPart = new TitleHtmlDescriptionPart
            {
                Description = new HtmlField(description)
            };

            DisplayText = TitlePart.Title;
        }

        public TitlePart TitlePart { get; set; }
        public TitleHtmlDescriptionPart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class TitleHtmlDescriptionPart
    {
        public HtmlField Description { get; set; }
    }

    public class SocCodeContentItem : TitleHtmlDescriptionContentItem
    {
        public SocCodeContentItem(string title, string timestamp, string description) : base("SOCCode", title, timestamp, description)
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
}
