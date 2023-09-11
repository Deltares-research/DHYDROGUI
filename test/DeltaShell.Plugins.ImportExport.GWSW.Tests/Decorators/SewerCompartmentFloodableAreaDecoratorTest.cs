using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.GWSW.Decorators;
using DHYDRO.Common.Logging;
using log4net.Core;
using NSubstitute;
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var compartment = new Compartment(logHandler, "some_compartment")
            {
                CompartmentStorageType = CompartmentStorageType.Reservoir
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);

            // Call
            decorator.ProcessInput(gwswElement);

            // Assert
            logHandler.Received().ReportWarningFormat("Missing floodable area value for 'some_compartment', using default value: 500");
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GwswAttribute floodableAreaAttribute = CreateGwswAttribute("FLOODABLE_AREA", 1.23, logHandler);
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var compartment = new Compartment(logHandler, "some_compartment")
            {
                CompartmentStorageType = CompartmentStorageType.Closed,
            };
            var gwswElement = new GwswElement();

            var decorator = new SewerCompartmentFloodableAreaDecorator(compartment);

            // Call
            decorator.ProcessInput(gwswElement);

            // Assert
            
            logHandler.Received().ReportWarningFormat("Missing floodable area value for 'some_compartment', using default value: 0");
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GwswAttribute floodableAreaAttribute = CreateGwswAttribute("FLOODABLE_AREA", 1.23, logHandler);
            gwswElement.GwswAttributeList.Add(floodableAreaAttribute);

            // Call
            decorator.ProcessInput(gwswElement);

            // Assert
            Assert.That(compartment.FloodableArea, Is.EqualTo(1.23));
        }

        private static GwswAttribute CreateGwswAttribute(string key, double value, ILogHandler logHandler)
        {
            var attributeType = new GwswAttributeType(logHandler)
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