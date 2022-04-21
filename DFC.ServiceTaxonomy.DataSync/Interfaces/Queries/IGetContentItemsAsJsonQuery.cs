namespace DFC.ServiceTaxonomy.DataSync.Interfaces.Queries
{
    interface IGetContentItemsAsJsonQuery : IQuery<string>
    {
        public string? QueryStatement { get; set; }
    }
}
