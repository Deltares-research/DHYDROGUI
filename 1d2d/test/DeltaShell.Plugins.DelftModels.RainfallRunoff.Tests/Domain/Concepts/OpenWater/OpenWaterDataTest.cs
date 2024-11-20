using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.OpenWater
{
    [TestFixture]
    public class OpenWaterDataTest
    {
        [Test]
        public void Constructor_SetsCatchmentModelDataOnCatchment()
        {
            // Setup
            var catchment = new Catchment();

            // Call
            var data = new OpenWaterData(catchment);

            // Assert
            Assert.That(catchment.ModelData, Is.SameAs(data));
        }
    }
}