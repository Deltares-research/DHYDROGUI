using System.Collections.Generic;
using System.Linq;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class SobekMeasurementLocationTest
    {
        [Test]
        public void Equals()
        {
            var sobekMeasurementLocations = new List<SobekMeasurementLocation>
                {
                    new SobekMeasurementLocation {Id = "SobekMeasurementLocation1"},
                    new SobekMeasurementLocation {Id = "SobekMeasurementLocation2"},
                    new SobekMeasurementLocation {Id = "SobekMeasurementLocation1"}
                };

            Assert.AreEqual(3, sobekMeasurementLocations.Count);
            Assert.AreEqual(2, sobekMeasurementLocations.Distinct().ToList().Count);
        }
    }
}
