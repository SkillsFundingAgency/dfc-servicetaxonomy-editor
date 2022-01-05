using System;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

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


        public SocCodeContentHandler(
            ISocMappingRepository socCodeMappingRepository,
            ISkillsFrameworkService skillsFrameworkService,
            IServiceProvider serviceProvider,
            ISession session,
            INotifier notifier,
            IHtmlLocalizer<SocCodeContentHandler> htmlLocalizer)
        {
            _socCodeMappingRepository = socCodeMappingRepository;
            _skillsFrameworkService = skillsFrameworkService;
            _serviceProvider = serviceProvider;
            _session = session;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.SOCcode)
            {
                var socCode = context.ContentItem.As<TitlePart>().Title;
                var onetCode = (string)context.ContentItem.Content.SOCcode.OnetOccupationCode.Text;

                if (string.IsNullOrEmpty(socCode) || string.IsNullOrEmpty(onetCode))
                {
                    context.Cancel = true;
                    _notifier.Error(H["Skills information not found. Please check the SOC code and ONet Occupation code or get in touch with support."]);
                    return;
                }

                // update soc/onet mapping
                if (!await _socCodeMappingRepository.UpdateMappingAsync(socCode, onetCode))
                {
                    context.Cancel = true;
                    _notifier.Error(H["Skills information not found. Please check the ONet Occupation code or get in touch with support."]);
                    //return;
                }


                // get skills

                // create skills if not present

                // create soc skills matrix
            }
        }

        public async Task DraftySavingAsync(SaveDraftContentContext context)
        {
            if (context.ContentItem.ContentType == "SOCCode")
            {
                var socCode = (string)context.ContentItem.Content.SOCCode.SOCCodeTextField.Text;
                if(!string.IsNullOrEmpty(socCode))
                {
                    // Need to do our stuff

                    // Get the ONET code
                    var socCodeMapping = _socCodeMappingRepository.GetById(socCode);
                    context.ContentItem.Content.SOCCode.ONetOccupationCodeTextField.Text = socCodeMapping.ONetOccupationalCode;
                    context.ContentItem.Content.SOCCode.SOCDescriptionTextField.Text = socCodeMapping.Description;
                    // Create the SOC Skills Matrix Content items
                    var socSkills = _skillsFrameworkService.GetRelatedSkillMapping(socCodeMapping.ONetOccupationalCode);
                    var skillContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == "Skill").ListAsync();
                    var socSkillsMatrixContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == "SOCSkillsMatrix").ListAsync();
                    var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                    foreach (var onetSkill in socSkills)
                    {
                        // Create skill if not present
                        var skillTitle = onetSkill.Name;
                        var skill = skillContentItemsList.FirstOrDefault(sk => sk.DisplayText == skillTitle);
                        if (skill == null)
                        {
                            skill = await contentManager.NewAsync("Skill");
                            skill.ContentItem.DisplayText = skillTitle;

                            var titlePart = skill.As<TitlePart>();
                            titlePart.Title = skillTitle;
                            titlePart.Apply();

                            var graphSyncPart = skill.As<GraphSyncPart>();
                            graphSyncPart.Text = $"<<contentapiprefix>>/skill/{Guid.NewGuid()}";
                            graphSyncPart.Apply();

                            skill.Content.Skill.Description.Text = onetSkill.Name;
                            skill.Content.Skill.OnetSkillID.Text = onetSkill.Id;

                            await contentManager.CreateAsync(skill);
                        }

                        //Create soc skills matrix if not present
                        var socSkillMatrixTitle = $"{socCode}-{onetSkill.Name}";
                        var socSkillsMatrix = socSkillsMatrixContentItemsList.FirstOrDefault(ssm => ssm.DisplayText == socSkillMatrixTitle);
                        if (socSkillsMatrix == null)
                        {
                            socSkillsMatrix = await contentManager.NewAsync("SOCSkillsMatrix");
                            socSkillsMatrix.ContentItem.DisplayText = socSkillMatrixTitle;

                            var titlePart = socSkillsMatrix.As<TitlePart>();
                            titlePart.Title = socSkillMatrixTitle;
                            titlePart.Apply();

                            var graphSyncPart = socSkillsMatrix.As<GraphSyncPart>();
                            graphSyncPart.Text = $"<<contentapiprefix>>/socskillsmatrix/{Guid.NewGuid()}";
                            graphSyncPart.Apply();

                            socSkillsMatrix.Content.SOCSkillsMatrix.LinkedSkill.ContentItemIds = new JArray(skill.ContentItemId);

                            await contentManager.CreateAsync(socSkillsMatrix);
                        }
                    }
                }
            }
        }
    }
}
