using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using OrchardCore;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters
{
    public class RealStoryMessageConverter : IMessageConverter<RealStory>
    {
        private readonly IOrchardHelper _orchardHelper;

        public RealStoryMessageConverter(IOrchardHelper orchardHelper)
        {
            _orchardHelper = orchardHelper;
        }

        public Task<RealStory> ConvertFromAsync(ContentItem contentItem)
        {
            Thumbnail? thumbnail = null;
            
            if (contentItem.Content.RealStory.Thumbnail.Paths is not null && contentItem.Content.RealStory.Thumbnail.Paths.Count > 0)
            {
                string assetPath = contentItem.Content.RealStory.Thumbnail.Paths[0];
                thumbnail = new Thumbnail(
                    url: _orchardHelper.AssetUrl(assetPath, width: 600),
                    text: (string)contentItem.Content.RealStory.Thumbnail.MediaTexts[0]
                );
            }

            var realStory = new RealStory(
                title: contentItem.DisplayText,
                summary: (string)contentItem.Content.RealStory.Summary.Text,
                thumbnail: thumbnail,
                furtherInformationHtml: (string)contentItem.Content.RealStory.FurtherInformation.Html,
                bodyHtml: (string)contentItem.Content.RealStory.Body.Html
            );

            return Task.FromResult(realStory);
        }
    }
}
