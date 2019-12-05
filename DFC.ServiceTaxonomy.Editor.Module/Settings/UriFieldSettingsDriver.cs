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
    public class UriFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<UriField>
    {
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixConfiguration;

        public UriFieldSettingsDriver(IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixConfiguration)
        {
            _namespacePrefixConfiguration = namespacePrefixConfiguration;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<UriFieldSettings>("UriFieldSettings_Edit", model =>
                {
                    partFieldDefinition.PopulateSettings(model);

                    var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;
                    // don't preselect the first option (ncs prefix) because of the way the html datalist works
                    //todo: find 3rd party bootstrap component to use instead of datalist (bootstrap itself doesn't support an input with select combo out of the box)
                    //if (model.NamespacePrefix == null && currentNamespacePrefixConfiguration.NamespacePrefixOptions.Any())
                    //    model.NamespacePrefix = currentNamespacePrefixConfiguration.NamespacePrefixOptions.First();
                    model.NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions;
                })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new UriFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            //todo: extract method
            var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;
            // could do with an ordered set
            // namespaceprefix should never be null. throw instead?
            if (model.NamespacePrefix != null && !currentNamespacePrefixConfiguration.NamespacePrefixOptions.Contains(model.NamespacePrefix))
            {
                currentNamespacePrefixConfiguration.NamespacePrefixOptions.Add(model.NamespacePrefix);
            }

            return Edit(partFieldDefinition);
        }
#pragma warning restore S927 // parameter names should match base declaration and other partial definitions
    }
}
