using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
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
            var pumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, sourceCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, targetCompartmentName, string.Empty)
                }
            };
            
            var createdPump = new SewerPumpGenerator().Generate(pumpGwswElement) as GwswConnectionPump;
            Assert.IsNotNull(createdPump);
            Assert.That(createdPump.SourceCompartmentId, Is.EqualTo(sourceCompartmentName));
            Assert.That(createdPump.TargetCompartmentId, Is.EqualTo(targetCompartmentName));
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
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(connectionType);

            var waterType = SewerConnectionWaterType.Combined;
            var waterTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);

            var flowDirectionString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(flowDirection);
            #endregion
            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;
            var gwswElement = GetSewerConnectionGwswElement(nvgString, startNode, endNode, connectionTypeString,
                nvgDouble, nvgDouble, flowDirectionString, nvgDouble, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);
            
            var createdPump = new SewerPumpGenerator().Generate(gwswElement) as Pump;
            Assert.IsNotNull(createdPump);
            Assert.That(createdPump.DirectionIsPositive, Is.EqualTo(expectedFlowDirectionValue));
        }

        [Test]
        public void AfterAddingAPumpYouCanExtendItsDefinition()
        {
            var typeDouble = "double";

            var pumpId = "pump123";
            var pumpCapacity = 30.0;
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
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                }
            };
            
            var createdPump = new SewerPumpGenerator().Generate(structurePumpGwswElement) as GwswStructurePump;
            Assert.IsNotNull(createdPump);
            Assert.That(createdPump.Name, Is.EqualTo(pumpId));
            Assert.That(createdPump.Capacity, Is.EqualTo(pumpCapacity));
            Assert.That(createdPump.StartDelivery, Is.EqualTo(startLevelUpstreams));
            Assert.That(createdPump.StopDelivery, Is.EqualTo(stopLevelUpstreams));
            Assert.That(createdPump.StartSuction, Is.EqualTo(startLevelDownstreams));
            Assert.That(createdPump.StopSuction, Is.EqualTo(stopLevelDownstreams));

        }
        #endregion
    }
}