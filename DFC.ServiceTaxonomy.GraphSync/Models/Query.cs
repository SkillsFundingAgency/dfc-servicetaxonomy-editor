namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    /// <summary>
    /// An executable query, i.e. the queries' text and its parameters.
    /// </summary>
    public class Query
    {
        public QueryDetail QueryDetail { get; }

        public Query(QueryDetail queryDetail)
        {
            QueryDetail = queryDetail;
        }

        public override string ToString()
        {
            return QueryDetail.Text;
        }
    }
}
