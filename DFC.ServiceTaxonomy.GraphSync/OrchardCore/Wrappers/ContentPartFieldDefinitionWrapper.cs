using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers
{
    public class ContentPartFieldDefinitionWrapper : IContentPartFieldDefinition
    {
        private readonly ContentPartFieldDefinition _contentPartFieldDefinition;

        public ContentPartFieldDefinitionWrapper(ContentPartFieldDefinition contentPartFieldDefinition)
        {
            _contentPartFieldDefinition = contentPartFieldDefinition;
        }

        string IContentDefinition.Name => _contentPartFieldDefinition.Name;

        public T GetSettings<T>() where T : new() => _contentPartFieldDefinition.GetSettings<T>();

        public void PopulateSettings<T>(T target) => _contentPartFieldDefinition.PopulateSettings(target);

        public ContentFieldDefinition FieldDefinition => _contentPartFieldDefinition.FieldDefinition;

        public ContentPartDefinition PartDefinition
        {
            get => _contentPartFieldDefinition.PartDefinition;
            set => _contentPartFieldDefinition.PartDefinition = value;
        }
    }
}
