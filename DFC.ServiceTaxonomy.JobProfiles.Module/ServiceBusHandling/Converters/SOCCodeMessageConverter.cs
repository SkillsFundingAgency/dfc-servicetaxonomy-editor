using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class SocCodeMessageConverter : IMessageConverter<SocCodeItem>
    {
        private readonly IServiceProvider _serviceProvider;

        public SocCodeMessageConverter(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public async Task<SocCodeItem> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> socCodes = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.SOCCode, contentManager);
            if (!socCodes.Any() || socCodes.Count() > 1)
            {
                return new SocCodeItem();
            }

            var socCode = socCodes.First();
            IEnumerable<ContentItem> apprenticeshipStandards = await Helper.GetContentItemsAsync(socCode.Content.SOCCode.ApprenticeshipStandards, contentManager);

            var socCodeItem = new SocCodeItem
            {
                Id = socCode.As<GraphSyncPart>().ExtractGuid(),
                SOCCode = socCode.As<TitlePart>().Title,
                Description = socCode.Content.SOCCode.Description is null ? default : (string?)socCode.Content.SOCCode.Description.Html,
                ONetOccupationalCode = socCode.Content.SOCCode.OnetOccupationCode is null ? default : (string?)socCode.Content.SOCCode.OnetOccupationCode.Text,
                SOC2020 = socCode.Content.SOCCode.SOC2020 is null ? default : (string?)socCode.Content.SOCCode.SOC2020.Text,
                SOC2020extension = socCode.Content.SOCCode.SOC2020extension is null ? default : (string?)socCode.Content.SOCCode.SOC2020extension.Text,
                ApprenticeshipStandards = Helper.MapClassificationData(apprenticeshipStandards)
            };
            return socCodeItem;
        }
    }
}
