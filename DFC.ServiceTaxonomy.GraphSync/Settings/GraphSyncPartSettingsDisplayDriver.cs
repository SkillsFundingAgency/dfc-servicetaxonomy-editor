using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver //ContentPartDefinitionDisplayDriver
    {
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixConfiguration;

        public GraphSyncPartSettingsDisplayDriver(IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixConfiguration)
        {
            _namespacePrefixConfiguration = namespacePrefixConfiguration;
        }

        // public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        // {
        //     if (!String.Equals(nameof(GraphSyncPart), contentPartDefinition.Name))
        //     {
        //         return null;
        //     }
        //
        //     return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
        //     {
        //         var settings = contentPartDefinition.GetSettings<GraphSyncPartSettings>();
        //
        //         model.MySetting = settings.MySetting;
        //         model.GraphSyncPartSettings = settings;
        //     }).Location("Content");
        // }
        //
        // public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        // {
        //     if (!String.Equals(nameof(GraphSyncPart), contentPartDefinition.Name))
        //     {
        //         return null;
        //     }
        //
        //     var model = new GraphSyncPartSettingsViewModel();
        //
        //     if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.MySetting))
        //     {
        //         context.Builder.WithSettings(new GraphSyncPartSettings { MySetting = model.MySetting });
        //     }
        //
        //     return Edit(contentPartDefinition);
        // }

//        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition, IUpdateModel updater)
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            //todo: contentTypePartDefinition.Name?
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;

                    contentTypePartDefinition.PopulateSettings(model);

                    model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

        // public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        // {
            var model = new GraphSyncPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            // could do with an ordered set
            // namespaceprefix should never be null. throw instead?
            if (model.NamespacePrefix != null)
            {
                var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;

                // if user has entered a new prefix, update the global list of prefixes, so that they get the choice when editing graph uri id fields in new content types
                // what do we do with pollution in the list, e.g. someone adds an incorrect prefix?

                if (!currentNamespacePrefixConfiguration.NamespacePrefixOptions.Contains(model.NamespacePrefix))
                    currentNamespacePrefixConfiguration.NamespacePrefixOptions.Add(model.NamespacePrefix);
            }

            // we just (manually) populate a datalist with NamespacePrefixOptions in the view, so we don't get the list back in the model
            // we could update the model, but we want the prefixes to be global, i.e. if a new content item is created, it should contain the full list of prefixes
            // so we always populate the model from the global list, instead of storing the current set in the model
            // note: if the list in the appsettings is updated, we'll probably lose the current global list
            // we could update appsettings with new prefixes, but if we do that, we'd probably want to allow the user to delete them,
            // so perhaps we just don't update appsettings in the app service when running ;-)

            context.Builder.WithSettings(model);

            return Edit(contentTypePartDefinition);
        }
    }
}
