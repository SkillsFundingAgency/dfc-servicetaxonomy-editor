using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.Title.Models;

using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Title.Models;

using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handlers
{
    internal class SocCodeContentHandler : ContentHandlerBase
    {
        private readonly ISocMappingRepository _socCodeMappingRepository;
        private readonly ISkillsFrameworkService _skillsFrameworkService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<SocCodeContentHandler> H;
        private readonly ILogger _logger;


        public SocCodeContentHandler(
            ISocMappingRepository socCodeMappingRepository,
            ISkillsFrameworkService skillsFrameworkService,
            IServiceProvider serviceProvider,
            ISession session,
            INotifier notifier,
            IHtmlLocalizer<SocCodeContentHandler> htmlLocalizer, ILogger logger)
        {
            _socCodeMappingRepository = socCodeMappingRepository;
            _skillsFrameworkService = skillsFrameworkService;
            _serviceProvider = serviceProvider;
            _session = session;
            _notifier = notifier;
            H = htmlLocalizer;
            _logger = logger;
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.SOCCode)
            {
                var socCode = context.ContentItem.As<TitlePart>().Title;
                var onetCode = (string)context.ContentItem.Content.SOCCode.OnetOccupationCode.Text;
                _logger.LogInformation($"PublishingAsync: context {context} socCode: {socCode} onetCode:{onetCode}");
                if (string.IsNullOrEmpty(socCode) || string.IsNullOrEmpty(onetCode))
                {
                    _logger.LogInformation("Skills information not found. Please check the SOC code and ONet Occupation code or get in touch with support.");
                    context.Cancel = true;
                    await _notifier.ErrorAsync(H["Skills information not found. Please check the SOC code and ONet Occupation code or get in touch with support."]);
                    return;
                }

                // update soc/onet mapping
                if (!await _socCodeMappingRepository.UpdateMappingAsync(socCode, onetCode))
                {
                    _logger.LogInformation("Skills information not found. Please check the ONet Occupation code or get in touch with support.");
                    context.Cancel = true;
                    await _notifier.ErrorAsync(H["Skills information not found. Please check the ONet Occupation code or get in touch with support."]);
                    return;
                }

                // Create the SOC Skills Matrix Content items
                var socSkills = _skillsFrameworkService.GetRelatedSkillMapping(onetCode);
                var skillContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.Skill).ListAsync();
                var socSkillsMatrixContentItemsList =
                    await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCSkillsMatrix && c.DisplayText.StartsWith(socCode)).ListAsync();

                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                var skillCreatedCount = 0;
                var socSkillsMatrixCreatedCount = 0;
                foreach (var onetSkill in socSkills)
                {
                    // Create skill if not present
                    var skillTitle = onetSkill.Name;
                    var skill = skillContentItemsList.FirstOrDefault(sk => sk.DisplayText == skillTitle && sk.IsPublished());
                    if (skill == null)
                    {
                        skill = await contentManager.NewAsync(ContentTypes.Skill);
                        skill.ContentItem.DisplayText = skillTitle;
                        // Title
                        var titlePart = skill.As<UniqueTitlePart>();
                        titlePart.Title = skillTitle;
                        titlePart.Apply();
                        // GraphSync
                        var graphSyncPart = skill.As<GraphSyncPart>();
                        graphSyncPart.Text = GetGraphSyncId(ContentTypes.Skill);
                        graphSyncPart.Apply();
                        // Fields
                        skill.Content.Skill.Description.Text = onetSkill.Description;
                        skill.Content.Skill.ONetElementId.Text = onetSkill.Id;

                        await contentManager.CreateAsync(skill);
                        skillCreatedCount++;
                    }

                    //Create soc skills matrix if not present
                    var socSkillMatrixTitle = $"{socCode}-{onetSkill.Name}";
                    var socSkillsMatrix = socSkillsMatrixContentItemsList.FirstOrDefault(ssm => ssm.DisplayText == socSkillMatrixTitle && ssm.IsPublished());
                    if (socSkillsMatrix == null)
                    {
                        socSkillsMatrix = await contentManager.NewAsync(ContentTypes.SOCSkillsMatrix);
                        socSkillsMatrix.ContentItem.DisplayText = socSkillMatrixTitle;
                        // Title
                        var titlePart = socSkillsMatrix.As<UniqueTitlePart>();
                        titlePart.Title = socSkillMatrixTitle;
                        titlePart.Apply();
                        // GraphSync
                        var graphSyncPart = socSkillsMatrix.As<GraphSyncPart>();
                        graphSyncPart.Text = GetGraphSyncId(ContentTypes.SOCSkillsMatrix);
                        graphSyncPart.Apply();
                        // Fields
                        socSkillsMatrix.Content.SOCSkillsMatrix.ONetAttributeType.Text = onetSkill.Category.ToString();
                        socSkillsMatrix.Content.SOCSkillsMatrix.ONetRank.Text = onetSkill.Score.ToString();
                        socSkillsMatrix.Content.SOCSkillsMatrix.RelatedSkill.Text = skill.DisplayText;
                        socSkillsMatrix.Content.SOCSkillsMatrix.RelatedSOCcode.Text = socCode;

                        await contentManager.CreateAsync(socSkillsMatrix);
                        socSkillsMatrixCreatedCount++;
                    }
                }
                _logger.LogInformation($"{skillCreatedCount} Skill content item{(skillCreatedCount != 1 ? "s" : string.Empty)} and {socSkillsMatrixCreatedCount} SOC Skills Matrix content item{(socSkillsMatrixCreatedCount != 1 ? "s" : string.Empty)} have been created and published.");
                await _notifier.SuccessAsync(H[$"{skillCreatedCount} Skill content item{(skillCreatedCount != 1 ? "s" : string.Empty)} and {socSkillsMatrixCreatedCount} SOC Skills Matrix content item{(socSkillsMatrixCreatedCount != 1 ? "s" : string.Empty)} have been created and published."]);
            }
        }


        public override async Task RemovingAsync(RemoveContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.SOCCode)
            {
                var relatedJobProfile = await _session.QueryIndex<JobProfileIndex>(jp => jp.SOCCode == context.ContentItem.ContentItemId).ListAsync();
                if (relatedJobProfile != null && relatedJobProfile.Any())
                {
                    return;
                }
                var socSkillsMatrixContentItemsList =
                    await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCSkillsMatrix && c.DisplayText.StartsWith(context.ContentItem.DisplayText)).ListAsync();
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                var removeCount = 0;
                foreach (var socSkillsmatrix in socSkillsMatrixContentItemsList)
                {
                    await contentManager.RemoveAsync(socSkillsmatrix);
                    removeCount++;
                }
                await _notifier.SuccessAsync(H[$"{removeCount} SOC Skills Matrix content item{(removeCount != 1 ? "s have" : " has")} been removed."]);
            }
        }

        private string GetGraphSyncId(string contentType) => $"<<contentapiprefix>>/{contentType.ToLower()}/{Guid.NewGuid()}";
    }
}
