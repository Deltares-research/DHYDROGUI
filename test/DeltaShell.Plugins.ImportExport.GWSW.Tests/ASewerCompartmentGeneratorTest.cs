using System;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DHYDRO.Common.Logging;
using NSubstitute;
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var generator = new TestSewerCompartmentGenerator(logHandler);
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

        [Test]
        public void SetBaseCompartmentProperties_WhenSettingClosedCompartmentStorageTypeWithoutSpecifyingFloodableArea_FloodableAreaIsZero()
        {
            // Setup
            var random = new Random(707);

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var generator = new TestSewerCompartmentGenerator(logHandler);

            var compartment = new Compartment("randomName");

            GwswElement gwswElement = GetNodeGwswElement("randomString", "randomString", "randomString",
                                                         random.NextDouble(), random.NextDouble(), random.NextDouble(),
                                                         random.NextDouble(), "randomString", random.NextDouble(),
                                                         random.NextDouble(), random.NextDouble());
            GwswAttribute floodableAreaAttribute = gwswElement.GwswAttributeList.SingleOrDefault(attribute => string.Equals(attribute.GwswAttributeType.Key, ManholeMapping.PropertyKeys.FloodableArea));
            Assert.That(floodableAreaAttribute, Is.Not.Null);

            // Simulate missing value
            floodableAreaAttribute.ValueAsString = string.Empty;

            const string closedCompartmentStorageType = "KNV";
            GwswAttribute compartmentStorageAttribute = GetDefaultGwswAttribute("SURFACE_SCHEMATISATION", closedCompartmentStorageType, "RES");
            gwswElement.GwswAttributeList.Add(compartmentStorageAttribute);

            // Call
            generator.TestSetBaseCompartmentProperties(compartment, gwswElement);

            // Assert
            Assert.That(compartment.CompartmentStorageType, Is.EqualTo(CompartmentStorageType.Closed));
            Assert.That(compartment.FloodableArea, Is.Zero);
        }
        
        [Test]
        public void SetBaseCompartmentProperties_WhenSettingClosedCompartmentStorageTypeWithExplicitlySpecifiedFloodableArea_CorrectlySetsFloodableArea()
        {
            // Setup
            var random = new Random(707);

            double floodableArea = random.NextDouble();

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var generator = new TestSewerCompartmentGenerator(logHandler);

            var compartment = new Compartment("randomName");

            GwswElement gwswElement = GetNodeGwswElement("randomString", "randomString", "randomString",
                                                         random.NextDouble(), random.NextDouble(), random.NextDouble(),
                                                         random.NextDouble(), "randomString", floodableArea,
                                                         random.NextDouble(), random.NextDouble());

            const string closedCompartmentStorageType = "KNV";
            GwswAttribute compartmentStorageAttribute = GetDefaultGwswAttribute("SURFACE_SCHEMATISATION", closedCompartmentStorageType, "RES");
            gwswElement.GwswAttributeList.Add(compartmentStorageAttribute);

            // Call
            generator.TestSetBaseCompartmentProperties(compartment, gwswElement);

            // Assert
            Assert.That(compartment.CompartmentStorageType, Is.EqualTo(CompartmentStorageType.Closed));
            Assert.That(compartment.FloodableArea, Is.EqualTo(floodableArea).Within(0.00001));
        }

        private class TestSewerCompartmentGenerator : ASewerCompartmentGenerator
        {
            public TestSewerCompartmentGenerator(ILogHandler logHandler)
                : base(logHandler)
            {
            }
            public void TestSetBaseCompartmentProperties(Compartment compartment, GwswElement gwswElement)
            {
                ILogHandler logHandler = Substitute.For<ILogHandler>();
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