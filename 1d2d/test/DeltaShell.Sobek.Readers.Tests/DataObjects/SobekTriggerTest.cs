using System.Collections.Generic;
using System.Linq;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class SobekTriggerTest
    {
        [Test]
        public void Equals()
        {
            var sobekMeasurementLocations = new List<SobekTrigger>
                {
                    new SobekTrigger {Id = "SobekTrigger1"},
                    new SobekTrigger {Id = "SobekTrigger2"},
                    new SobekTrigger {Id = "SobekTrigger1"}
                };

            Assert.AreEqual(3, sobekMeasurementLocations.Count);
            Assert.AreEqual(2, sobekMeasurementLocations.Distinct().ToList().Count);
        }
    }
}
