using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Helpers.Interface;

namespace DFC.ServiceTaxonomy.Neo4j.Helpers
{
    public class TcpPortHelper : ITcpPortHelper
    {
        public bool IsListening(int portNumber)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] objEndPoints = ipGlobalProperties.GetActiveTcpListeners();

            if (objEndPoints.Any(x => x.Port == portNumber))
            {
                return true;
            }

            return false;
        }
    }
}
