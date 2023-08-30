using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.GWSW.Decorators;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.Decorators
{
    [TestFixture]
    public class SewerCompartmentFloodableAreaDecoratorTest
    {
        [Test]
        public void ProcessInput_ForReservoirCompartment_ValueMissing_SetsTheFloodableAreaWith500_AndLogsWarning()
        {
            // Setup
            var compartment = new Compartment
            {
                Name = "some_compartment",
                CompartmentStorageType = CompartmentStorageType.Reservoir
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);

            // Call
            void Call() => decorator.ProcessInput(gwswElement);

            // Assert
            IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
            Assert.That(warnings, Does.Contain("Missing floodable area value for 'some_compartment', using default value: 500"));
            Assert.That(compartment.FloodableArea, Is.EqualTo(500.0));
        }

        [Test]
        public void ProcessInput_ForReservoirCompartment_ValueProvided_SetsTheFloodableAreaWithProvidedValue()
        {
            // Setup
            var compartment = new Compartment
            {
                Name = "some_compartment",
                CompartmentStorageType = CompartmentStorageType.Reservoir
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);
            GwswAttribute floodableAreaAttribute = CreateGwswAttribute("FLOODABLE_AREA", 1.23);
            gwswElement.GwswAttributeList.Add(floodableAreaAttribute);

            // Call
            decorator.ProcessInput(gwswElement);

            // Assert
            Assert.That(compartment.FloodableArea, Is.EqualTo(1.23));
        }

        [Test]
        [TestCase]
        public void ProcessInput_ForClosedCompartment_ValueMissing_SetsTheFloodableAreaWith0_AndLogsWarning()
        {
            // Setup
            var compartment = new Compartment
            {
                Name = "some_compartment",
                CompartmentStorageType = CompartmentStorageType.Closed
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);

            // Call
            void Call() => decorator.ProcessInput(gwswElement);

            // Assert
            IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
            Assert.That(warnings, Does.Contain("Missing floodable area value for 'some_compartment', using default value: 0"));
            Assert.That(compartment.FloodableArea, Is.EqualTo(0.0));
        }

        [Test]
        public void ProcessInput_ForClosedCompartment_ValueProvided_SetsTheFloodableAreaWithProvidedValue()
        {
            // Setup
            var compartment = new Compartment
            {
                Name = "some_compartment",
                CompartmentStorageType = CompartmentStorageType.Closed
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);
            GwswAttribute floodableAreaAttribute = CreateGwswAttribute("FLOODABLE_AREA", 1.23);
            gwswElement.GwswAttributeList.Add(floodableAreaAttribute);

            // Call
            decorator.ProcessInput(gwswElement);

            // Assert
            Assert.That(compartment.FloodableArea, Is.EqualTo(1.23));
        }

        private static GwswAttribute CreateGwswAttribute(string key, double value)
        {
            var attributeType = new GwswAttributeType
            {
                Key = key,
                AttributeType = typeof(double)
            };
            var attribute = new GwswAttribute
            {
                GwswAttributeType = attributeType,
                ValueAsString = value.ToString(CultureInfo.InvariantCulture)
            };

            return attribute;
        }
    }
}