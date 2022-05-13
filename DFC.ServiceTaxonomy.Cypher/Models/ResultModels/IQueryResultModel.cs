using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.Cypher.Models.ResultModels
{
    public interface IQueryResultModel
    {
        [JsonIgnore]
        string? Filter { get; }
    }
}
