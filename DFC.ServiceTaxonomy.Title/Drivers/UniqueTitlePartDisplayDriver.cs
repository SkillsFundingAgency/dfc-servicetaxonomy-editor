﻿using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Title.Models;
using DFC.ServiceTaxonomy.Title.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using DFC.ServiceTaxonomy.Extensions;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using YesSql;
using DFC.ServiceTaxonomy.Title.Settings;
using DFC.ServiceTaxonomy.DataAccess.Repositories;
using DFC.ServiceTaxonomy.Title.Indexes;

namespace DFC.ServiceTaxonomy.Title.Drivers
{
    public class UniqueTitlePartDisplayDriver : ContentPartDisplayDriver<UniqueTitlePart>
    {
        private readonly IStringLocalizer S;
        private readonly IGenericIndexRepository<UniqueTitlePartIndex> _uniqueTitleIndexRepository;

        public UniqueTitlePartDisplayDriver(IStringLocalizer<UniqueTitlePartDisplayDriver> localizer, IGenericIndexRepository<UniqueTitlePartIndex> uniqueTitleIndexRepository)
        {
            S = localizer;
            _uniqueTitleIndexRepository = uniqueTitleIndexRepository;
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
            model.ReadOnlyOnPublish = uniqueTitlePartSettings == null ? false : uniqueTitlePartSettings.ReadOnlyOnPublish;
        }

        public override async Task<IDisplayResult> UpdateAsync(UniqueTitlePart part, IUpdateModel updater,
            UpdatePartEditorContext context)
        {
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
