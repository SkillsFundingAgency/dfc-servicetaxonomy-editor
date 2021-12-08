namespace DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries
{
    interface IGetContentItemsAsJsonQuery : IQuery<string>
    {
        public string? QueryStatement { get; set; }
    }
}
