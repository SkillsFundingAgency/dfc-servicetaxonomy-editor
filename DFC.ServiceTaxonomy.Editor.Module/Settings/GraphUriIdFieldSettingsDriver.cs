using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
#pragma warning disable S927 // parameter names should match base declaration and other partial definitions
    public class GraphUriIdFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GraphUriIdField>
    {
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixConfiguration;

        public GraphUriIdFieldSettingsDriver(IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixConfiguration)
        {
            _namespacePrefixConfiguration = namespacePrefixConfiguration;
        }
        //todo: don't really want/need user to name field???
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<GraphUriIdFieldSettings>("GraphUriIdFieldSettings_Edit", model =>
                {
                    var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;

                    //model.NamespacePrefix ??= currentNamespacePrefixConfiguration.NamespacePrefixOptions.FirstOrDefault();

                    partFieldDefinition.PopulateSettings(model);

                    //model.NamespacePrefixOptions ??= currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                    model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;

                    //var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;
                    // don't preselect the first option (ncs prefix) because of the way the html datalist works
                    //todo: find 3rd party bootstrap component to use instead of datalist (bootstrap itself doesn't support an input with select combo out of the box)
                    //if (model.NamespacePrefix == null && currentNamespacePrefixConfiguration.NamespacePrefixOptions.Any())
                    //    model.NamespacePrefix = currentNamespacePrefixConfiguration.NamespacePrefixOptions.First();

                    //model.NamespacePrefix ??= currentNamespacePrefixConfiguration.NamespacePrefixOptions.FirstOrDefault();

                    //model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new GraphUriIdFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            // could do with an ordered set
            // namespaceprefix should never be null. throw instead?
            if (model.NamespacePrefix != null)
            {
                var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;

                // if user has entered a new prefix, update the global list of prefixes, so that they get the choice when editing graph uri id fields in new content types
                // what do we do with pollution in the list, e.g. someone adds an incorrect prefix?
#pragma warning disable S1066 // Collapsible "if" statements should be merged
                if (!currentNamespacePrefixConfiguration.NamespacePrefixOptions.Contains(model.NamespacePrefix))
#pragma warning restore S1066 // Collapsible "if" statements should be merged
                    currentNamespacePrefixConfiguration.NamespacePrefixOptions.Add(model.NamespacePrefix);

                //if (!model.NamespacePrefixOptions?.Contains(model.NamespacePrefix) ?? false)
                //    model.NamespacePrefixOptions?.Add(model.NamespacePrefix);

                //partFieldDefinition.PartDefinition.GetSettings<>()
                //partFieldDefinition.PartDefinition.PopulateSettings();
                //partFieldDefinition.PopulateSettings();
            }

            // we just (manually) populate a datalist with NamespacePrefixOptions in the view, so we don't get the list back in the model
            // we could update the model, but we want the prefixes to be global, i.e. if a new content item is created, it should contain the full list of prefixes
            // so we always populate the model from the global list, instead of storing the current set in the model
            // note: if the list in the appsettings is updated, we'll probably lose the current global list
            // we could update appsettings with new prefixes, but if we do that, we'd probably want to allow the user to delete them,
            // so perhaps we just don't update appsettings in the app service when running ;-)

            // so update the model with the latest global set of prefix options
            //model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
#pragma warning restore S927 // parameter names should match base declaration and other partial definitions
    }
}
