using System;
using System.Collections.Generic;
using Neo4j.Driver;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Models.Interface;

namespace DFC.ServiceTaxonomy.Neo4j.Extensions
{
    public static class DriverExtensions
    {
        public static IDriver PrimaryDriver(this IEnumerable<INeoDriver> drivers)
        {
            var primaryDrivers = drivers.Where(x => string.Equals(x.Type, "Primary", StringComparison.CurrentCultureIgnoreCase));

            if(primaryDrivers.Count() > 1)
            {
                throw new InvalidOperationException("More than 1 enabled Primary Driver defined in Neo4JConfiguration");
            }

            if(!primaryDrivers.Any())
            {
                throw new InvalidOperationException("No enabled Primary Driver defined in Neo4JConfiguration");
            }

            return primaryDrivers.FirstOrDefault().Driver;
        }

        public static IEnumerable<IDriver> AllDrivers(this IEnumerable<INeoDriver> drivers)
        {
            return drivers.Select(x => x.Driver);
        }
    }
}
