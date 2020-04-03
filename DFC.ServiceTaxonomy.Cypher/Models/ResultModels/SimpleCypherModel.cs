namespace DFC.ServiceTaxonomy.Cypher.Models.ResultModels
{
    public class SimpleCypherModel : IQueryResultModel
    {
        public string Filter => Occupation;

        public string Title { get; set; }

        public string LastModified { get; set; }

        public string Occupation { get; set; }
    }
}
