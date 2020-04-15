namespace DFC.ServiceTaxonomy.Neo4j.Helpers.Interface
{
    public interface ITcpPortHelper
    {
        bool IsListening(int portNumber);
    }
}
