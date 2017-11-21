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
                var test = network.AllHydroObjects.ToList();
                network.Nodes.Add(new Manhole("ManholeTest"));
                test = network.AllHydroObjects.ToList();
            }
            catch (Exception e)
            {
                Assert.Fail("Could not cast HydroOjbects: {0}", e.Message);
            }
        }

        #region Sewer Connection

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
        public void SewerConnectionTypeCanBeRetrieveWithAStringValue()
        {
            SewerConnectionType testValue;
            Assert.IsFalse(Enum.TryParse("failValue", out testValue));
            Assert.IsTrue(Enum.TryParse(SewerConnectionType.ClosedConnection.ToString(), out testValue));
        }

        [Test]
        public void CreateSewerConnectionFromFactoryWithoutAttributes()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString()
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));
        }

        [Test]
        public void CreateSewerConnectionFromFactoryWithUnknownAttributes()
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("unkownCode", "ValueShouldNotBeSet", "unknownType")
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));
        }

        [Test]
        [TestCase("GSL", SewerConnectionType.ClosedConnection, typeof(Pipe))]
        [TestCase("OVS", SewerConnectionType.Crest, typeof(SewerConnection))]
        [TestCase("ITR", SewerConnectionType.InfiltrationPipe, typeof(Pipe))]
        [TestCase("OPL", SewerConnectionType.Open, typeof(Pipe))]
        [TestCase("DRL", SewerConnectionType.Orifice, typeof(SewerConnection))]
        [TestCase("PMP", SewerConnectionType.Pump, typeof(SewerConnection))]
        public void CreateSewerConnectionMapsConnectionTypeFromFactory(string typeOfConnection, SewerConnectionType expectedConnectionType, Type expectedType)
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("PIPE_TYPE", typeOfConnection)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.AreEqual(expectedType, element.GetType(), "Created Sewer Connection is not of the expected type.");
            var sewerConnection = element as SewerConnection;
            Assert.NotNull(sewerConnection);
            Assert.AreEqual(expectedConnectionType, sewerConnection.SewerConnectionType, "Expected sewer connection type has not been mapped correctly.");
        }

        [Test]
        public void CreateSewerConnectionWithUnknownMapConnectionTypeFromFactory()
        {
            var typeOfConnection = "NotKnown";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("PIPE_TYPE", typeOfConnection)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var sewerConnection = element as SewerConnection;
            Assert.NotNull(sewerConnection);
            //Default value
            Assert.AreNotEqual(SewerConnectionType.ClosedConnection, sewerConnection.SewerConnectionType, "Expected sewer connection type has not been mapped correctly.");
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
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_START", startNode),
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_END", endNode)
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
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", "NODE_UNIQUE_ID_START",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startCompartmentName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", "NODE_UNIQUE_ID_END",
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

            var connectionType = SewerConnectionType.Orifice;
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
                levelStart, levelEnd, length, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);
     
            var network = new HydroNetwork();

            var createdElement = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull( sewerConnection);

            Assert.AreEqual(objectId, sewerConnection.Name);
            Assert.AreEqual(connectionType, sewerConnection.SewerConnectionType);
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
                    GetDefaultGwswAttribute("PIPE_TYPE", EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.Orifice)),
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_START", startNode),
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_END", endNode)
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
        [TestCase("GSL", SewerConnectionType.ClosedConnection, true)]
        [TestCase("ITR", SewerConnectionType.InfiltrationPipe, true)]
        [TestCase("OPL", SewerConnectionType.Open, true)]
        [TestCase("OVS", SewerConnectionType.Crest, false)]
        [TestCase("DRL", SewerConnectionType.Orifice, false)]
        [TestCase("PMP", SewerConnectionType.Pump, false)]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromFactory(string typeOfConnection, SewerConnectionType expectedConnectionType, bool isPipe)
        {
            var pipeId = "123";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("PIPE_INDICATOR", isPipe ? pipeId : string.Empty),
                    GetDefaultGwswAttribute("PIPE_TYPE", typeOfConnection)
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
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
            var pipeType = SewerConnectionType.ClosedConnection;
            var pipeTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(pipeType);

            var defaultString = string.Empty;
            var defaultDouble = 0.0;

            var nodeGwswElement = GetSewerConnectionGwswElement(string.Empty, startNode, endNode, pipeTypeString, startLevel, endLevel, length, crossSectionDef, defaultString, defaultString, defaultDouble, defaultDouble,defaultDouble,defaultDouble );

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

            Assert.IsNotNull(createdPipe.SewerConnectionType);
            Assert.AreEqual(pipeType, createdPipe.SewerConnectionType);

            Assert.IsNotNull(createdPipe.Length);
            Assert.AreEqual(length, createdPipe.Length);
        }

        [Test]
        public void CreatePipeFromFactoryCreatesDefaultCrossSectionDefinitionIfNotPresentInNetwork()
        {
            var sewerDefinitionName = "crossSectionDef001";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("CROSS_SECTION_DEF", sewerDefinitionName),
                    GetDefaultGwswAttribute("PIPE_TYPE", EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.ClosedConnection))
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

            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("CROSS_SECTION_DEF", sewerDefinitionName),
                    GetDefaultGwswAttribute("PIPE_TYPE", EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.ClosedConnection))
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
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    GetDefaultGwswAttribute("PIPE_TYPE", EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.Pump))
                }
            };

            var network = new HydroNetwork();
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            
            //A sewer connection is created.
            var sewerConnection = element as SewerConnection;
            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(SewerConnectionType.Pump, sewerConnection.SewerConnectionType);

            //A Pump has been added to the above sewer connection.
            var featuresInConnection = sewerConnection.BranchFeatures;
            Assert.AreEqual(1, featuresInConnection.Count);

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
        public void CreatePumpFromGwswElementWithExpectedValues()
        {
            #region Setting expected values
            var connectionType = SewerConnectionType.Pump;
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(connectionType);

            var waterType = SewerConnectionWaterType.MixedWasteWater;
            var waterTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);
            var inletLossStart = 1.0;
            var inletLossEnd = 1.5;
            var outletLossStart = 2.0;
            var outletLossEnd = 2.5;
            #endregion
            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;

            var gwswElement = GetSewerConnectionGwswElement(nvgString, nvgString, nvgString, connectionTypeString,
                nvgDouble, nvgDouble, nvgDouble, nvgString, nvgString, waterTypeString, inletLossStart,
                inletLossEnd, outletLossStart, outletLossEnd);

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(gwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            var createdPump = sewerConnection.BranchFeatures.OfType<Pump>().FirstOrDefault();
            Assert.IsNotNull(createdPump);

            Assert.AreEqual(inletLossStart, createdPump.StartSuction);
            Assert.AreEqual(inletLossEnd, createdPump.StopSuction);
            Assert.AreEqual(outletLossStart, createdPump.StartDelivery);
            Assert.AreEqual(outletLossEnd, createdPump.StopDelivery);
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
                    GetDefaultGwswAttribute("PIPE_TYPE", EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.Pump)),
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_START", startNode),
                    GetDefaultGwswAttribute("NODE_UNIQUE_ID_END", endNode)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.IsNotNull(createdElement);

            var sewerConnection = createdElement as SewerConnection;
            Assert.IsNotNull(sewerConnection);

            var createdPump = sewerConnection.BranchFeatures.OfType<Pump>().FirstOrDefault();
            Assert.IsNotNull(createdPump);

            Assert.IsNotNull(createdPump.Geometry, "Default geometry not given to pump.");
            Assert.IsNotNull(createdPump.Geometry.Coordinates);
            Assert.IsTrue(createdPump.Geometry.Coordinates.Any());

        }
        #endregion

        #endregion

        #region CompositeManholeNode

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
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0, 0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, null, 1);
        }
        
        [Test]
        public void GivenGwswElementWithBadlyFormattedStringForShape_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned()
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
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "01001",
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

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholePropertyKeys.ManholeId)
                .Select(attr => attr.ValueAsString).FirstOrDefault();
            TryCreateNodeAndCheckForLogMessage(badGwswElement, "Manhole with unique id '" + manholeId + "' is not imported.");
        }

        [TestCase("01FA", ManholePropertyKeys.NodeWidth)]
        [TestCase("01FA", ManholePropertyKeys.NodeLength)]
        [TestCase("01FA", ManholePropertyKeys.FloodableArea)]
        [TestCase("01FA", ManholePropertyKeys.BottomLevel)]
        [TestCase("01FA", ManholePropertyKeys.SurfaceLevel)]
        public void GivenGwswElementWithBadlyFormattedStringForDoubleValue_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned(string badlyFormattedEntry, string keyValue)
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
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "01001",
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

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholePropertyKeys.ManholeId)
                .Select(attr => attr.ValueAsString).FirstOrDefault();
            TryCreateNodeAndCheckForLogMessage(badGwswElement, "Manhole with unique id '" + manholeId + "' is not imported.");
        }

        [TestCase("01FA", "23.6")]
        [TestCase("23.6", "01FA")]
        public void GivenGwswElementWithBadEntriesForCoordinateValues_WhenCreatingWithFactory_ThenLogMessageIsShownAndNullValueIsReturned(string xStringValue, string yStringValue)
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
                    },
                    new GwswAttribute
                    {
                        ValueAsString = "01001",
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

            var manholeId = badGwswElement.GwswAttributeList.Where(attr => attr.GwswAttributeType.Key == ManholePropertyKeys.ManholeId)
                .Select(attr => attr.ValueAsString).FirstOrDefault();
            TryCreateNodeAndCheckForLogMessage(badGwswElement, "Manhole with unique id '" + manholeId + "' is not imported.");
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
            TryCreateNodeAndCheckForLogMessage(badGwswElement, "Manhole with manhole id '" + manholeId + "' could not be created, because one of its compartments misses its unique id.");
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
            
            TryCreateNodeAndCheckForLogMessage(badGwswElement, "There are lines in 'Knooppunt.csv' that do not contain a Manhole Id. These lines are not imported.");
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
            CheckCompartmentPropertyValues(compartment, uniqueId, manholeId, 0.0, 0.0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, null, 1);
        }

        #endregion

        #region Test helpers

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

        private static GwswElement GetSewerConnectionGwswElement(string uniqueId, string startNode, string endNode, string sewerConnectionTypeString , double startLevel, double endLevel, double length,
            string crossSectionDef, string pipeIndicator, string sewerConnectionWaterType, double inletLossStart, double inletLossEnd, double outletLossStart, double outletLossEnd)
        {
            #region Setting Object Variables
            var UniqueId = "UNIQUE_ID";
            var NodeUniqueIdStart = "NODE_UNIQUE_ID_START";
            var NodeUniqueIdEnd = "NODE_UNIQUE_ID_END";
            var PipeType = "PIPE_TYPE";
            var LevelStart = "LEVEL_START";
            var LevelEnd = "LEVEL_END";
            var Length = "LENGTH";
            var CrossSectionDef = "CROSS_SECTION_DEF";
            var PipeIndicator = "PIPE_INDICATOR";
            var WaterType = "WATER_TYPE";
            var InletLossStart = "INLETLOSS_START";
            var OutletLossStart = "OUTLETLOSS_START";
            var InletLossEnd = "INLETLOSS_END";
            var OutletLossEnd = "OUTLETLOSS_END";
            #endregion

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
                        GetDefaultGwswAttribute(UniqueId, uniqueId),
                        GetDefaultGwswAttribute(NodeUniqueIdStart, startNode),
                        GetDefaultGwswAttribute(NodeUniqueIdEnd,endNode),
                        GetDefaultGwswAttribute(PipeType,sewerConnectionTypeString),
                        GetDefaultGwswAttribute(LevelStart, startLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(LevelEnd, endLevel.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(Length, length.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(CrossSectionDef, crossSectionDef),
                        GetDefaultGwswAttribute(PipeIndicator, pipeIndicator),
                        GetDefaultGwswAttribute(WaterType, sewerConnectionWaterType),
                        GetDefaultGwswAttribute(InletLossStart, inletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(InletLossEnd, inletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(OutletLossStart, outletLossStart.ToString(CultureInfo.InvariantCulture), typeDouble),
                        GetDefaultGwswAttribute(OutletLossEnd, outletLossEnd.ToString(CultureInfo.InvariantCulture), typeDouble),
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

        private static void TryCreateNodeAndCheckForLogMessage(GwswElement badGwswElement, string expectedPartOfMessage)
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
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string", "UNIQUE_ID", "MyDescription", null, null)
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
                    GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 3, "MyColumnName", "string", "UNIQUE_ID", "MyDescription", null, null)
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
        
        #endregion
    }
}