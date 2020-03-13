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
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixOptions;

        public GraphSyncPartSettingsDisplayDriver(IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixOptions)
        {
            _namespacePrefixOptions = namespacePrefixOptions;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixOptions.CurrentValue;

                    GraphSyncPartSettings graphSyncPartSettings = contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();

                    model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                    model.NamespacePrefix = graphSyncPartSettings.NamespacePrefix;
                    model.BagPartContentItemRelationshipType = graphSyncPartSettings.BagPartContentItemRelationshipType;
                    model.PreexistingNode = graphSyncPartSettings.PreexistingNode;
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

            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.NamespacePrefix,
                m => m.BagPartContentItemRelationshipType,
                m => m.PreexistingNode))
            {
                // could do with an ordered set
                // namespaceprefix should never be null. throw instead?
                if (model.NamespacePrefix != null)
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixOptions.CurrentValue;

                    // if user has entered a new prefix, update the global list of prefixes,
                    // so that they get the choice when editing graph uri id fields in new content types
                    // (or at least until the process is restarted, as we don't persist changes to a config source)
                    // what do we do with pollution in the list, e.g. someone adds an incorrect prefix?
                    //todo: this is a hack - IOptionsMonitor doesn't really support updates:
                    // OnChange notification isn't fired, (InvokeChanged is not part of the interface and is private)
                    // not necessarily thread-safe
                    // (IOptionsSnapshot clones the value, so any updates are lost anyway)
                    // better to update env variable?

                    if (!currentNamespacePrefixConfiguration.NamespacePrefixOptions.Contains(model.NamespacePrefix))
                        currentNamespacePrefixConfiguration.NamespacePrefixOptions.Add(model.NamespacePrefix);
                }

                // we just (manually) populate a datalist with NamespacePrefixOptions in the view, so we don't get the list back in the model
                // we could update the model, but we want the prefixes to be global, i.e. if a new content item is created, it should contain the full list of prefixes
                // so we always populate the model from the global list, instead of storing the current set in the model
                // note: if the list in the appsettings is updated, we'll probably lose the current global list
                // we could update appsettings with new prefixes, but if we do that, we'd probably want to allow the user to delete them,
                // so perhaps we just don't update appsettings in the app service when running ;-)

                context.Builder.WithSettings(new GraphSyncPartSettings
                {
                    NamespacePrefix = model.NamespacePrefix,
                    BagPartContentItemRelationshipType = model.BagPartContentItemRelationshipType,
                    PreexistingNode = model.PreexistingNode
                });
            }

            return Edit(contentTypePartDefinition);
        }
    }
}
