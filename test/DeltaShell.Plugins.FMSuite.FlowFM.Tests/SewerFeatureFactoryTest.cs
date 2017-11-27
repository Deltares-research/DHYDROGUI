using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerFeatureFactoryTest
    {
        [Test]
        public void SewerFeatureGetsAllHydrObjects()
        {
            var network = new HydroNetwork();
            try
            {
                var allHydroObjectsInNetwork = network.AllHydroObjects.ToList();
                network.Nodes.Add(new Manhole("ManholeTest"));
                allHydroObjectsInNetwork = network.AllHydroObjects.ToList();
            }
            catch (Exception e)
            {
                Assert.Fail("Could not cast HydroOjbects: {0}", e.Message);
            }
        }

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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, sourceNode),
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains( () => SewerFeatureFactory.CreateInstance(nodeGwswElement), expectedMessage);
            Assert.IsNull(SewerFeatureFactory.CreateInstance(nodeGwswElement));
        }

        [Test]
        public void SewerFeatureCanNotBeCreatedIfSourceIsNotGivenAndLogMessageIsGiven()
        {
            var targetNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, targetNode)
                }
            };

            var expectedMessage =
                "Cannot import sewer connection(s) without Source and Target nodes. Please check the file for said empty fields";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateInstance(nodeGwswElement), expectedMessage);
            Assert.IsNull(SewerFeatureFactory.CreateInstance(nodeGwswElement));
        }

        [Test]
        public void SewerFeatureTypeCanBeRetrievedWithAStringValue()
        {
            SewerFeatureType testValue;
            Assert.IsFalse(Enum.TryParse("failValue", out testValue));
            Assert.IsTrue(Enum.TryParse(SewerFeatureType.Connection.ToString(), out testValue));
        }

        [Test]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        public void NotKnownSewerFeaturesDoNotInstantiate(SewerFeatureType type)
        {
            /* When the above features are added to the object model they can remove from this test. */
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };

            try
            {
                var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
                Assert.IsNull(element);
            }
            catch (Exception e)
            {
                Assert.Fail("There was a problem while instantiating. {0}", e.Message);
            }
        }

        [Test]
        public void CreateSewerConnectionFromFactoryWithUnknownAttributes()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("unkownCode", "ValueShouldNotBeSet", "unknownType"),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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
        [TestCase("DRL", typeof(SewerConnection))]
        [TestCase("PMP", typeof(SewerConnection))]
        public void CreateSewerConnectionMapsConnectionTypeFromFactory(string typeOfConnection, Type expectedType)
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeType, typeOfConnection),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeType, typeOfConnection),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
                }
            };

            var network = new HydroNetwork();
            Assert.IsFalse(network.Manholes.Any());

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));

            var createdConnection = element as SewerConnection;
            Assert.IsNotNull(createdConnection);

            //Defined
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(startNode)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(endNode)));

            //Created default manholes
            Assert.IsNotNull(createdConnection.Source);
            var sourceAsManhole = createdConnection.Source as Manhole;
            Assert.IsNotNull(sourceAsManhole);
            Assert.IsTrue(sourceAsManhole.ContainsCompartment(startNode));

            Assert.IsNotNull(createdConnection.Target);
            var targetAsManhole = createdConnection.Target as Manhole;
            Assert.IsNotNull(targetAsManhole);
            Assert.IsTrue(targetAsManhole.ContainsCompartment(endNode));

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
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(startCompartmentName)));
            Assert.IsTrue(network.Manholes.Any(n => n.Name.Equals(endNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(endCompartmentName)));

            //Create element and instantiate it.
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", _nodeUniqueIdStart,
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startCompartmentName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", _nodeUniqueIdEnd,
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
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

            var connectionType = ConnectionType.Orifice;
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
            Assert.IsNotNull( sewerConnection);

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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.Orifice)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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

        #region Pipes

        [Test]
        [TestCase("GSL", true)]
        [TestCase("ITR", true)]
        [TestCase("OPL", true)]
        [TestCase("OVS", false)]
        [TestCase("DRL", false)]
        [TestCase("PMP", false)]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromFactory(string typeOfConnection, bool isPipe)
        {
            var pipeId = "123";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeIndicator, isPipe ? pipeId : string.Empty),
                    GetDefaultGwswAttribute(_pipeType, typeOfConnection),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
                }
            };
            var network = new HydroNetwork();

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.AreEqual(isPipe, element is Pipe);
            if (isPipe)
            {
                var pipe = element as Pipe;
                Assert.NotNull(pipe);
                Assert.AreEqual(pipeId, pipe.PipeId);
            }
        }

        [Test]
        public void CreatePipeFromFactoryWithKnownAttributes()
        {
            var startLevel = 30;
            var endLevel = 100;
            var length = 200;
            var crossSectionDef = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var pipeType = ConnectionType.ClosedConnection;
            var pipeTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(pipeType);

            var defaultString = string.Empty;
            var defaultDouble = 0.0;

            var nodeGwswElement = GetSewerConnectionGwswElement(string.Empty, startNode, endNode, pipeTypeString, startLevel, endLevel, defaultString, length, crossSectionDef, defaultString, defaultString, defaultDouble, defaultDouble,defaultDouble,defaultDouble );

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            Assert.IsNull(createdPipe.Source);
            Assert.IsNull(createdPipe.Target);
            Assert.IsNull(createdPipe.CrossSectionShape);

            //Defined
            Assert.IsNotNull(createdPipe.LevelSource);
            Assert.AreEqual(startLevel, createdPipe.LevelSource);

            Assert.IsNotNull(createdPipe.LevelTarget);
            Assert.AreEqual(endLevel, createdPipe.LevelTarget);

            Assert.IsNotNull(createdPipe.Length);
            Assert.AreEqual(length, createdPipe.Length);
        }

        [Test]
        public void CreatePipeFromFactoryCreatesDefaultCrossSectionDefinitionIfNotPresentInNetwork()
        {
            var sewerDefinitionName = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_crossSectionDef, sewerDefinitionName),
                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.ClosedConnection)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
                }
            };

            var network = new HydroNetwork();
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            //Defined
            Assert.IsNotNull(createdPipe.CrossSectionShape);
            Assert.AreEqual(sewerDefinitionName, createdPipe.CrossSectionShape.Name);
            Assert.IsTrue(network.SewerProfiles.Any( cs => cs.Name.Equals(sewerDefinitionName)));
        }

        [Test]
        public void CreatePipeFromFactoryAssignsExistingCrossSectionDefinitionIfPresentInNetwork()
        {
            var network = new HydroNetwork();
            Assert.IsFalse(network.SewerProfiles.Any());

            var sewerDefinitionName = "crossSectionDef001";
            var auxCrossSection = CrossSection.CreateDefault();
            auxCrossSection.Name = sewerDefinitionName;

            network.SewerProfiles.Add(auxCrossSection);
            Assert.IsTrue(network.SewerProfiles.Any( sp => sp.Name.Equals(sewerDefinitionName)));

            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_crossSectionDef, sewerDefinitionName),
                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.ClosedConnection)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(Pipe)));

            var createdPipe = element as Pipe;
            Assert.IsNotNull(createdPipe);

            //Defined
            Assert.IsNotNull(createdPipe.CrossSectionShape);
            Assert.AreEqual(sewerDefinitionName, createdPipe.CrossSectionShape.Name);
            Assert.IsTrue(network.SewerProfiles.Any(cs => cs.Name.Equals(sewerDefinitionName)));
            Assert.AreEqual(auxCrossSection, createdPipe.CrossSectionShape);
        }

        #endregion

        #region Pumps

        [Test]
        public void AddPumpToSewerConnectionFromGwswElement()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.Pump)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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
        [TestCase(FlowDirection.FromStartToEnd, true)]
        [TestCase(FlowDirection.FromEndToStart, false)]
        [TestCase(FlowDirection.Closed, true)] /*We do not map these two to anything, so default value should prevail*/
        [TestCase(FlowDirection.Open, true)]/*We do not map these two to anything, so default value should prevail*/
        public void CreatePumpFromGwswElementWithExpectedValues(FlowDirection flowDirection, bool flowDirectionIsPositive)
        {
            #region Setting expected values

            var startNode = "node001";
            var endNode = "node002";

            var connectionType = ConnectionType.Pump;
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.Pump)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
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
        [TestCase(StructureType.Crest, false)]
        [TestCase(StructureType.Orifice, false)]
        [TestCase(StructureType.Outlet, false)]
        [TestCase(StructureType.Pump, true)]
        public void SewerFeatureFactoryReturnsStructuresWhenGivingNameForStructure(StructureType structureType, bool mapped)
        {
            var structureId = "structure123";
            var structurePumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_uniqueId, structureId),
                    GetDefaultGwswAttribute(_structureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structurePumpGwswElement, network);
            Assert.AreEqual(mapped, createdElement != null);
        }

        [Test]
        public void SewerFeatureFactoryReturnsNullStructuresWhenNotGivingNameForStructure()
        {
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_structureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureType.Pump)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNull(createdElement);
        }

        [Test]
        [TestCase(StructureType.Crest, false)]
        [TestCase(StructureType.Orifice, false)]
        [TestCase(StructureType.Outlet, false)]
        [TestCase(StructureType.Pump, true)]
        public void SewerFeatureFactoryCreatesStructureAndSewerConnectionIfNeitherExists(StructureType structureType, bool mapped)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_uniqueId, structureId),
                    GetDefaultGwswAttribute(_structureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.AreEqual(mapped, createdElement != null);
            if (!mapped) return;

            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsTrue(network.SewerConnections.Any(s => s.Name.Equals(structureId)));

            Assert.IsTrue(network.Structures.Any());
            Assert.IsTrue(network.Structures.Any( s => s.Name.Equals(structureId)));

            Assert.IsTrue(network.CompositeBranchStructures.Any());
            Assert.IsTrue(network.CompositeBranchStructures.Any( cb => cb.Structures.Any( s => s.Name.Equals(structureId))));
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_uniqueId, structureId),
                    GetDefaultGwswAttribute(_structureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureType.Pump)),
                    GetDefaultGwswAttribute(_pumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                }
            };

            var startNode = "node001";
            var endNode = "node002";
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {

                    GetDefaultGwswAttribute(_pipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.Pump)),
                    GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(_nodeUniqueIdEnd, endNode)
                }
            };


            #endregion

            //Instance the Pump AS STRUCTURE
            var structureElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNotNull(structureElement);
            Assert.IsTrue(network.Pumps.Any( p => p.Name.Equals(structureId)));
            Assert.IsTrue(network.SewerConnections.Any(p => p.Name.Equals(structureId)));

            var pumpPh = network.Pumps.FirstOrDefault();
            Assert.NotNull(pumpPh);
            Assert.AreEqual(pumpCapacity, pumpPh.Capacity);

            //Instance the Pump AS SEWER CONNETION
            var connectionElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(_uniqueId, pumpId),
                    GetDefaultGwswAttribute(_structureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(ConnectionType.Pump)),
                    GetDefaultGwswAttribute(_pumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(_startLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(_stopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(_startLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(_stopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
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
            var createdElement = SewerFeatureFactory.CreateInstance(structurePumpGwswElement, network);
            Assert.IsNotNull(createdElement);

            var createdPump = createdElement as Pump;
            Assert.IsNotNull(createdPump);
            Assert.AreEqual(pumpId, createdPump.Name);
            Assert.AreEqual(pumpCapacity, createdPump.Capacity);
            Assert.AreEqual(startLevelDownstreams, createdPump.StartDelivery);
            Assert.AreEqual(stopLevelDownstreams, createdPump.StopDelivery);
            Assert.AreEqual(startLevelUpstreams, createdPump.StartSuction);
            Assert.AreEqual(stopLevelUpstreams, createdPump.StopSuction);

        }
        #endregion

        #endregion

        #region Manhole

        [Test]
        public void GivenSimpleManholeData_WhenCreatingWithFactory_ThenManholeIsCorrectlyReturned()
        {
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement1);
            var compartment = element as Compartment;
            
            // Check Compartment properties
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, "put1", "01001", 7071, 7071, CompartmentShape.Square, 45.67, 0.01, 2.75, new Coordinate(400.0, 50.0), 1);
        }

        [Test]
        public void GivenGwswElementWithNotAllAttributesDefined_WhenCreatingManhole_ThenNoExceptionAndMissingPropertiesAreNotDefinedOrHaveDefaultValues()
        {
            var uniqueId = "put1";
            var manholeId = "01001";
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = uniqueId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var compartment = SewerFeatureFactory.CreateInstance(gwswElement) as Compartment;
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0, 0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, new Coordinate(0, 0), 1);
        }
        
        [Test]
        public void GivenGwswElementWithBadlyFormattedStringForShape_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = compartmentId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "UnkownValue",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.NodeShape, "MyDescription", null, null)
                    }
                }
            };

            TryCreateFeatureAndCheckForLogMessageAndCheckFeatureValidity(manholeId, badGwswElement, compartmentId);
        }

        [TestCase("01FA", ManholePropertyKeys.NodeWidth)]
        [TestCase("01FA", ManholePropertyKeys.NodeLength)]
        [TestCase("01FA", ManholePropertyKeys.FloodableArea)]
        [TestCase("01FA", ManholePropertyKeys.BottomLevel)]
        [TestCase("01FA", ManholePropertyKeys.SurfaceLevel)]
        public void GivenGwswElementWithBadlyFormattedStringForDoubleValue_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned(string badlyFormattedEntry, string keyValue)
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = compartmentId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = badlyFormattedEntry,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            keyValue, "MyDescription", null, null)
                    }
                }
            };

            TryCreateFeatureAndCheckForLogMessageAndCheckFeatureValidity(manholeId, badGwswElement, compartmentId);
        }

        [TestCase("01FA", "23.6")]
        [TestCase("23.6", "01FA")]
        public void GivenGwswElementWithBadEntriesForCoordinateValues_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned(string xStringValue, string yStringValue)
        {
            var compartmentId = "put1";
            var manholeId = "01001";
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = compartmentId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = xStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholePropertyKeys.XCoordinate, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = yStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholePropertyKeys.YCoordinate, "MyDescription", null, null)
                    }
                }
            };
            
            TryCreateFeatureAndCheckForLogMessageAndCheckFeatureValidity(manholeId, badGwswElement, compartmentId);
        }

        [Test]
        public void GivenGwswElementWithMissingUniqueId_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = "01001",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholePropertyKeys.ManholeId)
                .Select(attr => attr.ValueAsString).FirstOrDefault();
            TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(badGwswElement, "Manhole with manhole id '" + manholeId + "' could not be created, because one of its compartments misses its unique id.");
        }

        [Test]
        public void GivenGwswElementWithMissingManholeId_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
        {
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = "put1",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    }
                }
            };
            
            TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(badGwswElement, "There are lines in 'Knooppunt.csv' that do not contain a Manhole Id. These lines are not imported.");
        }

        [TestCase(ManholePropertyKeys.NodeLength)]
        [TestCase(ManholePropertyKeys.NodeWidth)]
        [TestCase(ManholePropertyKeys.FloodableArea)]
        [TestCase(ManholePropertyKeys.BottomLevel)]
        [TestCase(ManholePropertyKeys.SurfaceLevel)]
        [TestCase(ManholePropertyKeys.NodeShape)]
        public void GivenGwswElementWithEmptyValue_WhenCreatingWithFactory_ThenDefaultValuesAreGivenToTheCorrespondingCompartmentProperty(string manholePropertyKey)
        {
            var uniqueId = "put1";
            var manholeId = "01001";
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = uniqueId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            manholePropertyKey, "MyDescription", null, null)
                    }
                }
            };

            var compartment = SewerFeatureFactory.CreateInstance(gwswElement) as Compartment;
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0.0, 0.0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, new Coordinate(0, 0), 1);
        }

        #endregion

        #region Test helpers
        protected enum ConnectionType
        {
            [System.ComponentModel.Description("DRL")] Orifice,
            [System.ComponentModel.Description("GSL")] ClosedConnection /*Should be created as a pipe*/,
            [System.ComponentModel.Description("ITR")] InfiltrationPipe /*Should be created as a pipe*/,
            [System.ComponentModel.Description("OPL")] Open /*Should be created as a pipe*/,
            [System.ComponentModel.Description("OVS")] Crest,
            [System.ComponentModel.Description("PMP")] Pump
        }

        public enum StructureType
        {
            [System.ComponentModel.Description("DRL")] Orifice,
            [System.ComponentModel.Description("OVS")] Crest,
            [System.ComponentModel.Description("UIT")] Outlet,
            [System.ComponentModel.Description("PMP")] Pump
        }

        public enum FlowDirection /*Field STR_RCH*/
        {
            [System.ComponentModel.Description("GSL")] Closed,
            [System.ComponentModel.Description("OPN")] Open,
            [System.ComponentModel.Description("1_2")] FromStartToEnd,
            [System.ComponentModel.Description("2_1")] FromEndToStart,
        }

        private static GwswAttribute GetDefaultGwswAttribute(string attributeName, string attributeValue, string attributeType = null)
        {
            if (attributeValue == null)
                attributeValue = string.Empty;

            return new GwswAttribute()
            {
                GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", attributeType ?? "string", attributeName,
                    "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                ValueAsString = attributeValue
            };
        }

        private static GwswElement GetSewerConnectionGwswElement(string uniqueId, string startNode, string endNode, string sewerConnectionTypeString , double startLevel, double endLevel, string flowDirectionString, double length,
            string crossSectionDef, string pipeIndicator, string sewerConnectionWaterType, double inletLossStart, double inletLossEnd, double outletLossStart, double outletLossEnd)
        {
            var typeString = "string";
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Connection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>()
                    {
                        GetDefaultGwswAttribute(_uniqueId, uniqueId),
                        GetDefaultGwswAttribute(_nodeUniqueIdStart, startNode),
                        GetDefaultGwswAttribute(_nodeUniqueIdEnd,endNode),
                        GetDefaultGwswAttribute(_pipeType,sewerConnectionTypeString),
                        GetDefaultGwswAttribute(_levelStart, startLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_levelEnd, endLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_length, length.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_flowDirection, flowDirectionString),
                        GetDefaultGwswAttribute(_crossSectionDef, crossSectionDef),
                        GetDefaultGwswAttribute(_pipeIndicator, pipeIndicator),
                        GetDefaultGwswAttribute(_waterType, sewerConnectionWaterType),
                        GetDefaultGwswAttribute(_inletlossStart, inletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_inletlossEnd, inletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_outletlossStart, outletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_outletlossEnd, outletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);
            
            return nodeGwswElement;
        }

        private static GwswElement GetStructureGwswElement(string uniqueId, string structureType, double pumpCapacity, double startLevelDownstreams, double stopLevelDownstreams, double startLevelUpstreams, double stopLevelUpstreams)
        {
            var typeString = "string";
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Connection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>()
                    {
                        GetDefaultGwswAttribute(_uniqueId, uniqueId),
                        GetDefaultGwswAttribute(_structureType, structureType),
                        GetDefaultGwswAttribute(_pumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_startLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_stopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_startLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(_stopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }
        private static void CheckManholeNodePropertyValues(Manhole manhole, string manholeId, double xCoordinate, double yCoordinate, int numberOfCompartments)
        {
            Assert.That(manhole.Name, Is.EqualTo(manholeId));
            Assert.That(manhole.XCoordinate, Is.EqualTo(xCoordinate));
            Assert.That(manhole.YCoordinate, Is.EqualTo(yCoordinate));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(xCoordinate, yCoordinate)));
            Assert.NotNull(manhole.Compartments);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(numberOfCompartments));
        }

        private void CheckCompartmentPropertyValues(Compartment compartment, string uniqueId, string manholeId, double manholeLength, double manholeWidth, CompartmentShape shape, double floodableArea, double bottomLevel, double surfaceLevel, Coordinate coords, int numberOfParentManholeCompartments)
        {
            Assert.NotNull(compartment.ParentManhole);
            CheckManholeNodePropertyValues(compartment.ParentManhole, manholeId, coords?.X ?? 0.0, coords?.Y ?? 0.0, numberOfParentManholeCompartments);

            Assert.That(compartment.Name, Is.EqualTo(uniqueId));
            Assert.That(compartment.ManholeLength, Is.EqualTo(manholeLength));
            Assert.That(compartment.ManholeWidth, Is.EqualTo(manholeWidth));
            Assert.That(compartment.Shape, Is.EqualTo(shape));
            Assert.That(compartment.FloodableArea, Is.EqualTo(floodableArea));
            Assert.That(compartment.BottomLevel, Is.EqualTo(bottomLevel));
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(surfaceLevel));
            if (compartment.Geometry != null)
            {
                Assert.That(compartment.Geometry.Coordinates.Length, Is.EqualTo(1));
                Assert.That(compartment.Geometry.Coordinate, Is.EqualTo(coords));
            }
        }

        private static void TryCreateFeatureAndCheckForLogMessageAndCheckFeatureValidity(string manholeId, GwswElement badGwswElement, string compartmentId)
        {
            INetworkFeature feature = null;
            var expectedPartOfMessage = "Manhole with unique id '" + manholeId + "' is not imported.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = SewerFeatureFactory.CreateInstance(badGwswElement),
                expectedPartOfMessage);

            // Check compartment
            var compartment = feature as Compartment;
            Assert.NotNull(compartment);
            Assert.That(compartment.Name, Is.EqualTo(compartmentId));
            Assert.NotNull(compartment.ParentManhole);
            Assert.That(compartment.ParentManhole.Name, Is.EqualTo(manholeId));
        }

        private static void TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(GwswElement badGwswElement, string expectedPartOfMessage)
        {
            INetworkFeature feature = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = SewerFeatureFactory.CreateInstance(badGwswElement), expectedPartOfMessage);
            Assert.IsNull(feature);
        }

        private readonly GwswElement nodeGwswElement1 = new GwswElement
        {
            ElementTypeName = SewerFeatureType.Node.ToString(),
            GwswAttributeList =
            {
                new GwswAttribute
                {
                    ValueAsString = "put1",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", _uniqueId, "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "01001",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "MANHOLE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "400.00",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "X_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "50.00",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "Y_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "7071",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "integer", "NODE_LENGTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "7071",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "integer", "NODE_WIDTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "RND",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "NODE_SHAPE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "45,67",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "FLOODABLE_AREA", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "0.01",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "BOTTOM_LEVEL", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "2.75",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double", "SURFACE_LEVEL", "MyDescription", null, null)
                }
            }
        };

        private readonly GwswElement nodeGwswElement2 = new GwswElement
        {
            ElementTypeName = SewerFeatureType.Node.ToString(),
            GwswAttributeList =
            {
                new GwswAttribute
                {
                    ValueAsString = "put2",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", _uniqueId, "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "01001",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "MANHOLE_ID", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "400.20",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "X_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "50.20",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "Y_COORDINATE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "4561",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "integer", "NODE_LENGTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "5561",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "integer", "NODE_WIDTH", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "RHK",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "NODE_SHAPE", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "89,5",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "FLOODABLE_AREA", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "-0.45",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "BOTTOM_LEVEL", "MyDescription", null, null)
                },
                new GwswAttribute
                {
                    ValueAsString = "1.83",
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "double", "SURFACE_LEVEL", "MyDescription", null, null)
                }
            }
        };

        private static string _uniqueId = "UNIQUE_ID";
        // Sewer connection attributes.
        private static string _nodeUniqueIdStart = "NODE_UNIQUE_ID_START";
        private static string _nodeUniqueIdEnd = "NODE_UNIQUE_ID_END";
        private static string _pipeType = "PIPE_TYPE";
        private static string _pipeIndicator = "PIPE_INDICATOR";
        private static string _crossSectionDef = "CROSS_SECTION_DEF";
        private static string _levelStart = "LEVEL_START";
        private static string _levelEnd = "LEVEL_END";
        private static string _flowDirection = "FLOW_DIRECTION";
        private static string _length = "LENGTH";
        private static string _waterType = "WATER_TYPE";
        private static string _inletlossStart = "INLETLOSS_START";
        private static string _outletlossStart = "OUTLETLOSS_START";
        private static string _inletlossEnd = "INLETLOSS_END";
        private static string _outletlossEnd = "OUTLETLOSS_END";
        // Structure attributes
        private static string _structureType = "STRUCTURE_TYPE";
        private static string _pumpCapacity = "PUMP_CAPACITY";
        private static string _startLevelDownstreams = "START_LEVEL_DOWNSTREAMS";
        private static string _stopLevelDownstreams = "STOP_LEVEL_DOWNSTREAMS";
        private static string _startLevelUpstreams = "START_LEVEL_UPSTREAMS";
        private static string _stopLevelUpstreams = "STOP_LEVEL_UPSTREAMS";

        #endregion
    }
}