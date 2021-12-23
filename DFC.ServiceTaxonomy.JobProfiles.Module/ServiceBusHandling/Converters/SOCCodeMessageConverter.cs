using System;
using System.Collections.Generic;
using System.Linq;
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

        public SocCodeMessageConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public SocCodeItem ConvertFrom(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            List<ContentItem> socCodes = Helper.GetContentItems(contentItem.Content.JobProfile.SOCcode, contentManager);
            if(!socCodes.Any() || socCodes.Count > 1)
            {
                return new SocCodeItem();
            }
            var socCode = socCodes.First();
            List<ContentItem> apprenticeshipStandards = Helper.GetContentItems(socCode.Content.SOCcode.ApprenticeshipStandards, contentManager);

            var socCodeItem = new SocCodeItem
            {
                Id = socCode.As<GraphSyncPart>().ExtractGuid(),
                SOCCode = socCode.As<TitlePart>().Title,
                Description = socCode.Content.SOCcode.Description == null ? default(string?) : (string?)socCode.Content.SOCcode.Description.Html,
                ONetOccupationalCode = socCode.Content.SOCcode.OnetOccupationCode == null ? default(string?) : (string?)socCode.Content.SOCcode.OnetOccupationCode.Text,
                SOC2020 = socCode.Content.SOCcode.SOC2020 == null ? default(string?) : (string?)socCode.Content.SOCcode.SOC2020.Text,
                SOC2020extension = socCode.Content.SOCcode.SOC2020extension == null ? default(string?) : (string?)socCode.Content.SOCcode.SOC2020extension.Text,
                ApprenticeshipStandards = Helper.MapClassificationData(apprenticeshipStandards)
            };
            return socCodeItem;
        }
    }
}
