using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Hydro.CrossSections;
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

        #region Connection

        [Test]
        public void PipeTypeCanBeRetrieveWithAStringValue()
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
                    new GwswAttribute
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 0, "columnName", "unkownType", "unkownCode", "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = "ValueShouldNotBeSet"
                    }
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));
        }

        [Test]
        [TestCase("GSL", SewerConnectionType.ClosedConnection, typeof(Pipe))]
        [TestCase("DRP", SewerConnectionType.Crest, typeof(SewerConnection))]
        [TestCase("ITR", SewerConnectionType.InfiltrationPipe, typeof(SewerConnection))]
        [TestCase("OPN", SewerConnectionType.Open, typeof(SewerConnection))]
        [TestCase("DRL", SewerConnectionType.Orifice, typeof(SewerConnection))]
        [TestCase("PMP", SewerConnectionType.Pump, typeof(SewerConnection))]
        public void CreateSewerConnectionMapsConnectionTypeFromFactory(string typeOfConnection, SewerConnectionType expectedConnectionType, Type expectedType)
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_TYPE"},
                        ValueAsString = typeOfConnection
        }
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            Assert.AreEqual(element.GetType(), expectedType);
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
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_TYPE"},
                        ValueAsString = typeOfConnection
                    }
                }
            };

            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement);
            var sewerConnection = element as SewerConnection;
            Assert.NotNull(sewerConnection);
            //Default value
            Assert.AreNotEqual(SewerConnectionType.ClosedConnection, sewerConnection.SewerConnectionType, "Expected sewer connection type has not been mapped correctly.");
        }

        [Test]
        [TestCase("GSL", SewerConnectionType.ClosedConnection, true)]
        [TestCase("DRP", SewerConnectionType.Crest, false)]
        [TestCase("ITR", SewerConnectionType.InfiltrationPipe, false)]
        [TestCase("OPN", SewerConnectionType.Open, false)]
        [TestCase("DRl", SewerConnectionType.Orifice, false)]
        [TestCase("PMP", SewerConnectionType.Pump, false)]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromFactory(string typeOfConnection, SewerConnectionType expectedConnectionType, bool isPipe)
        {
            var pipeId = "123";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_TYPE"},
                        ValueAsString = typeOfConnection
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_INDICATOR"},
                        ValueAsString = isPipe ? pipeId : string.Empty
                    }
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

            var nodeGwswElement = GetDefaultPipeGwswElement(pipeType, startLevel, endLevel, length, crossSectionDef, startNode, endNode);

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
        public void CreateSewerConnectionFromFactoryCreatesDefaultNodesIfTheyAreNotPresentInNetwork()
        {
            var startNode = "node001";
            var endNode = "node002";
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", "NODE_UNIQUE_ID_START",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startNode
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", "NODE_UNIQUE_ID_END",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endNode
                    },
                }
            };

            var network = new HydroNetwork();
            Assert.IsFalse(network.ManholeNodes.Any(n => n.Name.Equals(startNode)));
            Assert.IsFalse(network.ManholeNodes.Any(n => n.Name.Equals(endNode)));
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));

            var createdConnection = element as SewerConnection;
            Assert.IsNotNull(createdConnection);

            //Defined
            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(startNode)));
            Assert.IsNotNull(createdConnection.Source);

            var sourceAsManhole = createdConnection.Source as Manhole;
            Assert.IsNotNull(sourceAsManhole);
            Assert.AreEqual(startNode, sourceAsManhole.Name);

            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(endNode)));
            Assert.IsNotNull(createdConnection.Target);

            var targetAsManhole = createdConnection.Target as Manhole;
            Assert.IsNotNull(targetAsManhole);
            Assert.AreEqual(endNode, targetAsManhole.Name);

        }

        [Test]
        public void CreateSewerConnectionFromFactoryAssignsExistingNodesIfTheyArePresentInNetwork()
        {
            //Create network and nodes.
            var network = new HydroNetwork();

            var startNodeName = "node001";
            var startNode = new Manhole(startNodeName);
            network.ManholeNodes.Add(startNode);

            var endNodeName = "node002";
            var endNode = new Manhole(endNodeName);
            network.ManholeNodes.Add(endNode);

            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(startNodeName)));
            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(endNodeName)));

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
                        ValueAsString = startNodeName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", "NODE_UNIQUE_ID_END",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endNodeName
                    },
                }
            };
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement, network);
            Assert.That(element.GetType(), Is.EqualTo(typeof(SewerConnection)));

            var createdConnection = element as SewerConnection;
            Assert.IsNotNull(createdConnection);

            //Defined
            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(startNodeName)));
            Assert.IsNotNull(createdConnection.Source);
            Assert.AreEqual(startNode, createdConnection.Source as Manhole);

            Assert.IsTrue(network.ManholeNodes.Any(n => n.Name.Equals(endNodeName)));
            Assert.IsNotNull(createdConnection.Target);
            Assert.AreEqual(endNode, createdConnection.Target as Manhole);
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
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 4, "columnName", "string", "CROSS_SECTION_DEF",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = sewerDefinitionName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_TYPE"},
                        ValueAsString =EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.ClosedConnection)
                }
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
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 4, "columnName", "string", "CROSS_SECTION_DEF",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = sewerDefinitionName
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType{ Key = "PIPE_TYPE"},
                        ValueAsString =EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionType.ClosedConnection)
        }
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

        #region CompositeManholeNode

        [Test]
        public void GivenSimpleManholeData_WhenCreatingWithFactory_ThenManholeIsCorrectlyReturned()
        {
            var element = SewerFeatureFactory.CreateInstance(nodeGwswElement1);
            var manholeNode = element as Manhole;

            // Check manholeNode properties
            Assert.NotNull(manholeNode);
            CheckManholeNodePropertyValues(manholeNode, "01001", 400.0, 50.0, 1);

            // Check Manhole properties
            var compartment = manholeNode.Compartments.FirstOrDefault();
            CheckCompartmentPropertyValues(compartment, "put1", 7071, 7071, CompartmentShape.Square, 45.67, 0.01, 2.75, new Coordinate(400.0, 50.0));
        }

        [Test]
        public void GivenTwoCompartmentsOfTheSameLocation_WhenCreatingWithFactory_ThenManholeNodeIsCorrectlyReturned()
        {
            var elementList = new List<GwswElement> {nodeGwswElement1, nodeGwswElement2};
            var element = SewerFeatureFactory.CreateInstance(elementList);
            var manholeNode = element as Manhole;

            // Check manholeNode properties
            Assert.NotNull(manholeNode);
            CheckManholeNodePropertyValues(manholeNode, "01001", 400.1, 50.1, 2);

            // Check Manhole properties
            var compartment1 = manholeNode.Compartments[0];
            CheckCompartmentPropertyValues(compartment1, "put1", 7071, 7071, CompartmentShape.Square, 45.67, 0.01, 2.75, new Coordinate(400.0, 50.0));
            var compartment2 = manholeNode.Compartments[1];
            CheckCompartmentPropertyValues(compartment2, "put2", 4561, 5561, CompartmentShape.Rectangular, 89.5, -0.45, 1.83, new Coordinate(400.2, 50.2));
        }

        [Test]
        public void GivenGwswElementWithNotAllAttributesDefined_WhenCreatingManhole_ThenNoExceptionAndMissingPropertiesAreNotDefinedOrHaveDefaultValues()
        {
            var manholeNodeId = "01001";
            var manholeId = "put1";
            var gwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.UniqueId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeNodeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholePropertyKeys.ManholeId, "MyDescription", null, null)
                    }
                }
            };

            var manholeNode = SewerFeatureFactory.CreateInstance(gwswElement) as Manhole;
            Assert.NotNull(manholeNode);
            CheckManholeNodePropertyValues(manholeNode, manholeNodeId, 0, 0, 1);

            var manhole = manholeNode.Compartments.FirstOrDefault();
            CheckCompartmentPropertyValues(manhole, manholeId, 0, 0, CompartmentShape.Unknown, 0.0, 0.0, 0.0, null);
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

        #endregion

        #region Test helpers
        private static GwswElement GetDefaultPipeGwswElement(SewerConnectionType sewerConnectionType, int startLevel, int endLevel, int length,
            string crossSectionDef, string startNode, string endNode)
        {
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>()
                {
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 0, "columnName", "string", "PIPE_TYPE",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(sewerConnectionType)
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 1, "columnName", "double", "LEVEL_START",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startLevel.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 2, "columnName", "double", "LEVEL_END",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endLevel.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 3, "columnName", "double", "LENGTH",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = length.ToString()
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 4, "columnName", "string", "CROSS_SECTION_DEF",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = crossSectionDef
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", "NODE_UNIQUE_ID_START",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = startNode
                    },
                    new GwswAttribute()
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 6, "columnName", "string", "NODE_UNIQUE_ID_END",
                            "unkownDefinition", "mandatoryMaybe", "noRemarks"),
                        ValueAsString = endNode
                    },
                }
            };
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

        private void CheckCompartmentPropertyValues(Compartment compartment, string uniqueId, double manholeLength, double manholeWidth, CompartmentShape shape, double floodableArea, double bottomLevel, double surfaceLevel, Coordinate coords)
        {
            Assert.NotNull(compartment);
            Assert.That(compartment.Name, Is.EqualTo(uniqueId));
            Assert.That(compartment.ManholeLength, Is.EqualTo(manholeLength));
            Assert.That(compartment.ManholeWidth, Is.EqualTo(manholeWidth));
            Assert.That(compartment.Shape, Is.EqualTo(shape));
            Assert.That(compartment.FloodableArea, Is.EqualTo(floodableArea));
            Assert.That(compartment.BottomLevel, Is.EqualTo(bottomLevel));
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(surfaceLevel));
            Assert.That(compartment.Coordinates, Is.EqualTo(coords));
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