using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IOptionsSnapshot<NamespacePrefixConfiguration> _namespacePrefixOptions;

        public GraphSyncPartSettingsDisplayDriver(IOptionsSnapshot<NamespacePrefixConfiguration> namespacePrefixOptions)
        {
            _namespacePrefixOptions = namespacePrefixOptions;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            //todo: contentTypePartDefinition.Name?
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixOptions.Value;

                    contentTypePartDefinition.PopulateSettings(model);

                    GraphSyncPartSettings graphSyncPartSettings = contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();

                    model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                    model.NamespacePrefix = graphSyncPartSettings.NamespacePrefix;
                })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            var model = new GraphSyncPartSettingsViewModel();

            //todo: if prefix added, doesn't add to global choice
            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.NamespacePrefix))
            {
                // could do with an ordered set
                // namespaceprefix should never be null. throw instead?
                if (model.NamespacePrefix != null)
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixOptions.Value;

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

                context.Builder.WithSettings(new GraphSyncPartSettings {NamespacePrefix = model.NamespacePrefix});
            }

            return Edit(contentTypePartDefinition);
        }
    }
}
