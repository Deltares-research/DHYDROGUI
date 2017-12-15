using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    public class SewerConnectionGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Sewer Connection

        [Test]
        public void SewerFeatureCanNotBeCreatedIfTargetAndSourceAreNotGivenAndLogMessageIsGiven()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString()
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateInstance(nodeGwswElement), expectedMessage);
            Assert.IsNull(SewerFeatureFactory.CreateInstance(nodeGwswElement));
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfTargetIsNotGivenAndLogMessageIsGiven()
        {
            var sourceNode = "node001";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, sourceNode, string.Empty)
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateInstance(nodeGwswElement), expectedMessage);
            Assert.IsNull(SewerFeatureFactory.CreateInstance(nodeGwswElement));
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfSourceIsNotGivenAndLogMessageIsGiven()
        {
            var targetNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, targetNode, string.Empty)
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateInstance(nodeGwswElement), expectedMessage);
            Assert.IsNull(SewerFeatureFactory.CreateInstance(nodeGwswElement));
        }

        [Test]
        public void CreateSewerConnectionFromFactoryWithUnknownAttributes()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute("unkownCode", "ValueShouldNotBeSet", string.Empty, "unknownType"),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));
        }

        [Test]
        [TestCase("GSL", typeof(Pipe))]
        [TestCase("OVS", typeof(SewerConnection))]
        [TestCase("ITR", typeof(Pipe))]
        [TestCase("OPL", typeof(Pipe))]
        [TestCase("DRL", typeof(SewerConnectionOrifice))]
        [TestCase("PMP", typeof(SewerConnection))]
        public void CreateSewerConnectionMapsConnectionTypeFromFactory(string typeOfConnection, Type expectedType)
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };
            var network = new HydroNetwork();
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.AreEqual(expectedType, element.GetType(), "Created Sewer Connection is not of the expected type.");
            var sewerConnection = element as SewerConnection;
            Assert.NotNull(sewerConnection);
        }

        [Test]
        public void CreateSewerConnectionWithUnknownMapConnectionTypeFromFactory()
        {
            var typeOfConnection = "NotKnown";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var sewerConnection = element as SewerConnection;
            Assert.NotNull(sewerConnection);
        }

        [Test]
        public void CreateSewerConnectionFromFactoryCreatesDefaultNodesIfTheyAreNotPresentInNetwork()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            Assert.IsFalse(network.Manholes.Any());

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));

            var createdConnection = element as SewerConnection;
            Assert.IsNotNull(createdConnection);

            //Defined
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(startNode)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(endNode)));

            //Created default manholes
            Assert.IsNotNull(createdConnection.Source);
            var sourceAsManhole = createdConnection.Source as Manhole;
            Assert.IsNotNull(sourceAsManhole);
            Assert.IsTrue(sourceAsManhole.ContainsCompartmentWithName(startNode));

            Assert.IsNotNull(createdConnection.Target);
            var targetAsManhole = createdConnection.Target as Manhole;
            Assert.IsNotNull(targetAsManhole);
            Assert.IsTrue(targetAsManhole.ContainsCompartmentWithName(endNode));

            //Created default compartments.
            Assert.IsNotNull(createdConnection.SourceCompartment);
            Assert.AreEqual(startNode, createdConnection.SourceCompartment.Name);

            Assert.IsNotNull(createdConnection.TargetCompartment);
            Assert.AreEqual(endNode, createdConnection.TargetCompartment.Name);
        }

        [Test]
        public void CreateSewerConnectionFromFactoryAssignsExistingNodesIfTheyArePresentInNetwork()
        {
            //Create network and nodes.
            var network = new HydroNetwork();

            var startNodeName = "man001";
            var startCompartmentName = "node001";
            var startNode = new Manhole(startNodeName);
            var startCompartment = new Compartment(startCompartmentName);
            startNode.Compartments.Add(startCompartment);
            network.Nodes.Add(startNode);

            var endNodeName = "man001";
            var endCompartmentName = "node002";
            var endNode = new Manhole(endNodeName);
            var endCompartment = new Compartment(endCompartmentName);
            endNode.Compartments.Add(endCompartment);
            network.Nodes.Add(endNode);

            Assert.IsTrue(network.Manholes.Any(n => n.Name.Equals(startNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(startCompartmentName)));
            Assert.IsTrue(network.Manholes.Any(n => n.Name.Equals(endNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(endCompartmentName)));

            //Create element and instantiate it.
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        GwswAttributeType = GetGwswAttributeType("testFile", 5, "columnName", "string", SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart,
                            "unkownDefinition", "mandatoryMaybe", string.Empty, "noRemarks"),
                        ValueAsString = startCompartmentName
                    },
                    new GwswAttribute
                    {
                        GwswAttributeType = GetGwswAttributeType("testFile", 6, "columnName", "string", SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd,
                            "unkownDefinition", "mandatoryMaybe", string.Empty, "noRemarks"),
                        ValueAsString = endCompartmentName
                    },
                }
            };
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));

            var createdConnection = element as SewerConnection;
            Assert.IsNotNull(createdConnection);

            Assert.IsTrue(network.Manholes.Any(n => n.Name.Equals(startNodeName)));
            Assert.IsNotNull(createdConnection.Source);
            Assert.AreEqual(startNode, createdConnection.Source as Manhole);

            Assert.IsTrue(network.Manholes.Any(n => n.Name.Equals(endNodeName)));
            Assert.IsNotNull(createdConnection.Target);
            Assert.AreEqual(endNode, createdConnection.Target as Manhole);
        }

        [Test]
        public void CreateSewerConnectionReturnsObjectWithExpectedValues()
        {
            #region Setting expected values
            var objectId = "Obj123";

            var connectionType = SewerConnectionMapping.ConnectionType.Orifice;
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(connectionType);

            var levelStart = 2.0;
            var levelEnd = 2.5;
            var length = 5.0;

            var waterType = SewerConnectionWaterType.DryWeatherRainage;
            var waterTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);
            #endregion
            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;

            var gwswElement = GetSewerConnectionGwswElement(objectId, nvgString, nvgString, connectionTypeString,
                levelStart, levelEnd, nvgString, length, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);

            var network = new HydroNetwork();

            var createdElement = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            Assert.AreEqual(objectId, sewerConnection.Name);
            Assert.AreEqual(levelStart, sewerConnection.LevelSource);
            Assert.AreEqual(levelEnd, sewerConnection.LevelTarget);
            Assert.AreEqual(length, sewerConnection.Length);
            Assert.AreEqual(waterType, sewerConnection.WaterType);
        }

        [Test]
        public void SewerFeatureFactoryGivesDefaultValidGeometryToSewerConnection()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode, string.Empty)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            Assert.IsNotNull(sewerConnection.Geometry, "Default geometry not given to Sewer Connection.");
            Assert.IsNotNull(sewerConnection.Geometry.Coordinates);
            Assert.IsTrue(sewerConnection.Geometry.Coordinates.Any());
        }

        #endregion
    }
}