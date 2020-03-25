namespace DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces
{
    public interface IContentDefinition
    {
        string Name { get; }

        T GetSettings<T>() where T : new();
        void PopulateSettings<T>(T target);
    }
}
