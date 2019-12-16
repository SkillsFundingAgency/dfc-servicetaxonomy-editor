using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class GraphSyncPartDisplayDriver : ContentPartDisplayDriver<GraphSyncPart>
    {
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixConfiguration;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        //todo: more appropriate to use IOptionsSnapshot?
        public GraphSyncPartDisplayDriver(
            IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixConfiguration,
            IContentDefinitionManager contentDefinitionManager)
        {
            _namespacePrefixConfiguration = namespacePrefixConfiguration;
            _contentDefinitionManager = contentDefinitionManager;
        }

        // public override IDisplayResult Display(GraphSyncPart GraphSyncPart)
        // {
        //     return Combine(
        //         Initialize<GraphSyncPartViewModel>("GraphSyncPart", m => BuildViewModel(m, GraphSyncPart))
        //             .Location("Detail", "Content:20"),
        //         Initialize<GraphSyncPartViewModel>("GraphSyncPart_Summary", m => BuildViewModel(m, GraphSyncPart))
        //             .Location("Summary", "Meta:5")
        //     );
        // }

        public override IDisplayResult Edit(GraphSyncPart graphSyncPart) //, BuildPartEditorContext context)
        {
            return Initialize<GraphSyncPartViewModel>("GraphSyncPart_Edit", m => BuildViewModel(m, graphSyncPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphSyncPart model, IUpdateModel updater)
        {
            //var settings = GetGraphSyncPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Text);

            return Edit(model);
        }

        //todo: easier to get from context?
        public GraphSyncPartSettings GetGraphSyncPartSettings(GraphSyncPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            var settings = contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();

            return settings;
        }

        private Task BuildViewModel(GraphSyncPartViewModel model, GraphSyncPart part)
        {
            var settings = GetGraphSyncPartSettings(part);

            // model.Text = part.Text;
            //
            // model.ContentItem = part.ContentItem;
            // // model.MySetting = settings.MySetting;
            // // model.Show = part.Show;
            // model.GraphSyncPart = part;
            // // model.Settings = settings;


            string namespacePrefix =
                //context.PartFieldDefinition.GetSettings<GraphUriIdFieldSettings>().NamespacePrefix ??
                settings.NamespacePrefix ??
                _namespacePrefixConfiguration.CurrentValue.NamespacePrefixOptions.FirstOrDefault();

//            model.Text = part.Text ?? $"{namespacePrefix}{context.TypePartDefinition.ContentTypeDefinition.Name.ToLowerInvariant()}/{Guid.NewGuid():D}";
            model.Text = part.Text ?? $"{namespacePrefix}{part.ContentItem.ContentType.ToLowerInvariant()}/{Guid.NewGuid():D}";
            // model.Field = field;
            // model.Part = context.ContentPart;
            // model.PartFieldDefinition = context.PartFieldDefinition;

            return Task.CompletedTask;
        }
    }
}
