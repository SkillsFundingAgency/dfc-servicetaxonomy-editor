using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Banners.Models;
using DFC.ServiceTaxonomy.Banners.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using YesSql;

namespace DFC.ServiceTaxonomy.Banners.Drivers
{
    public class BannerPartDisplayDriver : ContentPartDisplayDriver<BannerPart>
    {
        private readonly IStringLocalizer S;
        private readonly ISession _session;

        public BannerPartDisplayDriver(IStringLocalizer<BannerPartDisplayDriver> localizer, ISession session)
        {
            S = localizer;
            _session = session;
        }

        public override IDisplayResult Display(BannerPart part, BuildPartDisplayContext context)
        {
            return Initialize<BannerPartViewModel>(GetDisplayShapeType(context), model => BuildViewModel(model, part, context.TypePartDefinition))
                .Location("Detail", "Content")
                .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(BannerPart part, BuildPartEditorContext context)
        {
            return Initialize<BannerPartViewModel>(GetEditorShapeType(context), model => BuildViewModel(model, part, context.TypePartDefinition));
        }

        private void BuildViewModel(BannerPartViewModel model, BannerPart part, ContentTypePartDefinition typePartDefinition)
        {
            model.WebPageName = part.WebPageName;
            model.WebPageURL = part.WebPageURL;
            model.BannerPart = part;
            model.PartDefinition = typePartDefinition;
        }

        public override async Task<IDisplayResult> UpdateAsync(BannerPart part, IUpdateModel updater,
            UpdatePartEditorContext context)
        {
            var updated = await updater.TryUpdateModelAsync(part, Prefix, b => b.WebPageName, b => b.WebPageURL);
            if (updated)
            {
                await foreach (var item in part.ValidateAsync(S, _session))
                {
                    updater.ModelState.BindValidationResult(Prefix, item);
                }
            }

            return await EditAsync(part, context);
        }
    }
}
