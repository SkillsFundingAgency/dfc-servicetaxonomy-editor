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

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handler
{
    internal class JobProfileContentHandler : ContentHandlerBase
    {
        private readonly ISocMappingRepository _socCodeMappingRepository;
        private readonly ISkillsFrameworkService _skillsFrameworkService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        public JobProfileContentHandler(
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

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
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

                    // Create the SOC Skills Matrix Content items
                    var socSkills = _skillsFrameworkService.GetRelatedSkillMapping(socCodeMapping.ONetOccupationalCode);
                    var skillContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == "Skill").ListAsync();
                    var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                    foreach (var onetSkill in socSkills)
                    {
                        var title = $"{onetSkill.Id}-{onetSkill.Name}";
                        if (!skillContentItemsList.Any(sk => sk.DisplayText == title))
                        {
                            // Create skill
                            var skill = await contentManager.NewAsync("Skill");
                            var titlePart = skill.As<TitlePart>();
                            titlePart.Title = title;
                            skill.ContentItem.DisplayText = title;
                            titlePart.Apply();

                            var graphSyncPart = skill.As<GraphSyncPart>();
                            graphSyncPart.Text = Guid.NewGuid().ToString();
                            graphSyncPart.Apply();

                            skill.Content.Skill.Description.Text = onetSkill.Name;

                            await contentManager.CreateAsync(skill);
                        }
                        // Create skill and soc skill matrix
                    }
                }
            }
        }
    }
}
