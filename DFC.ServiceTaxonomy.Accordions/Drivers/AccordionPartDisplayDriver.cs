using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.Accordions.Models;
using DFC.ServiceTaxonomy.Accordions.Settings;
using DFC.ServiceTaxonomy.Accordions.ViewModels;

namespace DFC.ServiceTaxonomy.Accordions.Drivers
{
    public class AccordionPartDisplayDriver : ContentPartDisplayDriver<AccordionPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AccordionPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(AccordionPart AccordionPart)
        {
            return Combine(
                Initialize<AccordionPartViewModel>("AccordionPart", m => BuildViewModel(m, AccordionPart))
                    .Location("Detail", "Content:20"),
                Initialize<AccordionPartViewModel>("AccordionPart_Summary", m => BuildViewModel(m, AccordionPart))
                    .Location("Summary", "Meta:5")
            );
        }
        
        public override IDisplayResult Edit(AccordionPart AccordionPart)
        {
            return Initialize<AccordionPartViewModel>("AccordionPart_Edit", m => BuildViewModel(m, AccordionPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(AccordionPart model, IUpdateModel updater)
        {
            var settings = GetAccordionPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Fields);
            
            return Edit(model);
        }

        public AccordionPartSettings GetAccordionPartSettings(AccordionPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(AccordionPart));
            var settings = contentTypePartDefinition.GetSettings<AccordionPartSettings>();

            return settings;
        }

        private Task BuildViewModel(AccordionPartViewModel model, AccordionPart part)
        {
            var settings = GetAccordionPartSettings(part);

            model.AccordionPart = part;
            model.Settings = settings;

            return Task.CompletedTask;
        }
    }
}
