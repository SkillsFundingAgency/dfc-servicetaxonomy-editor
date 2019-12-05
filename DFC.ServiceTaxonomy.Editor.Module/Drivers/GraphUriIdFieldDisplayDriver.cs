using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    //todo: need to associate a prefix with a content type when loading from a recipe
    public class GraphUriIdFieldDisplayDriver : ContentFieldDisplayDriver<GraphUriIdField>
    {
        private readonly IOptionsMonitor<NamespacePrefixConfiguration> _namespacePrefixConfiguration;

        public GraphUriIdFieldDisplayDriver(IOptionsMonitor<NamespacePrefixConfiguration> namespacePrefixConfiguration)
        {
            _namespacePrefixConfiguration = namespacePrefixConfiguration;
        }

        public override IDisplayResult Display(GraphUriIdField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Initialize<DisplayGraphUriIdFieldViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
                {
                    model.Field = field;
                    model.Part = fieldDisplayContext.ContentPart;
                    model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
                })
                .Location("Content")
                .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(GraphUriIdField field, BuildFieldEditorContext context)
        {
            return Initialize<EditGraphUriIdFieldViewModel>(GetEditorShapeType(context), model =>
            {
                // why is the prefix options empty?
                //var graphUriIdSettings = context.PartFieldDefinition.GetSettings<GraphUriIdFieldSettings>();
                //if (graphUriIdSettings.NamespacePrefix == null)
                //{
                //    var currentNamespacePrefixConfiguration = _namespacePrefixConfiguration.CurrentValue;

                //    // new content item without editing field settings
                //    //todo: if this works service to do this, as is used by settings driver also. could we do this in settings ctor?
                //    var defaultGraphUriFieldSettings = new GraphUriIdFieldSettings
                //    {
                //        NamespacePrefixOptions = currentNamespacePrefixConfiguration.NamespacePrefixOptions,
                //        NamespacePrefix = currentNamespacePrefixConfiguration.NamespacePrefixOptions.FirstOrDefault() 
                //    };

                //    context.PartFieldDefinition.PopulateSettings(defaultGraphUriFieldSettings);
                //}
                //model.Text = field.Text ?? $"{context.PartFieldDefinition.GetSettings<GraphUriIdFieldSettings>().NamespacePrefix}{context.TypePartDefinition.Name.ToLowerInvariant()}/{Guid.NewGuid():D}";
                string namespacePrefix =
                    context.PartFieldDefinition.GetSettings<GraphUriIdFieldSettings>().NamespacePrefix ??
                    _namespacePrefixConfiguration.CurrentValue.NamespacePrefixOptions.FirstOrDefault();

                model.Text = field.Text ?? $"{namespacePrefix}{context.TypePartDefinition.Name.ToLowerInvariant()}/{Guid.NewGuid():D}";
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphUriIdField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, context);
        }
    }
}
