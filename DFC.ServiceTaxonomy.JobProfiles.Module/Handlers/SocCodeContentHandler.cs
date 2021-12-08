using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System.Linq;
using OrchardCore.Title.Models;
using System;
using Microsoft.Extensions.DependencyInjection;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handlers
{
    internal class SocCodeContentHandler : ContentHandlerBase
    {
        private readonly ISocMappingRepository _socCodeMappingRepository;
        private readonly ISkillsFrameworkService _skillsFrameworkService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        public SocCodeContentHandler(
            ISocMappingRepository socCodeMappingRepository,
            ISkillsFrameworkService skillsFrameworkService,
            IServiceProvider serviceProvider,
            ISession session)
        {
            _socCodeMappingRepository = socCodeMappingRepository;
            _skillsFrameworkService = skillsFrameworkService;
            _serviceProvider = serviceProvider;
            _session = session;
        }

        public override async Task DraftSavingAsync(SaveDraftContentContext context)
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
