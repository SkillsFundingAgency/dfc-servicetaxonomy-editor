namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public sealed class RealStory
    {
        public RealStory(string title, Thumbnail? thumbnail, string summary, string bodyHtml)
        {
            Title = title;
            Thumbnail = thumbnail;
            Summary = summary;
            BodyHtml = bodyHtml;
        }

        /// <summary>
        /// Gets the title of the real story.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the thumbnail of the real story.
        /// </summary>
        /// <value>
        /// A reference to the thumbnail image; otherwise, a value of <c>null</c>.
        /// </value>
        public Thumbnail? Thumbnail { get; }

        /// <summary>
        /// Gets the summary text for of the real story.
        /// </summary>
        /// <value>
        /// Plain text.
        /// </value>
        public string Summary { get; }

        /// <summary>
        /// Gets the body HTML content of the real story.
        /// </summary>
        /// <remarks>
        /// <para>This is raw HTML text that is input into a WYSIWYG field in the CMS.</para>
        /// </remarks>
        public string BodyHtml { get; }
    }
}
