using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataAccess.Repositories;
using DFC.ServiceTaxonomy.Extensions;
using DFC.ServiceTaxonomy.Title.Indexes;
using DFC.ServiceTaxonomy.Title.Models;
using DFC.ServiceTaxonomy.Title.Settings;
using DFC.ServiceTaxonomy.Title.ViewModels;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Title.Drivers
{
    public class UniqueTitlePartDisplayDriver : ContentPartDisplayDriver<UniqueTitlePart>
    {
        private readonly IStringLocalizer S;
        private readonly IGenericIndexRepository<UniqueTitlePartIndex> _uniqueTitleIndexRepository;
        private readonly ILogger<UniqueTitlePartDisplayDriver> _logger;

        public UniqueTitlePartDisplayDriver(IStringLocalizer<UniqueTitlePartDisplayDriver> localizer, IGenericIndexRepository<UniqueTitlePartIndex> uniqueTitleIndexRepository, ILogger<UniqueTitlePartDisplayDriver> logger)
        {
            S = localizer;
            _uniqueTitleIndexRepository = uniqueTitleIndexRepository;
            _logger = logger;
        }

        public override IDisplayResult Display(UniqueTitlePart part, BuildPartDisplayContext context)
        {
            return Initialize<UniqueTitlePartViewModel>(GetDisplayShapeType(context), model => BuildViewModel(model, part, context.TypePartDefinition))
                .Location("Detail", "Content")
                .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(UniqueTitlePart part, BuildPartEditorContext context)
        {
            return Initialize<UniqueTitlePartViewModel>(GetEditorShapeType(context), model => BuildViewModel(model, part, context.TypePartDefinition));
        }

        private void BuildViewModel(UniqueTitlePartViewModel model, UniqueTitlePart part, ContentTypePartDefinition typePartDefinition)
        {
            var uniqueTitlePartSettings = typePartDefinition.GetSettings<UniqueTitlePartSettings>();

            model.Title = part.Title;
            model.UniqueTitlePart = part;
            model.PartDefinition = typePartDefinition;
            model.Hint = uniqueTitlePartSettings == null ? string.Empty : uniqueTitlePartSettings.Hint;
            model.Placeholder = uniqueTitlePartSettings == null ? string.Empty : uniqueTitlePartSettings.Placeholder;
            model.ReadOnlyOnPublish = uniqueTitlePartSettings is { ReadOnlyOnPublish: true };
        }

        public override async Task<IDisplayResult> UpdateAsync(UniqueTitlePart part, IUpdateModel updater,
            UpdatePartEditorContext context)
        {
            _logger.LogInformation("UpdateAsync: UniqueTitlePart {Part}", part);
            var updated = await updater.TryUpdateModelAsync(part, Prefix, b => b.Title);
            if (updated)
            {
                await foreach (var item in part.ValidateAsync(S, _uniqueTitleIndexRepository))
                {
                    updater.ModelState.BindValidationResult(Prefix, item);
                }
            }

            return await EditAsync(part, context);
        }
    }
}
