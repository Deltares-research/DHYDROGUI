using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerFeatureFactoryTest: SewerFeatureFactoryTestHelper
    {
        [Test]
        public void SewerFeatureFactory_AddStructureToBranch()
        {
            //SewerFeatureFactory.AddStructureToBranch();
        }

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
        public void SewerFeatureFactoryDoesNotGenerateStructuresAndLogsErrorWithoutNetwork()
        {
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Pump), string.Empty)
                }
            };
            var expectedMsg = String.Format(Resources
                .SewerPumpGenerator_CreatePumpFromGwswStructure_Pump_s__cannot_be_created_without_a_network_previously_defined_);

            var createdElement = new Branch() as INetworkFeature;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement), expectedMsg);
            Assert.IsNull(createdElement);
        }

        [Test]
        public void SewerFeatureFactoryReturnsNullStructuresWhenNotGivingNameForStructure()
        {
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Pump), string.Empty)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNull(createdElement);
        }

        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest)]
        [TestCase(SewerStructureMapping.StructureType.Orifice)]
        [TestCase(SewerStructureMapping.StructureType.Outlet)]
        [TestCase(SewerStructureMapping.StructureType.Pump)]
        public void SewerFeatureFactoryReturnsStructuresWhenGivingNameForStructure(SewerStructureMapping.StructureType structureType)
        {
            var structureId = "structure123";
            var structurePumpGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType), string.Empty)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structurePumpGwswElement, network);
            Assert.IsNotNull(createdElement);
        }

        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest, false, true, true)]
        [TestCase(SewerStructureMapping.StructureType.Orifice, false, true, false)]
        [TestCase(SewerStructureMapping.StructureType.Outlet, true, false, false)]
        [TestCase(SewerStructureMapping.StructureType.Pump, false, true, true)]
        public void SewerFeatureFactoryCreatesStructureAndSewerConnectionIfNeitherExists(SewerStructureMapping.StructureType structureType, bool isNode, bool isBranch, bool isStructure)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(structureType), string.Empty)
                }
            };

            var network = new HydroNetwork();
            var createdElement = SewerFeatureFactory.CreateInstance(structureGwswElement, network);
            Assert.IsNotNull(createdElement);
            
            if (isNode)
            {
                Assert.IsTrue(network.Nodes.Any());
                Assert.IsTrue(network.Manholes.Any(m => m.Name.Equals(structureId) || m.ContainsCompartment(structureId)));
            }

            if (isBranch)
            {
                Assert.IsTrue(network.SewerConnections.Any());
                Assert.IsTrue(network.SewerConnections.Any(s => s.Name.Equals(structureId)));
            }

            if (isStructure)
            {
                Assert.IsTrue(network.Structures.Any());
                Assert.IsTrue(network.Structures.Any(s => s.Name.Equals(structureId)));

                Assert.IsTrue(network.CompositeBranchStructures.Any());
                Assert.IsTrue(network.CompositeBranchStructures.Any(cb => cb.Structures.Any(s => s.Name.Equals(structureId))));
            }
        }

        [Test]
        public void SewerFeatureFactoryGeneratesManholesWhenGivingABatchOfCompartiments()
        {
            //define a few compartiments with different manholes
            #region creating GwswElements
            var manholeOne = "manholeOne";
            var manholeTwo = "maholeTwo";
            var dbl = 0.0;
            var str = string.Empty;
            //Node one
            var nodeOneCoordX = 0.0;
            var nodeOneCoordY = 10.0;
            var nodeOneName = "nodeOne";
            var nodeOne = GetNodeGwswElement(nodeOneName, manholeOne, str, nodeOneCoordX, nodeOneCoordY, dbl, dbl, str, dbl, dbl, dbl);
            //Node two
            var nodeTwoCoordX = 10.0;
            var nodeTwoCoordY = 20.0;
            var nodeTwoName = "nodeTwo";
            var nodeTwo = GetNodeGwswElement(nodeTwoName, manholeOne, str, nodeTwoCoordX, nodeTwoCoordY, dbl, dbl, str, dbl, dbl, dbl);
            //Node three
            var nodeThreeCoordX = 10.0;
            var nodeThreeCoordY = 20.0;
            var nodeThreeName = "nodeThree";
            var nodeThree = GetNodeGwswElement(nodeThreeName, manholeTwo, str, nodeTwoCoordX, nodeTwoCoordY, dbl, dbl, str, dbl, dbl, dbl);
            #endregion
            //generate all instances
            var listOfElements = new List<GwswElement> {nodeOne, nodeTwo, nodeThree};
            var features = SewerFeatureFactory.CreateMultipleInstances(listOfElements, null);
            Assert.IsNotNull(features);

            var listOfManholes = features.OfType<Manhole>().ToList();
            Assert.IsNotNull(listOfManholes);
            Assert.IsTrue(listOfManholes.Any());
            Assert.AreEqual(listOfElements.Count, listOfManholes.Count);

            //check compartiments exist
            var compartimentList = listOfManholes.SelectMany(m => m.Compartments).ToList();
            Assert.IsTrue(compartimentList.Any( c => c.Name.Equals(nodeOneName)));
            Assert.IsTrue(compartimentList.Any(c => c.Name.Equals(nodeTwoName)));
            Assert.IsTrue(compartimentList.Any(c => c.Name.Equals(nodeThreeName)));

            //check manholes and their geometries
            Assert.IsTrue(listOfManholes.Any( m => m.Name.Equals(manholeOne)));
            var mOne = listOfManholes.First(m => m.Name.Equals(manholeOne));
            var avgX = (nodeOneCoordX + nodeTwoCoordX) / 2;
            var avgY = (nodeOneCoordY + nodeTwoCoordY) / 2;
            Assert.AreEqual(avgX, mOne.XCoordinate);
            Assert.AreEqual(avgY, mOne.YCoordinate);

            Assert.IsTrue(listOfManholes.Any(m => m.Name.Equals(manholeTwo)));
            var mTwo = listOfManholes.First(m => m.Name.Equals(manholeTwo));
            Assert.AreEqual(nodeThreeCoordX, mTwo.XCoordinate);
            Assert.AreEqual(nodeThreeCoordY, mTwo.YCoordinate);
        }

        [Test]
        public void SewerFeatureFactoryGeneratesManholesWhenGivingABatchOfCompartimentsWithoutManholeId()
        {
            //define a few compartiments with different manholes
            #region creating GwswElements
            var dbl = 0.0;
            var str = string.Empty;
            //Node one
            var nodeOneCoordX = 0.0;
            var nodeOneCoordY = 10.0;
            var nodeOneName = "nodeOne";
            var nodeOne = GetNodeGwswElement(nodeOneName, str, str, nodeOneCoordX, nodeOneCoordY, dbl, dbl, str, dbl, dbl, dbl);
            //Node two
            var nodeTwoCoordX = 10.0;
            var nodeTwoCoordY = 20.0;
            var nodeTwoName = "nodeTwo";
            var nodeTwo = GetNodeGwswElement(nodeTwoName, str, str, nodeTwoCoordX, nodeTwoCoordY, dbl, dbl, str, dbl, dbl, dbl);
            #endregion
            //generate all instances
            var listOfElements = new List<GwswElement> { nodeOne, nodeTwo };
            var features = SewerFeatureFactory.CreateMultipleInstances(listOfElements, null);
            Assert.IsNotNull(features);

            var listOfManholes = features.OfType<Manhole>().ToList();
            Assert.IsNotNull(listOfManholes);
            Assert.IsTrue(listOfManholes.Any());
            Assert.AreEqual(listOfElements.Count, listOfManholes.Count);

            //check compartiments exist
            var compartimentList = listOfManholes.SelectMany(m => m.Compartments).ToList();
            Assert.IsTrue(compartimentList.Any(c => c.Name.Equals(nodeOneName)));
            Assert.IsTrue(compartimentList.Any(c => c.Name.Equals(nodeTwoName)));
        }
    }
}