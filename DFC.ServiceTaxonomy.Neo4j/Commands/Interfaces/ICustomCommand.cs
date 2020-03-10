namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface ICustomCommand : ICommand
    {
        public string? Command { get; set; }
    }
}
