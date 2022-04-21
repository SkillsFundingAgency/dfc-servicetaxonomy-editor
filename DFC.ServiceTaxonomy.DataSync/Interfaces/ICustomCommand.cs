namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface ICustomCommand : ICommand
    {
        public string? Command { get; set; }
    }
}
