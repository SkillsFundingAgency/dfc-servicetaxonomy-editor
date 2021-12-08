namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface ICustomCommand : ICommand
    {
        public string? Command { get; set; }
    }
}
