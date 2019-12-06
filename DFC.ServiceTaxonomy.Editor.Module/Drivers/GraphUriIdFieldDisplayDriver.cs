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
    //todo: now custom field is in a content part -> the namespace prefix is at the content part level, rather than content type level, where it needs to be
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
                string namespacePrefix =
                    context.PartFieldDefinition.GetSettings<GraphUriIdFieldSettings>().NamespacePrefix ??
                    _namespacePrefixConfiguration.CurrentValue.NamespacePrefixOptions.FirstOrDefault();

                model.Text = field.Text ?? $"{namespacePrefix}{context.TypePartDefinition.ContentTypeDefinition.Name.ToLowerInvariant()}/{Guid.NewGuid():D}";
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
