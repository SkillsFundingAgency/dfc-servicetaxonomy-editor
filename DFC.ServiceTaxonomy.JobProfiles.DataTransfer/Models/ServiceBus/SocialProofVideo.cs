namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public sealed class SocialProofVideo
    {
        public SocialProofVideo(string type, string title, string summaryHtml, Thumbnail? thumbnail, string furtherInformationHtml, string url, string? linkText, string duration, string transcript)
        {
            Type = type;
            Title = title;
            SummaryHtml = summaryHtml;
            Thumbnail = thumbnail;
            FurtherInformationHtml = furtherInformationHtml;
            Url = url;
            LinkText = linkText;
            Duration = duration;
            Transcript = transcript;
        }

        /// <summary>
        /// Gets the type of the social proof video.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the title of the social proof video. This is used to render the
        /// <c>title</c> attribute of a video embed.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the summary HTML content for of the social proof video.
        /// </summary>
        /// <remarks>
        /// <para>This is raw HTML text that is input into a WYSIWYG field in the CMS.</para>
        /// </remarks>
        public string SummaryHtml { get; }

        /// <summary>
        /// Gets the thumbnail of the video.
        /// </summary>
        /// <value>
        /// A reference to the thumbnail image; otherwise, a value of <c>null</c>.
        /// </value>
        public Thumbnail? Thumbnail { get; }

        /// <summary>
        /// Gets the further information HTML content that is shown below the thumbnail.
        /// </summary>
        /// <remarks>
        /// <para>This is raw HTML text that is input into a WYSIWYG field in the CMS.</para>
        /// </remarks>
        public string FurtherInformationHtml { get; }

        /// <summary>
        /// Gets the URL of the social proof video.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the link text for showing a call to action button.
        /// </summary>
        /// <value>
        /// Plain text when call to action button is present; otherwise, a value of <c>null</c>.
        /// </value>
        public string? LinkText { get; }

        /// <summary>
        /// Gets the duration of the social proof video.
        /// </summary>
        /// <remarks>
        /// <para>This is text that is provided by the content editor; eg "One minute watch".</para>
        /// </remarks>
        public string Duration { get; }

        /// <summary>
        /// Gets the transcript text for the social proof video.
        /// </summary>
        /// <value>
        /// Plain text.
        /// </value>
        public string Transcript { get; }
    }
}
