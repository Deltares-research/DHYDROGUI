using System.Collections.Generic;
using System.Linq;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class SobekControllerTest
    {
        [Test]
        public void Equals()
        {
            var sobekMeasurementLocations = new List<SobekController>
                {
                    new SobekController {Id = "SobekController1"},
                    new SobekController {Id = "SobekController2"},
                    new SobekController {Id = "SobekController1"}
                };

            Assert.AreEqual(3, sobekMeasurementLocations.Count);
            Assert.AreEqual(2, sobekMeasurementLocations.Distinct().ToList().Count);
        }
    }
}
