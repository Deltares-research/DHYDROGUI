using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerPumpGeneratorTest : SewerFeatureFactoryTestHelper
    {
        #region Pumps

        [Test]
        public void AddPumpToSewerConnectionFromGwswElement()
        {
            var sourceCompartmentName = "cmp001";
            var targetCompartmentName = "cmp002";
            var length = 4.1;
            var pumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, SewerConnectionMapping.ConnectionType.Pump.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, sourceCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, targetCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.Length, length.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdPump = new SewerPumpGenerator(logHandler).Generate(pumpGwswElement) as GwswConnectionPump;
            Assert.IsNotNull(createdPump);
            Assert.That(createdPump.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(createdPump.TargetCompartmentName, Is.EqualTo(targetCompartmentName));
            Assert.That(createdPump.Length, Is.EqualTo(length));
        }

        [Test]
        [TestCase(SewerConnectionMapping.FlowDirection.FromStartToEnd, true)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromEndToStart, false)]
        [TestCase(SewerConnectionMapping.FlowDirection.Closed, true)]
        [TestCase(SewerConnectionMapping.FlowDirection.Open, true)]
        public void CreatePumpFromGwswElementWithExpectedValues(SewerConnectionMapping.FlowDirection flowDirection, bool expectedFlowDirectionValue)
        {
            #region Setting expected values

            var startNode = "node001";
            var endNode = "node002";

            var connectionType = SewerConnectionMapping.ConnectionType.Pump;
            var connectionTypeString = connectionType.GetDescription();

            var waterType = SewerConnectionWaterType.Combined;
            var waterTypeString = waterType.GetDescription();

            var flowDirectionString = flowDirection.GetDescription();
            #endregion
            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;
            var gwswElement = GetSewerConnectionGwswElement(nvgString, startNode, endNode, connectionTypeString,
                nvgDouble, nvgDouble, flowDirectionString, nvgDouble, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdPump = new SewerPumpGenerator(logHandler).Generate(gwswElement) as Pump;
            Assert.IsNotNull(createdPump);
            Assert.That(createdPump.DirectionIsPositive, Is.EqualTo(expectedFlowDirectionValue));
        }

        [Test]
        public void AfterAddingAPumpYouCanExtendItsDefinition()
        {
            var typeDouble = "double";

            var pumpId = "pump123";
            var pumpCapacity = 36.0;
            var expectedPumpCapacity = pumpCapacity / 3600; // Pump capacity in Gwsw files is in m3/hour, so divide by 3600 for m3/s
            var startLevelDownstreams = 0.5;
            var stopLevelDownstreams = 1;
            var startLevelUpstreams = -1;
            var stopLevelUpstreams = -0.5;
            var structurePumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, pumpId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, SewerConnectionMapping.ConnectionType.Pump.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                }
            };

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdPump = new SewerPumpGenerator(logHandler).Generate(structurePumpGwswElement) as GwswStructurePump;
            Assert.IsNotNull(createdPump);
            Assert.That((object) createdPump.Name, Is.EqualTo(pumpId));
            Assert.That(createdPump.Capacity, Is.EqualTo(expectedPumpCapacity));
            Assert.That(createdPump.StartDelivery, Is.EqualTo(startLevelUpstreams));
            Assert.That(createdPump.StopDelivery, Is.EqualTo(stopLevelUpstreams));
            Assert.That(createdPump.StartSuction, Is.EqualTo(startLevelDownstreams));
            Assert.That(createdPump.StopSuction, Is.EqualTo(stopLevelDownstreams));

        }
        #endregion
    }
}