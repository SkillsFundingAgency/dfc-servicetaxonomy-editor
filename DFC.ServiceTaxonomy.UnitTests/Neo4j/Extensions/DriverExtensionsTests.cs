using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.Neo4j.Extensions;
using DFC.ServiceTaxonomy.Neo4j.Models;
using FakeItEasy;
using Neo4j.Driver;
using Xunit;

namespace DFC.ServiceTaxonomy.UnitTests.Neo4j.Extensions
{
    public class DriverExtensionsTests
    {
        [Fact]
        public void DriverExtensions_WhenPassedCollection_ReturnsPrimaryDriver()
        {
            var listOfDrivers = new List<NeoDriver>() {
                new NeoDriver("Primary", A.Fake<IDriver>(), "http://my-primary-driver"),
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-driver"),
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-2-driver"),
            };

            var primaryDriver = listOfDrivers.PrimaryDriver();

            Assert.NotNull(primaryDriver);
        }

        [Fact]
        public void DriverExtensions_WhenPassedCollectionWithoutPrimaryDriver_ThrowsException()
        {
            var listOfDrivers = new List<NeoDriver>() {
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-driver"),
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-2-driver"),
            };

            var exception = Assert.Throws<InvalidOperationException>(listOfDrivers.PrimaryDriver);
        }

        [Fact]
        public void DriverExtensions_WhenPassedCollectionWithMultiplePrimaryDriver_ThrowsException()
        {
            var listOfDrivers = new List<NeoDriver>() {
                new NeoDriver("Primary", A.Fake<IDriver>(), "http://my-primary-driver"),
                new NeoDriver("Primary", A.Fake<IDriver>(), "http://my-primary-2-driver"),
            };

            var exception = Assert.Throws<InvalidOperationException>(listOfDrivers.PrimaryDriver);
        }

        [Fact]
        public void DriverExtensions_WhenAllDriversCalled_ReturnsAllDrivers()
        {
            var listOfDrivers = new List<NeoDriver>() {
                new NeoDriver("Primary", A.Fake<IDriver>(), "http://my-primary-driver"),
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-driver"),
                new NeoDriver("Secondary", A.Fake<IDriver>(), "http://my-secondary-2-driver"),
            };

            var drivers = listOfDrivers.AllDrivers();
                
            Assert.Equal(3, drivers.Count());
        }
    }
}
