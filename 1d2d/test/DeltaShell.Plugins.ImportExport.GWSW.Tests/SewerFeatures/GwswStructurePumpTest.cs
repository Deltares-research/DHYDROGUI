using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.SewerFeatures
{
    [TestFixture]
    public class GwswStructurePumpTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var instance = new GwswStructurePump("randomName");

            // Assert
            Assert.That(instance, Is.InstanceOf<Pump>());
            Assert.That(instance.StartDelivery, Is.Zero);
            Assert.That(instance.StopDelivery, Is.Zero);
            Assert.That(instance.StartSuction, Is.Zero);
            Assert.That(instance.StopSuction, Is.Zero);
        }
    }
}