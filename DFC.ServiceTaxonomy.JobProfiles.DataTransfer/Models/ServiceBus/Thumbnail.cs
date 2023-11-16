namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public sealed class Thumbnail
    {
        public Thumbnail(string url, string text)
        {
            Url = url;
            Text = text;
        }

        public string Url { get; }

        public string Text { get; }
    }
}
