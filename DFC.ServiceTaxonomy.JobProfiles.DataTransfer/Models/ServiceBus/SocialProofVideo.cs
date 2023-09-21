namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus
{
    public sealed class SocialProofVideo
    {
        public SocialProofVideo(string title, string summary, string url, string transcript)
        {
            Title = title;
            Summary = summary;
            Url = url;
            Transcript = transcript;
        }

        /// <summary>
        /// Gets the title of the social proof video. This is used to render the
        /// <c>title</c> attribute of a video embed.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the summary text for of the social proof video.
        /// </summary>
        /// <value>
        /// Plain text.
        /// </value>
        public string Summary { get; }

        /// <summary>
        /// Gets the URL of the social proof video.
        /// </summary>
        /// <remarks>
        /// <para>At the time of writing this would be the URL of a video on YouTube.</para>
        /// </remarks>
        public string Url { get; }

        /// <summary>
        /// Gets the transcript text for the social proof video.
        /// </summary>
        /// <value>
        /// Plain text.
        /// </value>
        public string Transcript { get; }
    }
}
