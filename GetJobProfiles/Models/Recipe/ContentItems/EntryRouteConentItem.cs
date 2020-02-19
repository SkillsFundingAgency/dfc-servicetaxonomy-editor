namespace GetJobProfiles.Models.Recipe
{
    public class DirectRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public DirectRouteContentItem(string title, string timestamp, string description) : base("DirectRoute", title, timestamp, description)
        {
        }
    }

    public class OtherRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public OtherRouteContentItem(string title, string timestamp, string description) : base("OtherRoute", title, timestamp, description)
        {
        }
    }

    public class VolunteeringRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public VolunteeringRouteContentItem(string title, string timestamp, string description) : base("VolunteeringRoute", title, timestamp, description)
        {
        }
    }

    public class WorkRouteContentItem : TitleHtmlDescriptionContentItem
    {
        public WorkRouteContentItem(string title, string timestamp, string description) : base("WorkRoute", title, timestamp, description)
        {
        }
    }
}
