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
    public class SewerPumpGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Pumps

        [Test]
        public void AddPumpToSewerConnectionFromGwswElement()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);

            //A sewer connection is created.
            var sewerConnection = element as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            //A Pump has been added to the above sewer connection.
            var featuresInConnection = sewerConnection.GetStructuresFromBranchFeatures<Pump>();
            Assert.IsNotNull(featuresInConnection);

            var foundPump = featuresInConnection.FirstOrDefault();
            Assert.IsNotNull(foundPump);
            Assert.AreEqual(typeof(Pump), foundPump.GetType());

            //Pumps should contain the above definition if the branch is added to the network.
            Assert.IsFalse(network.Pumps.Any());
            Assert.IsFalse(network.Branches.Contains(sewerConnection));
            Assert.IsFalse(network.SewerConnections.Contains(sewerConnection));

            network.Branches.Add(sewerConnection);

            Assert.IsTrue(network.Branches.Contains(sewerConnection));
            Assert.IsTrue(network.SewerConnections.Contains(sewerConnection));

            //Check now the pumps
            Assert.IsTrue(network.Pumps.Any());
            Assert.AreEqual(1, network.Pumps.Count());

            var networkPump = network.Pumps.FirstOrDefault();
            Assert.IsNotNull(networkPump);
            Assert.AreEqual(foundPump, networkPump);
        }

        [Test]
        [TestCase(SewerConnectionMapping.FlowDirection.FromStartToEnd, true)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromEndToStart, false)]
        [TestCase(SewerConnectionMapping.FlowDirection.Closed, true)] /*We do not map these two to anything, so default value should prevail*/
        [TestCase(SewerConnectionMapping.FlowDirection.Open, true)]/*We do not map these two to anything, so default value should prevail*/
        public void CreatePumpFromGwswElementWithExpectedValues(SewerConnectionMapping.FlowDirection flowDirection, bool flowDirectionIsPositive)
        {
            #region Setting expected values

            var startNode = "node001";
            var endNode = "node002";

            var connectionType = SewerConnectionMapping.ConnectionType.Pump;
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(connectionType);

            var waterType = SewerConnectionWaterType.MixedWasteWater;
            var waterTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);

            var flowDirectionString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(flowDirection);
            #endregion
            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;
            var gwswElement = GetSewerConnectionGwswElement(nvgString, startNode, endNode, connectionTypeString,
                nvgDouble, nvgDouble, flowDirectionString, nvgDouble, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            var createdPump = sewerConnection.GetStructuresFromBranchFeatures<Pump>().FirstOrDefault();
            Assert.IsNotNull(createdPump);

            Assert.AreEqual(flowDirectionIsPositive, createdPump.DirectionIsPositive);
        }

        [Test]
        public void SewerFeatureFactoryGivesDefaultValidGeometryToPump()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            var createdPump = sewerConnection.GetStructuresFromBranchFeatures<Pump>().FirstOrDefault();
            Assert.IsNotNull(createdPump);

            Assert.IsNotNull(createdPump.Geometry, "Default geometry not given to pump.");
            Assert.IsNotNull(createdPump.Geometry.Coordinates);
            Assert.IsTrue(createdPump.Geometry.Coordinates.Any());

        }

        [Test]
        public void TestCreatePumpThenCreateSewerConnectionWithThatPumpKeepsStructureValues()
        {
            //Create network
            var network = new HydroNetwork();

            #region GwswElements
            var typeDouble = "double";
            var structureId = "structure123";
            var pumpCapacity = 30.0;
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                }
            };

            var startNode = "node001";
            var endNode = "node002";
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };


            #endregion

            //Instance the Pump AS STRUCTURE
            var structureElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNotNull(structureElement);
            Assert.IsTrue(network.Pumps.Any(p => p.Name.Equals(structureId)));
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));

            var pumpPh = network.Pumps.FirstOrDefault();
            Assert.NotNull(pumpPh);
            Assert.AreEqual(pumpCapacity, pumpPh.Capacity);

            //Instance the Pump AS SEWER CONNETION
            var connectionElement = SewerFeatureFactory.CreateInstance(sewerConnectionGwswElement, network);
            Assert.IsNotNull(connectionElement);
            Assert.IsTrue(network.Pumps.Any(p => p.Name.Equals(structureId)));
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));

            var replacedStructure = network.Pumps.FirstOrDefault(s => s.Name.Equals(pumpPh.Name));
            Assert.AreEqual(pumpPh, replacedStructure, "the attributes from the element do not match");

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

            //Create the pump, we know it works because of the previous tests.
            var network = new HydroNetwork();
            var sewerConnection = new SewerConnection();
            var pump = new Pump(pumpId);
            sewerConnection.BranchFeatures.Add(pump);
            network.Branches.Add(sewerConnection);
            Assert.IsTrue(network.Pumps.Contains(pump));

            //Now createInstance for the pump definition.
            var createdElement = new SewerPumpGenerator().Generate(structurePumpGwswElement, network);
            Assert.IsNotNull(createdElement);

            var createdPump = createdElement as Pump;
            Assert.IsNotNull(createdPump);
            Assert.AreEqual(pumpId, createdPump.Name);
            Assert.AreEqual(pumpCapacity, createdPump.Capacity);
            Assert.AreEqual(startLevelUpstreams, createdPump.StartDelivery);
            Assert.AreEqual(stopLevelUpstreams, createdPump.StopDelivery);
            Assert.AreEqual(startLevelDownstreams, createdPump.StartSuction);
            Assert.AreEqual(stopLevelDownstreams, createdPump.StopSuction);

        }
        #endregion
    }
}