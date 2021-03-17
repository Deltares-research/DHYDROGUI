using System;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class ASewerCompartmentGeneratorTest : SewerFeatureFactoryTestHelper
    {
        [Test]
        [TestCase(null, CompartmentStorageType.Reservoir)]
        [TestCase("", CompartmentStorageType.Reservoir)]
        [TestCase("   ", CompartmentStorageType.Reservoir)]
        [TestCase("RES", CompartmentStorageType.Reservoir)]
        [TestCase("KNV", CompartmentStorageType.Closed)]
        [TestCase("VRL", CompartmentStorageType.Reservoir)] // Not supported, setting default: 'Reservoir'
        public void SetBaseCompartmentProperties_SetsExpectedCompartmentStorageType(string gwswCompartmentStorageType, CompartmentStorageType expectedStorageType)
        {
            // Setup
            var random = new Random(80085);

            var generator = new TestSewerCompartmentGenerator();
            var compartment = new Compartment("randomName");

            GwswElement gwswElement = GetNodeGwswElement("randomString", "randomString", "randomString",
                                                         random.NextDouble(), random.NextDouble(), random.NextDouble(),
                                                         random.NextDouble(), "randomString", random.NextDouble(),
                                                         random.NextDouble(), random.NextDouble());

            GwswAttribute compartmentStorageAttribute = GetDefaultGwswAttribute("SURFACE_SCHEMATISATION", gwswCompartmentStorageType, "RES");
            gwswElement.GwswAttributeList.Add(compartmentStorageAttribute);

            // Call
            generator.TestSetBaseCompartmentProperties(compartment, gwswElement);

            // Assert
            Assert.That(compartment.CompartmentStorageType, Is.EqualTo(expectedStorageType));
        }

        private class TestSewerCompartmentGenerator : ASewerCompartmentGenerator
        {
            public void TestSetBaseCompartmentProperties(Compartment compartment, GwswElement gwswElement)
            {
                SetBaseCompartmentProperties(compartment, gwswElement);
            }

            public override ISewerFeature Generate(GwswElement gwswElement)
            {
                throw new NotImplementedException();
            }

            protected override void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement)
            {
                throw new NotImplementedException();
            }
        }
    }
}