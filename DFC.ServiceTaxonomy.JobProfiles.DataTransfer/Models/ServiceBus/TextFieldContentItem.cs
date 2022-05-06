namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public class TextFieldContentItem : RelatedContentItem
    {
        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
