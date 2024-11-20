using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerFeatureFactoryTest : SewerFeatureFactoryTestHelper
    {
        [Test]
        public void SewerFeatureGetsAllHydrObjects()
        {
            var network = new HydroNetwork();
            try
            {
                network.AllHydroObjects.ToList();
                network.Nodes.Add(new Manhole("ManholeTest"));
                network.AllHydroObjects.ToList();
            }
            catch (Exception e)
            {
                Assert.Fail("Could not cast HydroOjbects: {0}", e.Message);
            }
        }

        [Test]
        public void SewerFeatureTypeCanBeRetrievedWithAStringValue()
        {
            Assert.IsFalse(Enum.TryParse("failValue", out SewerFeatureType _));
            Assert.IsTrue(Enum.TryParse(SewerFeatureType.Connection.ToString(), out SewerFeatureType _));
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
            var gwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };

            try
            {
                var sewerEntity = CreateSewerFeature<ISewerFeature>(gwswElement);
                Assert.IsNull(sewerEntity);
            }
            catch (Exception e)
            {
                Assert.Fail("There was a problem while instantiating. {0}", e.Message);
            }
        }
        
        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest)]
        [TestCase(SewerStructureMapping.StructureType.Orifice)]
        [TestCase(SewerStructureMapping.StructureType.Outlet)]
        [TestCase(SewerStructureMapping.StructureType.Pump)]
        public void SewerFeatureFactoryReturnsStructuresWhenGivingNameForStructure(SewerStructureMapping.StructureType structureType)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureType.GetDescription(), string.Empty)
                }
            };

            var createdElement = CreateSewerFeature<ISewerFeature>(structureGwswElement);
            Assert.IsNotNull(createdElement);
        }

        [Test]
        [TestCase(SewerStructureMapping.StructureType.Crest, false, true, true)] // Fails for now
        [TestCase(SewerStructureMapping.StructureType.Orifice, false, true, false)]
        [TestCase(SewerStructureMapping.StructureType.Outlet, true, false, false)]
        [TestCase(SewerStructureMapping.StructureType.Pump, false, true, true)] // Fails for now
        public void SewerFeatureFactoryCreatesStructureAndSewerConnectionIfNeitherExists(SewerStructureMapping.StructureType structureType, bool isNode, bool isBranch, bool isStructure)
        {
            var structureId = "structure123";
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, structureId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureType.GetDescription(), string.Empty)
                }
            };

            var createdSewerEntity = CreateSewerFeature<ISewerFeature>(structureGwswElement);
            Assert.IsNotNull(createdSewerEntity);

            var network = new HydroNetwork();
            createdSewerEntity.AddToHydroNetwork(network, null);
            if (isNode)
            {
                Assert.IsTrue(network.Nodes.Any());
                Assert.IsTrue(network.Manholes.Any(m => m.Name.Equals(structureId) || m.ContainsCompartmentWithName(structureId)));
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
        /*
        [Test]
        public void GivenThreeCompartmentGwswElements_WhenGeneratingCompartments_ThenManholesAreCreatedCorrectly()
        {
            //define a few compartiments with different manholes
            #region creating GwswElements
            const string manholeOneName = "manholeOne";
            const string manholeTwoName = "maholeTwo";
            const double defaultDoubleValue = 0.0;
            var defaultStringValue = string.Empty;

            //Node one
            const double nodeOneCoordX = 0.0;
            const double nodeOneCoordY = 10.0;
            const string compartmentOneName = "CompartmentOne";
            var nodeOne = GetNodeGwswElement(compartmentOneName, manholeOneName, defaultStringValue, nodeOneCoordX, nodeOneCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);

            //Node two
            const double nodeTwoCoordX = 10.0;
            const double nodeTwoCoordY = 20.0;
            const string compartmentTwoName = "CompartmentTwo";
            var nodeTwo = GetNodeGwswElement(compartmentTwoName, manholeOneName, defaultStringValue, nodeTwoCoordX, nodeTwoCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);

            //Node three
            const double nodeThreeCoordX = 10.0;
            const double nodeThreeCoordY = 20.0;
            const string compartmentThreeName = "CompartmentThree";
            var nodeThree = GetNodeGwswElement(compartmentThreeName, manholeTwoName, defaultStringValue, nodeTwoCoordX, nodeTwoCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);
            #endregion

            //generate all instances
            var network = new HydroNetwork();
            var gwswElements = new List<GwswElement> { nodeOne, nodeTwo, nodeThree };
            var generatedSewerFeatures = SewerFeatureFactory.CreateSewerEntities(gwswElements);
            generatedSewerFeatures.ForEach(sf => sf.AddToHydroNetwork(network, null));

            //check manholes and their geometries
            var manholeWithTwoCompartments = network.Manholes.FirstOrDefault(m => m.Name == manholeOneName) as Manhole;
            Assert.IsNotNull(manholeWithTwoCompartments);
            const double averageX = (nodeOneCoordX + nodeTwoCoordX) / 2;
            const double averageY = (nodeOneCoordY + nodeTwoCoordY) / 2;
            Assert.That(manholeWithTwoCompartments.XCoordinate, Is.EqualTo(averageX));
            Assert.That(manholeWithTwoCompartments.YCoordinate, Is.EqualTo(averageY));

            var manholeTwo = network.Manholes.FirstOrDefault(m => m.Name == manholeTwoName) as Manhole;
            Assert.IsNotNull(manholeTwo);
            Assert.That(manholeTwo.XCoordinate, Is.EqualTo(nodeThreeCoordX));
            Assert.That(manholeTwo.YCoordinate, Is.EqualTo(nodeThreeCoordY));
        }

        [Test]
        public void GivenThreeCompartmentGwswElements_WhenGeneratingCompartments_ThenThreeCompartmentsAreCreated()
        {
            //define a few compartiments with different manholes
            #region creating GwswElements
            const string manholeOneName = "manholeOne";
            const string manholeTwoName = "maholeTwo";
            const double defaultDoubleValue = 0.0;
            var defaultStringValue = string.Empty;

            //Node one
            const double nodeOneCoordX = 0.0;
            const double nodeOneCoordY = 10.0;
            const string nodeOneName = "nodeOne";
            var nodeOne = GetNodeGwswElement(nodeOneName, manholeOneName, defaultStringValue, nodeOneCoordX, nodeOneCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);

            //Node two
            const double nodeTwoCoordX = 10.0;
            const double nodeTwoCoordY = 20.0;
            const string nodeTwoName = "nodeTwo";
            var nodeTwo = GetNodeGwswElement(nodeTwoName, manholeOneName, defaultStringValue, nodeTwoCoordX, nodeTwoCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);

            //Node three
            const string nodeThreeName = "nodeThree";
            var nodeThree = GetNodeGwswElement(nodeThreeName, manholeTwoName, defaultStringValue, nodeTwoCoordX, nodeTwoCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);
            #endregion

            //generate all instances
            var gwswElements = new List<GwswElement> {nodeOne, nodeTwo, nodeThree};
            var generatedSewerFeatures = SewerFeatureFactory.CreateSewerEntities(gwswElements);
            Assert.IsNotEmpty(generatedSewerFeatures);

            var generatedCompartments = generatedSewerFeatures.OfType<Compartment>().Distinct().ToList(); 
            Assert.IsNotNull(generatedCompartments);
            Assert.That(generatedCompartments.Count, Is.EqualTo(3));

            //check compartiments exist
            Assert.IsTrue(generatedCompartments.Any(c => c.Name.Equals(nodeOneName)));
            Assert.IsTrue(generatedCompartments.Any(c => c.Name.Equals(nodeTwoName)));
            Assert.IsTrue(generatedCompartments.Any(c => c.Name.Equals(nodeThreeName)));
        }

        [Test]
        public void SewerFeatureFactoryGeneratesManholesWhenGivingABatchOfCompartimentsWithoutManholeId()
        {
            #region creating GwswElements
            //define two compartment gwsw elements
            var defaultDoubleValue = 0.0;
            var defaultStringValue = string.Empty;

            //Node one
            const double nodeOneCoordX = 0.0;
            const double nodeOneCoordY = 10.0;
            const string nodeOneName = "nodeOne";
            var nodeGwswElement1 = GetNodeGwswElement(nodeOneName, defaultStringValue, defaultStringValue, nodeOneCoordX, nodeOneCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);

            //Node two
            const double nodeTwoCoordX = 10.0;
            const double nodeTwoCoordY = 20.0;
            const string nodeTwoName = "nodeTwo";
            var nodeGwswElement2 = GetNodeGwswElement(nodeTwoName, defaultStringValue, defaultStringValue, nodeTwoCoordX, nodeTwoCoordY, defaultDoubleValue, defaultDoubleValue, defaultStringValue, defaultDoubleValue, defaultDoubleValue, defaultDoubleValue);
            #endregion

            //generate all instances
            var gwswElements = new List<GwswElement> { nodeGwswElement1, nodeGwswElement2 };
            var generatedSewerFeatures = SewerFeatureFactory.CreateSewerEntities(gwswElements);
            Assert.IsNotEmpty(generatedSewerFeatures);

            var generatedCompartments = generatedSewerFeatures.OfType<Compartment>().ToList();
            Assert.IsNotEmpty(generatedCompartments);
            Assert.That(generatedCompartments.Count, Is.EqualTo(gwswElements.Count));

            //check compartiments exist
            Assert.IsTrue(generatedCompartments.Any(c => c.Name.Equals(nodeOneName)));
            Assert.IsTrue(generatedCompartments.Any(c => c.Name.Equals(nodeTwoName)));
        }

        [Test]
        public void GivenNodeAndOutletGwswElement_WhenGeneratingOutlet_ThenSurfaceWaterLevelOfOutletCompartmentHasBeenAssigned()
        {
            #region create outlet gwsw element
            const string compartmentName1 = "cmp1";
            const double surfaceWaterLevel = 15.0;
            var structureType = SewerStructureMapping.StructureType.Outlet.GetDescription();

            const double defaultDouble = 0.0;
            var outletGwswElement = GetStructureGwswElement(compartmentName1, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);
            #endregion

            #region create node gwsw element
            const double manholeLength = 7.0;
            const double manholeWidth = 7.0;
            const CompartmentShape compartmentShape = CompartmentShape.Square;
            const double floodableArea = 45.67;
            const double bottomLevel = 0.01;
            const double surfaceLevel = 2.75;

            var network = TestNetworkAndDiscretisationProvider.CreateSimpleSewerNetwork("myPipe");
            var pipe = Enumerable.FirstOrDefault<IPipe>(network.Pipes);
            Assert.IsNotNull(pipe);
            var sourceCompartment = (Compartment) pipe.SourceCompartment;
            sourceCompartment.ManholeLength = manholeLength;
            sourceCompartment.ManholeWidth = manholeWidth;
            sourceCompartment.Shape = compartmentShape;
            sourceCompartment.BottomLevel = bottomLevel;
            sourceCompartment.SurfaceLevel = surfaceLevel;
            sourceCompartment.FloodableArea = floodableArea;
            var sourceCompartmentGeometry = sourceCompartment.Geometry;

            #endregion

            var outletCompartment = CreateSewerFeature<OutletCompartment>(outletGwswElement);
            Assert.IsNotNull(outletCompartment);
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));
        }
        */
        [Test]
        public void GivenNodeAndOutletGwswElement_WhenGeneratingOutletAndAddingToNetwork_ThenExistingCompartmentIsReplacedByOutletCompartment()
        {
            #region create outlet gwsw element
            const string compartmentName1 = "cmp1";
            const double surfaceWaterLevel = 15.0;
            var structureType = SewerStructureMapping.StructureType.Outlet.GetDescription();

            const double defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(compartmentName1, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);
            #endregion

            #region create network
            const double manholeLength = 7.0;
            const double manholeWidth = 7.0;
            const CompartmentShape compartmentShape = CompartmentShape.Round;
            const double floodableArea = 45.67;
            const double bottomLevel = 0.01;
            const double surfaceLevel = 2.75;

            var network = TestNetworkAndDiscretisationProvider.CreateSimpleSewerNetwork("myPipe");
            var pipe = Enumerable.FirstOrDefault<IPipe>(network.Pipes);
            Assert.IsNotNull(pipe);
            var sourceCompartment = (Compartment) pipe.SourceCompartment;
            sourceCompartment.ManholeLength = manholeLength;
            sourceCompartment.ManholeWidth = manholeWidth;
            sourceCompartment.Shape = compartmentShape;
            sourceCompartment.BottomLevel = bottomLevel;
            sourceCompartment.SurfaceLevel = surfaceLevel;
            sourceCompartment.FloodableArea = floodableArea;
            var sourceCompartmentGeometry = sourceCompartment.Geometry;

            #endregion

            var outletCompartment = CreateSewerFeature<GwswStructureOutletCompartment>(structureGwswElement);
            outletCompartment.AddToHydroNetwork(network, null);

            // Check new outlet compartment properties
            Assert.IsNotNull(outletCompartment);
            Assert.That(outletCompartment.ManholeLength, Is.EqualTo(manholeLength));
            Assert.That(outletCompartment.ManholeWidth, Is.EqualTo(manholeWidth));
            Assert.That(outletCompartment.Shape, Is.EqualTo(compartmentShape));
            Assert.That(outletCompartment.BottomLevel, Is.EqualTo(bottomLevel));
            Assert.That(outletCompartment.SurfaceLevel, Is.EqualTo(surfaceLevel));
            Assert.That(outletCompartment.FloodableArea, Is.EqualTo(floodableArea));
            Assert.That((object) outletCompartment.Geometry.Coordinate.X, Is.EqualTo(sourceCompartmentGeometry.Coordinate.X));
            Assert.That((object) outletCompartment.Geometry.Coordinate.Y, Is.EqualTo(sourceCompartmentGeometry.Coordinate.Y));
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));

            // Add the outlet sewer entity to the network
            var networkSourceCompartment = Enumerable.FirstOrDefault<IPipe>(network.Pipes)?.SourceCompartment as OutletCompartment;
            Assert.IsNotNull(networkSourceCompartment);
        }

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
            var pipeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeId, isPipe ? pipeId : string.Empty, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };

            var createdPipe = CreateSewerFeature<Pipe>(pipeGwswElement);
            if (isPipe)
            {
                Assert.IsNotNull(createdPipe);
                Assert.That(createdPipe.PipeId, Is.EqualTo(pipeId));
            }
        }
    }
}