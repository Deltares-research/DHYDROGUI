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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, sourceNode),
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, targetNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart,
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startCompartmentName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd,
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeIndicator, isPipe ? pipeId : string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
            var pipeType = SewerConnectionMapping.ConnectionType.ClosedConnection;
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, sewerDefinitionName),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.ClosedConnection)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, sewerDefinitionName),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.ClosedConnection)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
        [TestCase(StructureMapping.StructureType.Crest, false)]
        [TestCase(StructureMapping.StructureType.Orifice, false)]
        [TestCase(StructureMapping.StructureType.Outlet, true)]
        [TestCase(StructureMapping.StructureType.Pump, true)]
        public void SewerFeatureFactoryReturnsStructuresWhenGivingNameForStructure(StructureMapping.StructureType structureType, bool mapped)
        {
            var structureId = "structure123";
            var structurePumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.UniqueId, structureId),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
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
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Pump)),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNull(createdElement);
        }

        [Test]
        [TestCase(StructureMapping.StructureType.Crest, false)]
        [TestCase(StructureMapping.StructureType.Orifice, false)]
        /*[TestCase(StructureType.Outlet, true)] It's a type of compartment */
        [TestCase(StructureMapping.StructureType.Pump, true)]
        public void SewerFeatureFactoryCreatesStructureAndSewerConnectionIfNeitherExists(StructureMapping.StructureType structureType, bool mapped)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.UniqueId, structureId),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType)),
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
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.UniqueId, structureId),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Pump)),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                }
            };

            var startNode = "node001";
            var endNode = "node002";
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump)),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd, endNode)
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
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.UniqueId, pumpId),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Pump)),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
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
            Assert.AreEqual(startLevelUpstreams, createdPump.StartDelivery);
            Assert.AreEqual(stopLevelUpstreams, createdPump.StopDelivery);
            Assert.AreEqual(startLevelDownstreams, createdPump.StartSuction);
            Assert.AreEqual(stopLevelDownstreams, createdPump.StopSuction);

        }
        #endregion

        #endregion

        #region Manhole

        [Test]
        public void GivenSimpleManholeData_WhenCreatingWithFactory_ThenManholeIsCorrectlyReturned()
        {
            #region expectedVariables
            var uniqueId = "put1";
            var manholeId = "01001";
            var manholeLength = 7071;
            var manholeWidth = 7071;
            var compartmentShape = CompartmentShape.Square;
            var compartmentShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(compartmentShape);
            var nodeType = string.Empty;
            var floodableArea = 45.67;
            var bottomLevel = 0.01;
            var surfaceLevel = 2.75;
            var xCoordinate = 400.0;
            var yCoordinate = 50.0;
            var numberOfParentManholeCompartments = 1;
            #endregion

            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, nodeType, xCoordinate, yCoordinate, manholeLength, manholeWidth, compartmentShapeAsString, floodableArea, bottomLevel, surfaceLevel);
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var compartment = element as Compartment;
            
            // Check Compartment properties
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, manholeLength, manholeWidth, compartmentShape, floodableArea, bottomLevel, surfaceLevel, new Coordinate(xCoordinate, yCoordinate), numberOfParentManholeCompartments);
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "UnkownValue",
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.NodeShape, "MyDescription", null, null)
                    }
                }
            };

            TryCreateFeatureAndCheckForLogMessageAndCheckFeatureValidity(manholeId, badGwswElement, compartmentId);
        }

        [TestCase("01FA", ManholeMapping.PropertyKeys.NodeWidth)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.NodeLength)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.FloodableArea)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.BottomLevel)]
        [TestCase("01FA", ManholeMapping.PropertyKeys.SurfaceLevel)]
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = xStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholeMapping.PropertyKeys.XCoordinate, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = yStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholeMapping.PropertyKeys.YCoordinate, "MyDescription", null, null)
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
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholeMapping.PropertyKeys.ManholeId)
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    }
                }
            };
            
            TryCreateFeatureAndCheckForLogMessageAndFeatureIsNull(badGwswElement, "There are lines in 'Knooppunt.csv' that do not contain a Manhole Id. These lines are not imported.");
        }

        [TestCase(ManholeMapping.PropertyKeys.NodeLength)]
        [TestCase(ManholeMapping.PropertyKeys.NodeWidth)]
        [TestCase(ManholeMapping.PropertyKeys.FloodableArea)]
        [TestCase(ManholeMapping.PropertyKeys.BottomLevel)]
        [TestCase(ManholeMapping.PropertyKeys.SurfaceLevel)]
        [TestCase(ManholeMapping.PropertyKeys.NodeShape)]
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
                            ManholeMapping.PropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = string.Empty,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            manholePropertyKey, "MyDescription", null, null)
                    }
                }
            };

            var compartment = SewerFeatureFactory.CreateInstance(gwswElement) as Compartment;
            Assert.NotNull(compartment);
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0.0, 0.0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, new Coordinate(0, 0), 1);
        }

        #region Outlets

        [Test]
        public void CreateOutletCompartmentFromGwswElementNodeType()
        {
            var uniqueId = "outlet123";
            var manholeId = "man123";
            var typeDouble = "double";
            var xCoord = 30.0;
            var yCoord = 15.0;
            var nodeLength = 14.0;
            var nodeWidth = 13.0;
            var nodeShape = CompartmentShape.Square;
            var nodeShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(nodeShape);
            var floodableArea = 11.0;
            var bottomLevel = 10.0;
            var surfaceLevel = 5.0;
            var nodeType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);
            CheckCompartmentPropertyValues(createdElement as Compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, new Coordinate(xCoord, yCoord), 1);
            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementNodeTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var manholeId = "man123";
            var typeDouble = "double";
            var xCoord = 30.0;
            var yCoord = 15.0;
            var nodeLength = 14.0;
            var nodeWidth = 13.0;
            var nodeShape = CompartmentShape.Square;
            var nodeShapeAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(nodeShape);
            var floodableArea = 11.0;
            var bottomLevel = 10.0;
            var surfaceLevel = 5.0;
            var nodeType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                }
            };

            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);
            CheckCompartmentPropertyValues(createdElement as Compartment, uniqueId, manholeId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel, new Coordinate(xCoord, yCoord), 1);
            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureType()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);
             
            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);

            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
            Assert.AreEqual(surfaceWaterLevel, outlet.SurfaceWaterLevel);
        }

        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement);
            Assert.NotNull(createdElement);

            //Check it can be casted into a compartment.
            Assert.NotNull(createdElement as Compartment);

            //Check it can be casted into an outlet.
            var outlet = createdElement as OutletCompartment;
            Assert.IsNotNull(outlet);
            Assert.AreEqual(surfaceWaterLevel, outlet.SurfaceWaterLevel);
        }

        [Test]
        public void CreateOutletFromGwswStructureThenCreateSameOutletFromGwswNodeShouldAddAttributesNotRemove()
        {
            var uniqueId = "outlet123";
            var manholeId = "manhole1";
            var surfaceWaterLevel = 15.0;
            var structureType =
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(StructureMapping.StructureType.Outlet);

            var defaultString = string.Empty;
            var defaultDouble = 0.0;
            //Gwsw elements
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);
            var nodeGwswElement = GetNodeGwswElement(uniqueId, manholeId, structureType, defaultDouble, defaultDouble, defaultDouble,
                defaultDouble, defaultString, defaultDouble, defaultDouble, defaultDouble);

            //Create structure element and add it to the network.
            var network = new HydroNetwork();
            var createdStructureElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.NotNull(createdStructureElement);
            
            //Check it can be casted into an outlet.
            var outletFromStructure = createdStructureElement as OutletCompartment;
            Assert.IsNotNull(outletFromStructure);
            Assert.AreEqual(surfaceWaterLevel, outletFromStructure.SurfaceWaterLevel);

            //Create node element and make sure it still has the surface water level value.
            var createdNodeElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.NotNull(createdNodeElement);

            //Check it can be casted into an outlet.
            var outletFromNode = createdNodeElement as OutletCompartment;
            Assert.IsNotNull(outletFromNode);
            Assert.AreEqual(surfaceWaterLevel, outletFromNode.SurfaceWaterLevel);
        }
        #endregion

        #endregion

        #region Test helpers
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

        #region Gwsw Elements

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

        private static GwswElement GetNodeGwswElement(string uniqueId, string manholeId, string nodeType, double xCoordinate, double yCoordinate, double nodeLength, double nodeWidth, string nodeShape, double floodableArea, double bottomLevel, double surfaceLevel)
        {
            var typeString = "string";
            var typeDouble = "double";
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Node.ToString(),
                    GwswAttributeList =
                    {
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoordinate.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoordinate.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShape),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;

            return nodeGwswElement;
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
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart, startNode),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd,endNode),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType,sewerConnectionTypeString),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelStart, startLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelEnd, endLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.Length, length.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionString),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDef, crossSectionDef),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeIndicator, pipeIndicator),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.WaterType, sewerConnectionWaterType),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossStart, inletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossEnd, inletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossStart, outletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossEnd, outletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
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

        private static GwswElement GetStructureGwswElement(string uniqueId, string structureType, double pumpCapacity, double startLevelDownstreams, double stopLevelDownstreams, double startLevelUpstreams, double stopLevelUpstreams, double surfaceWaterLevel)
        {
            var typeString = "string";
            var typeDouble = "double";

            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Structure.ToString(),
                    GwswAttributeList = new List<GwswAttribute>()
                    {
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.UniqueId, uniqueId),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StructureType, structureType),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(StructureMapping.PropertyKeys.SurfaceWaterLevel, surfaceWaterLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
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

        #endregion

        #endregion
    }
}