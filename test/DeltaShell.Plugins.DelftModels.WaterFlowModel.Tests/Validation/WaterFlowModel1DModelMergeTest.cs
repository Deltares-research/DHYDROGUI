using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.CoordinateSystems.Transformations;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DModelMergeTest
    {
        private WaterFlowModel1D destinationWFM1D;
        private WaterFlowModel1D sourceWFM1D;
        
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenNetworkOfDestinationModelIsExpanded()
        {
            Assert.That(destinationWFM1D.Network.Branches.Count, Is.EqualTo(1));
            Assert.That(destinationWFM1D.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(destinationWFM1D.Network.CrossSections.Count(), Is.EqualTo(1));

            Assert.That(sourceWFM1D.Network.Branches.Count, Is.EqualTo(1));
            Assert.That(sourceWFM1D.Network.Channels.Count(), Is.EqualTo(1));
            Assert.That(sourceWFM1D.Network.Nodes.Count, Is.EqualTo(2));
            Assert.That(sourceWFM1D.Network.CrossSections.Count(), Is.EqualTo(1));

            destinationWFM1D.Merge(sourceWFM1D, null);
            Assert.That(destinationWFM1D.Network.Branches.Count, Is.EqualTo(2));
            Assert.That(destinationWFM1D.Network.Channels.Count(), Is.EqualTo(2));
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.CrossSections.Count(), Is.EqualTo(2));
        }
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DWithNotCompleteEqualConnectingGeometryNodesAreMergedWhenMergedThenNetworkNodesAreMerged()
        {
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(2));
            
            var coordinateSystem = sourceWFM1D.Network.CoordinateSystem;
            var calc = new GeodeticDistance(coordinateSystem); 
            var sourceConnectingNode = sourceWFM1D.Network.Nodes.FirstOrDefault();
            Assert.That(sourceConnectingNode, Is.Not.Null);
            var nodeCoordinate = sourceConnectingNode.Geometry.Coordinate;
            double dist = 0.0d;
            Coordinate coordinateInRange = new Coordinate(nodeCoordinate);
            for (int i = 0; i < 15; i++)
            {
                coordinateInRange = new Coordinate(nodeCoordinate.X + i, nodeCoordinate.Y);
                dist = calc.Distance(nodeCoordinate, coordinateInRange);
                if (dist > 8 && dist <= 10) break;
            }
            Assert.That(dist, Is.GreaterThan(8).And.LessThanOrEqualTo(10), "Couldn't screw up model...something is wrong with the coordinatesystem");

            sourceConnectingNode.Geometry = new Point(coordinateInRange);
            Assert.That(sourceWFM1D.Network.Nodes.Count, Is.EqualTo(2));

            destinationWFM1D.Merge(sourceWFM1D, null);
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.Nodes.First(n => n.Name == "node2").Geometry.Coordinate.X, Is.EqualTo(nodeCoordinate.X) );
            Assert.That(destinationWFM1D.Network.Nodes.First(n => n.Name == "node2").Geometry.Coordinate.Y, Is.EqualTo(nodeCoordinate.Y) );
        }
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenNetworkNodeDuplicateNamesAreRenamed()
        {
            destinationWFM1D.Merge(sourceWFM1D, null);
            var node3 = destinationWFM1D.Network.Nodes.LastOrDefault();
            Assert.That(node3, Is.Not.Null);
            Assert.That(node3.Name, Is.EqualTo("Source0_node2"));
        }

        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenNetworkNodeBoundaryConditionAreSetToNoneOnCoupledNodes()
        {
            var bc_before = destinationWFM1D.BoundaryConditions.FirstOrDefault(bc => bc.Node.Name == "node2");
            Assert.That(bc_before, Is.Not.Null);
            Assert.That(bc_before.DataType, Is.EqualTo(WaterFlowModel1DBoundaryNodeDataType.FlowConstant));
            Assert.That(bc_before.Flow, Is.EqualTo(42));

            destinationWFM1D.Merge(sourceWFM1D, null);
            var bc_after = destinationWFM1D.BoundaryConditions.FirstOrDefault(bc => bc.Node.Name == "node2");
            Assert.That(bc_after, Is.Not.Null);
            Assert.That(bc_after.DataType, Is.EqualTo(WaterFlowModel1DBoundaryNodeDataType.None));
        }
        
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenNetworkBranchesDuplicateNamesAreRenamed()
        {
            destinationWFM1D.Merge(sourceWFM1D, null);
            var channel2 = destinationWFM1D.Network.Channels.LastOrDefault();
            Assert.That(channel2, Is.Not.Null);
            Assert.That(channel2.Name, Is.EqualTo("Source0_channel"));
        }
        [Test]
        public void GivenSourceWFM1DAndDestinationWFM1DAreMergedWhenMergedThenNetworkCrossSectionsDuplicateNamesAreRenamed()
        {
            destinationWFM1D.Merge(sourceWFM1D, null);
            var crossSection2 = destinationWFM1D.Network.CrossSections.LastOrDefault();
            Assert.That(crossSection2, Is.Not.Null);
            Assert.That(crossSection2.Name, Is.EqualTo("Source0_cross_section"));
        }

        [Test]
        public void TestMergeCopiesSourceModelBoundaryConditionsToDestinationModel()
        {
            var sourceBoundaryCondition = sourceWFM1D.BoundaryConditions.First(bc => bc.Node.Name == "node2");
            sourceBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            sourceBoundaryCondition.Flow = 5.555;

            destinationWFM1D.Merge(sourceWFM1D, null);

            var copiedBoundaryCondition = destinationWFM1D.BoundaryConditions.First(bc => bc.Node.Name == "Source0_node2");
            Assert.AreEqual(sourceBoundaryCondition.DataType, copiedBoundaryCondition.DataType, "SourceModel BoundaryCondition type was not copied to DestinationModel");
            Assert.AreEqual(sourceBoundaryCondition.Flow, copiedBoundaryCondition.Flow, "SourceModel BoundaryCondition value was not copied to DestinationModel");
        }

        [Test]
        public void TestMergeDoesNotCopySourceModelBoundaryConditionsOfTypeNoneToDestinationModel()
        {
            var sourceBoundaryCondition = sourceWFM1D.BoundaryConditions.First(bc => bc.Node.Name == "node2");
            sourceBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.None;
            
            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.IsFalse(destinationWFM1D.BoundaryConditions.Any(bc => bc.Node.Name == "Source0_node2"), "SourceModel BoundaryConditions of type 'None' should not be copied to the DestinationModel");
        }

        [Test]
        public void TestMergeCopiesSourceModelSharedCrossSectionsToDestinationModelAndRelinksSections()
        {
            var destinationModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 200);
            var destinationNetwork = destinationModel.Network;
            destinationNetwork.Name = "Network_DestinationModel";

            var sourceModel = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 95);
            var sourceNetwork = sourceModel.Network;
            sourceNetwork.Name = "Network_SourceModel";

            Assert.AreEqual(1, sourceNetwork.CrossSections.Count());
            Assert.AreEqual(1, destinationNetwork.CrossSections.Count());

            var sourceCrossSection = sourceNetwork.CrossSections.First();
            sourceCrossSection.ShareDefinitionAndChangeToProxy();

            var unusedSharedCrossSectionDefinition = (CrossSectionDefinition)sourceNetwork.SharedCrossSectionDefinitions.First().Clone();
            unusedSharedCrossSectionDefinition.Sections.Add(new CrossSectionSection() { SectionType = sourceNetwork.CrossSectionSectionTypes.First() });
            sourceNetwork.SharedCrossSectionDefinitions.Add(unusedSharedCrossSectionDefinition);

            Assert.AreEqual(0, destinationNetwork.SharedCrossSectionDefinitions.Count);
            Assert.AreEqual(1, destinationNetwork.CrossSectionSectionTypes.Count);

            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(destinationModel, sourceModel);
            WaterFlowModel1DModelMergeHelper.Merge(destinationModel, (WaterFlowModel1D)sourceModel.Clone());

            Assert.AreEqual(2, destinationNetwork.CrossSections.Count());
            Assert.AreEqual(2, destinationNetwork.SharedCrossSectionDefinitions.Count,
                "SharedCrossSection definition was not merged from source model to destination model");

            var unusedSharedCrossSectionDefinitionSection = destinationNetwork.SharedCrossSectionDefinitions.Last().Sections.Last();
            Assert.IsTrue(unusedSharedCrossSectionDefinitionSection.SectionType == destinationNetwork.CrossSectionSectionTypes.First(),
                "Merged SharedCrossSection definition section was not relinked to destination model SectionType");

            Assert.IsFalse(unusedSharedCrossSectionDefinitionSection.SectionType == sourceNetwork.CrossSectionSectionTypes.First(),
                "Merged SharedCrossSection definition section was not relinked to destination model SectionType");
        }

        [Test]
        public void TestMergeCopiesSourceModelNetworkDiscretizationToDestinationModel()
        {
            sourceWFM1D.NetworkDiscretization = WaterFlowModel1DModelMergeTestHelper.SetupUniformNetworkDiscretization(sourceWFM1D.Network, 5);
            destinationWFM1D.NetworkDiscretization = WaterFlowModel1DModelMergeTestHelper.SetupUniformNetworkDiscretization(destinationWFM1D.Network, 6);
            Assert.AreEqual(6, destinationWFM1D.NetworkDiscretization.Locations.Values.Count);

            destinationWFM1D.Merge(sourceWFM1D, null);
            
            Assert.AreEqual(11, destinationWFM1D.NetworkDiscretization.Locations.Values.Count);
            Assert.AreEqual("(Source0_channel, 75)", destinationWFM1D.NetworkDiscretization.Locations.Values[8].ToString(), "SourceModel NetworkDiscretization was not copied to DestinationModel");
        }

        [Test]
        public void TestMergeCopiesSourceModelLateralSourceDataToDestinationModel()
        {
            var sourceModelBranch = sourceWFM1D.Network.Branches[0];
            var lateralSource = new LateralSource(){ Branch = sourceModelBranch };
            sourceModelBranch.BranchFeatures.Add(lateralSource);
            
            var lateralSourceData = new WaterFlowModel1DLateralSourceData()
            {
                DataType = WaterFlowModel1DLateralDataType.FlowConstant, 
                Flow = 5.555, 
                Feature = lateralSource
            };
            sourceWFM1D.LateralSourceData.Add(lateralSourceData);

            Assert.AreEqual(0, destinationWFM1D.LateralSourceData.Count);
            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.AreEqual(1, destinationWFM1D.LateralSourceData.Count);
            Assert.AreEqual(lateralSourceData.Flow, destinationWFM1D.LateralSourceData[0].Flow, "SourceModel LateralSourceData value was not copied to DestinationModel");
            Assert.AreEqual(lateralSourceData.DataType, destinationWFM1D.LateralSourceData[0].DataType, "SourceModel LateralSourceData type was not copied to DestinationModel");
        }

        [Test]
        public void TestMergeCopiesSourceModelRoughnessCoveragesToDestinationModel()
        {
            var sourceMainRoughnessCoverage = sourceWFM1D.RoughnessSections[0].RoughnessNetworkCoverage;
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceMainRoughnessCoverage, 2);

            var sourceMainRoughnessValues = sourceMainRoughnessCoverage.Components[0].Values;
            sourceMainRoughnessValues[0] = 11.11;
            sourceMainRoughnessValues[1] = 22.22;
            
            var sourceMainRoughnessTypes = sourceMainRoughnessCoverage.Components[1].Values;
            sourceMainRoughnessTypes[0] = RoughnessType.Chezy;
            sourceMainRoughnessTypes[1] = RoughnessType.Manning;

            var floodPlain1 = new CrossSectionSectionType() {Name = "FloodPlain1"};
            sourceWFM1D.Network.CrossSectionSectionTypes.Add(floodPlain1);
            sourceWFM1D.RoughnessSections.Add(new RoughnessSection(floodPlain1, sourceWFM1D.Network) { Name = "FloodPlain1" });

            var sourceFloodPlain1RoughnessCoverage = sourceWFM1D.RoughnessSections[1].RoughnessNetworkCoverage;
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceFloodPlain1RoughnessCoverage, 1);

            var sourceFloodPlain1RoughnessValues = sourceFloodPlain1RoughnessCoverage.Components[0].Values;
            sourceFloodPlain1RoughnessValues[0] = 33.33;
            
            var sourceFloodPlain1RoughnessTypes = sourceFloodPlain1RoughnessCoverage.Components[1].Values;
            sourceFloodPlain1RoughnessTypes[0] = RoughnessType.StricklerKn;
            
            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            Assert.AreEqual(1, destinationWFM1D.Network.CrossSectionSectionTypes.Count);
            var destinationMainRoughnessCoverage = destinationWFM1D.RoughnessSections[0].RoughnessNetworkCoverage;
            Assert.AreEqual(0, destinationMainRoughnessCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(0, destinationMainRoughnessCoverage.Components[0].Values.Count);
            Assert.AreEqual(0, destinationMainRoughnessCoverage.Components[1].Values.Count);

            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.AreEqual(2, destinationWFM1D.RoughnessSections.Count);
            Assert.AreEqual(2, destinationWFM1D.Network.CrossSectionSectionTypes.Count);
            Assert.AreEqual(2, destinationMainRoughnessCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(2, destinationMainRoughnessCoverage.Components[0].Values.Count);
            Assert.AreEqual(2, destinationMainRoughnessCoverage.Components[1].Values.Count);

            var destinationFloodPlain1RoughnessCoverage = destinationWFM1D.RoughnessSections[1].RoughnessNetworkCoverage;
            Assert.AreEqual("(Source0_channel, 0)", destinationFloodPlain1RoughnessCoverage.Arguments[0].Values[0].ToString(), "SourceModel Roughness Location was not copied to DestinationModel");
            Assert.AreEqual(33.33, destinationFloodPlain1RoughnessCoverage.Components[0].Values[0], "SourceModel Roughness value was not copied to DestinationModel");
            Assert.AreEqual(RoughnessType.StricklerKn, (RoughnessType)destinationFloodPlain1RoughnessCoverage.Components[1].Values[0], "SourceModel Roughness type was not copied to DestinationModel");
        }
        
        [Test]
        public void TestMergeCopiesSourceModelRoughnessWaterDischargeCoveragesToDestinationModel()
        {
            var network = sourceWFM1D.Network;
            var branch1 = network.Branches.FirstOrDefault();
            Assert.That(branch1, Is.Not.Null);

            var mainRoughnessSection = sourceWFM1D.RoughnessSections.FirstOrDefault();
            Assert.That(mainRoughnessSection, Is.Not.Null);
            
            var dischargeFunctionBranch1Main = RoughnessSection.DefineFunctionOfQ();
            dischargeFunctionBranch1Main[0.0, 0.0] = 1.1;
            dischargeFunctionBranch1Main[0.0, 10000.0] = 2.1;

            dischargeFunctionBranch1Main[2500.0, 0.0] = 11.1;
            dischargeFunctionBranch1Main[2500.0, 10000.0] = 12.1;
            mainRoughnessSection.AddQRoughnessFunctionToBranch(branch1, dischargeFunctionBranch1Main);

            Assert.AreEqual(1, sourceWFM1D.RoughnessSections.Count);
            var functionsOfQSource = TypeUtils.GetPropertyValue(sourceWFM1D.RoughnessSections[0], "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQSource);
            Assert.AreEqual(1, functionsOfQSource.Count);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            var functionsOfQDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQDestination);
            Assert.AreEqual(0, functionsOfQDestination.Count);

            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            functionsOfQDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQDestination);
            Assert.AreEqual(1, functionsOfQDestination.Count);
        }

        [Test]
        public void TestMergeCopiesSourceModelRoughnessWaterLevelCoveragesToDestinationModel()
        {
            var network = sourceWFM1D.Network;
            var branch1 = network.Branches.FirstOrDefault();
            Assert.That(branch1, Is.Not.Null);
            
            var mainRoughnessSection = sourceWFM1D.RoughnessSections.FirstOrDefault();
            Assert.That(mainRoughnessSection, Is.Not.Null);
            
            var waterLevelFunctionBranch1Main = RoughnessSection.DefineFunctionOfH();
            waterLevelFunctionBranch1Main[0.0, 1.0] = 31.1;
            waterLevelFunctionBranch1Main[0.0, 4.0] = 41.1;

            waterLevelFunctionBranch1Main[3500.0, 1.0] = 111.1;
            waterLevelFunctionBranch1Main[3500.0, 4.0] = 112.1;

            mainRoughnessSection.AddHRoughnessFunctionToBranch(branch1, waterLevelFunctionBranch1Main);

            Assert.AreEqual(1, sourceWFM1D.RoughnessSections.Count);
            var functionsOfH = TypeUtils.GetPropertyValue(sourceWFM1D.RoughnessSections[0], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfH);
            Assert.AreEqual(1, functionsOfH.Count);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            var functionsOfHDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfHDestination);
            Assert.AreEqual(0, functionsOfHDestination.Count);
            
            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            functionsOfHDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfHDestination);
            Assert.AreEqual(1, functionsOfHDestination.Count);
        }

        [Test]
        public void TestMergeCopiesSourceModelExtraRoughnessWaterLevelCoveragesToDestinationModel()
        {
            var network = sourceWFM1D.Network;
            var branch1 = network.Branches.FirstOrDefault();
            Assert.That(branch1, Is.Not.Null);
            branch1.Name = "MySourceBranch";
            EventedList<CrossSectionSectionType> crossSectionSectionTypes = GetSectionTypesList(new[] { "Main", "FloodPlain1" });
            TypeUtils.SetPrivatePropertyValue(sourceWFM1D.Network, "CrossSectionSectionTypes", crossSectionSectionTypes);
            var floodPlain1CrossSectionSectionType = sourceWFM1D.Network.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name == "FloodPlain1");
            Assert.NotNull(floodPlain1CrossSectionSectionType);
            var floodPlain1RoughnessSection = new RoughnessSection(floodPlain1CrossSectionSectionType, network);

            var waterLevelFunctionBranch1FloodPlain1 = RoughnessSection.DefineFunctionOfH();
            waterLevelFunctionBranch1FloodPlain1[0.0, 1.0] = 31.1;
            waterLevelFunctionBranch1FloodPlain1[0.0, 4.0] = 41.1;

            waterLevelFunctionBranch1FloodPlain1[3500.0, 1.0] = 111.1;
            waterLevelFunctionBranch1FloodPlain1[3500.0, 4.0] = 112.1;

            floodPlain1RoughnessSection.AddHRoughnessFunctionToBranch(branch1, waterLevelFunctionBranch1FloodPlain1);
            sourceWFM1D.RoughnessSections.Add(floodPlain1RoughnessSection);
            
            Assert.AreEqual(2, sourceWFM1D.RoughnessSections.Count);
            var functionsOfH = TypeUtils.GetPropertyValue(sourceWFM1D.RoughnessSections[1], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfH);
            Assert.AreEqual(1, functionsOfH.Count);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            Assert.AreEqual(1, destinationWFM1D.Network.CrossSectionSectionTypes.Count);
            Assert.IsFalse(destinationWFM1D.Network.CrossSectionSectionTypes.Any(csst => csst.Name == "FloodPlain1"));

            destinationWFM1D.Merge(sourceWFM1D, null);

            Assert.AreEqual(2, destinationWFM1D.RoughnessSections.Count);
            Assert.AreEqual(2, destinationWFM1D.Network.CrossSectionSectionTypes.Count);
            Assert.IsTrue(destinationWFM1D.Network.CrossSectionSectionTypes.Any(csst => csst.Name == "FloodPlain1"));
            var destinationRoughnessSectionFloodPlain1 = destinationWFM1D.RoughnessSections.FirstOrDefault(rs => rs.Name == "FloodPlain1");
            Assert.NotNull(destinationRoughnessSectionFloodPlain1);
            var functionsOfHDestination = TypeUtils.GetPropertyValue(destinationRoughnessSectionFloodPlain1, "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfHDestination);
            Assert.AreEqual(1, functionsOfHDestination.Count);

            var sourceBranchInDestinationNetwork = destinationWFM1D.Network.Branches.FirstOrDefault(b => b.Name == "MySourceBranch");
            Assert.NotNull(sourceBranchInDestinationNetwork);
            var mergedFunctionH = destinationRoughnessSectionFloodPlain1.FunctionOfH(sourceBranchInDestinationNetwork);
            Assert.That(mergedFunctionH[0.0, 1.0], Is.EqualTo(31.1).Within(0.001));
            Assert.That(mergedFunctionH[0.0, 4.0], Is.EqualTo(41.1).Within(0.001));
            Assert.That(mergedFunctionH[3500.0, 1.0], Is.EqualTo(111.1).Within(0.001));
            Assert.That(mergedFunctionH[3500.0, 4.0], Is.EqualTo(112.1).Within(0.001));

        }

        private static EventedList<CrossSectionSectionType> GetSectionTypesList(IEnumerable<string> names)
        {
            return new EventedList<CrossSectionSectionType>(
                names
                    .Select(name => new CrossSectionSectionType
                    {
                        Name = name
                    }));
        }

        [Test]
        public void TestMergeCopiesSourceModelRoughnessFunctionsToDestinationModel()
        {
            var network = sourceWFM1D.Network;
            var branch1 = network.Branches.FirstOrDefault();
            Assert.That(branch1, Is.Not.Null);
            branch1.Name = "SourceBranch1";
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network, Geometry = new Point(new Coordinate(branch1.Target.Geometry.Coordinate.X + 100, 0)) };
           
            network.Nodes.Add(node3);
            
            var branch2 = new Channel("SourceBranch2", branch1.Target, node3);
            var vertices = new List<Coordinate>
            {
                new Coordinate(branch1.Target.Geometry.Coordinate.X, 0),
                new Coordinate(branch1.Target.Geometry.Coordinate.X + 100, 0)
            };
            branch2.Geometry = GeometryFactory.Default.CreateLineString(vertices.ToArray());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch2, CrossSectionDefinitionZW.CreateDefault(), 50);

            network.Branches.Add(branch2);
            
            // a H function 
            var mainRoughnessSection = sourceWFM1D.RoughnessSections.FirstOrDefault();
            Assert.That(mainRoughnessSection, Is.Not.Null);
            
            var waterLevelFunctionBranch1Main = RoughnessSection.DefineFunctionOfH();
            waterLevelFunctionBranch1Main[0.0, 1.0] = 31.1;
            waterLevelFunctionBranch1Main[0.0, 4.0] = 41.1;

            waterLevelFunctionBranch1Main[3500.0, 1.0] = 111.1;
            waterLevelFunctionBranch1Main[3500.0, 4.0] = 112.1;

            mainRoughnessSection.AddHRoughnessFunctionToBranch(branch1, waterLevelFunctionBranch1Main);
            
            // an other H function 
            
            var waterLevelFunctionBranch2Main = RoughnessSection.DefineFunctionOfH();
            waterLevelFunctionBranch2Main[0.0, 1.0] = 131.1;
            waterLevelFunctionBranch2Main[0.0, 4.0] = 141.1;

            waterLevelFunctionBranch2Main[3500.0, 1.0] = 1111.1;
            waterLevelFunctionBranch2Main[3500.0, 4.0] = 1112.1;
            mainRoughnessSection.AddHRoughnessFunctionToBranch(branch2, waterLevelFunctionBranch2Main);

            // a Q function 
            var dischargeFunctionBranch1Main = RoughnessSection.DefineFunctionOfQ();
            dischargeFunctionBranch1Main[0.0, 0.0] = 1.1;
            dischargeFunctionBranch1Main[0.0, 10000.0] = 2.1;

            dischargeFunctionBranch1Main[2500.0, 0.0] = 11.1;
            dischargeFunctionBranch1Main[2500.0, 10000.0] = 12.1;
            mainRoughnessSection.AddQRoughnessFunctionToBranch(branch1, dischargeFunctionBranch1Main);
            
            //premerge checks
            Assert.AreEqual(1, sourceWFM1D.RoughnessSections.Count);
            var functionsOfH = TypeUtils.GetPropertyValue(sourceWFM1D.RoughnessSections[0], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfH);
            Assert.AreEqual(2, functionsOfH.Count);

            var functionsOfQ = TypeUtils.GetPropertyValue(sourceWFM1D.RoughnessSections[0], "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQ);
            Assert.AreEqual(1, functionsOfQ.Count);

            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            var functionsOfHDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfHDestination);
            Assert.AreEqual(0, functionsOfHDestination.Count);
            
            var functionsOfQDestination = TypeUtils.GetPropertyValue(destinationWFM1D.RoughnessSections[0], "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQDestination);
            Assert.AreEqual(0, functionsOfQDestination.Count);
            
            // merge
            destinationWFM1D.Merge(sourceWFM1D, null);

            // check if merge is successfull
            Assert.AreEqual(1, destinationWFM1D.RoughnessSections.Count);
            var destinationRoughnessSectionMain = destinationWFM1D.RoughnessSections[0];
            functionsOfHDestination = TypeUtils.GetPropertyValue(destinationRoughnessSectionMain, "RoughnessFunctionOfH") as IList<IFunction>;
            Assert.NotNull(functionsOfHDestination);
            Assert.AreEqual(2, functionsOfHDestination.Count);

            functionsOfQDestination = TypeUtils.GetPropertyValue(destinationRoughnessSectionMain, "RoughnessFunctionOfQ") as IList<IFunction>;
            Assert.NotNull(functionsOfQDestination);
            Assert.AreEqual(1, functionsOfQDestination.Count);

            var destBranch1 = destinationWFM1D.Network.Branches.FirstOrDefault(b => b.Name == "SourceBranch1");
            Assert.NotNull(destBranch1);
            var destFuncHBranch1 = destinationRoughnessSectionMain.FunctionOfH(destBranch1);
            Assert.NotNull(destFuncHBranch1);
            Assert.That(destFuncHBranch1[0.0, 1.0], Is.EqualTo(31.1).Within(0.001));
            Assert.That(destFuncHBranch1[0.0, 4.0], Is.EqualTo(41.1).Within(0.001));
            Assert.That(destFuncHBranch1[3500.0, 1.0], Is.EqualTo(111.1).Within(0.001));
            Assert.That(destFuncHBranch1[3500.0, 4.0], Is.EqualTo(112.1).Within(0.001));
            
            var destFuncQBranch1 = destinationRoughnessSectionMain.FunctionOfQ(destBranch1);
            Assert.NotNull(destFuncQBranch1);
            Assert.That(destFuncQBranch1[0.0, 0.0], Is.EqualTo(1.1).Within(0.001));
            Assert.That(destFuncQBranch1[0.0, 10000.0], Is.EqualTo(2.1).Within(0.001));
            Assert.That(destFuncQBranch1[2500.0, 0.0], Is.EqualTo(11.1).Within(0.001));
            Assert.That(destFuncQBranch1[2500.0, 10000.0], Is.EqualTo(12.1).Within(0.001));
            
            var destBranch2 = destinationWFM1D.Network.Branches.FirstOrDefault(b => b.Name == "SourceBranch2");
            Assert.NotNull(destBranch2);
            var destFuncHBranch2 = destinationRoughnessSectionMain.FunctionOfH(destBranch2);
            Assert.NotNull(destFuncHBranch2);
            Assert.That(destFuncHBranch2[0.0, 1.0], Is.EqualTo(131.1).Within(0.001));
            Assert.That(destFuncHBranch2[0.0, 4.0], Is.EqualTo(141.1).Within(0.001));
            Assert.That(destFuncHBranch2[3500.0, 1.0], Is.EqualTo(1111.1).Within(0.001));
            Assert.That(destFuncHBranch2[3500.0, 4.0], Is.EqualTo(1112.1).Within(0.001));
        }

        [Test]
        public void TestMergeCopiesSourceModelInitialConditionCoveragesToDestinationModel()
        {
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.InitialConditions, 2);
            var sourceModelInitialConditionsValues = sourceWFM1D.InitialConditions.Components[0].Values;
            sourceModelInitialConditionsValues[0] = 5.55;
            sourceModelInitialConditionsValues[1] = 7.77;

            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.InitialFlow, 1);
            var sourceModelInitialFlowValues = sourceWFM1D.InitialFlow.Components[0].Values;
            sourceModelInitialFlowValues[0] = 3.33;

            var destinationModelInitialConditionsLocations = destinationWFM1D.InitialConditions.Arguments[0].Values;
            var destinationModelInitialFlowLocations = destinationWFM1D.InitialFlow.Arguments[0].Values;
            Assert.AreEqual(0, destinationModelInitialConditionsLocations.Count);
            Assert.AreEqual(0, destinationModelInitialFlowLocations.Count);

            destinationWFM1D.Merge(sourceWFM1D, null);

            var destinationModelInitialConditionsValues = destinationWFM1D.InitialConditions.Components[0].Values;
            var destinationModelInitialFlowValues = destinationWFM1D.InitialFlow.Components[0].Values;
            Assert.AreEqual(2, destinationModelInitialConditionsLocations.Count);
            Assert.AreEqual(1, destinationModelInitialFlowLocations.Count);
            Assert.AreEqual("(Source0_channel, 0)", destinationModelInitialConditionsLocations[0].ToString(), "SourceModel InitialConditions Location was not copied to DestinationModel");
            Assert.AreEqual(5.55, destinationModelInitialConditionsValues[0], "SourceModel InitialConditions value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 75)", destinationModelInitialConditionsLocations[1].ToString(), "SourceModel InitialConditions Location was not copied to DestinationModel");
            Assert.AreEqual(7.77, destinationModelInitialConditionsValues[1], "SourceModel InitialConditions value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 0)", destinationModelInitialFlowLocations[0].ToString(), "SourceModel InitialFlow Location was not copied to DestinationModel");
            Assert.AreEqual(3.33, destinationModelInitialFlowValues[0], "SourceModel InitialFlow value was not copied to DestinationModel");            
        }

        [Test]
        public void TestMergeCopiesSourceModelWindShieldingCoveragesToDestinationModel()
        {
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.WindShielding, 2);
            var sourceModelWindShieldingValues = sourceWFM1D.WindShielding.Components[0].Values;
            sourceModelWindShieldingValues[0] = 5.55;
            sourceModelWindShieldingValues[1] = 7.77;

            var destinationModelWindShieldingLocations = destinationWFM1D.WindShielding.Arguments[0].Values;
            Assert.AreEqual(0, destinationModelWindShieldingLocations.Count);

            destinationWFM1D.Merge(sourceWFM1D, null);

            var destinationModelWindShieldingValues = destinationWFM1D.WindShielding.Components[0].Values;
            Assert.AreEqual(2, destinationModelWindShieldingLocations.Count);
            Assert.AreEqual("(Source0_channel, 0)", destinationModelWindShieldingLocations[0].ToString(), "SourceModel WindShielding Location was not copied to DestinationModel");
            Assert.AreEqual(5.55, destinationModelWindShieldingValues[0], "SourceModel WindShielding value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 75)", destinationModelWindShieldingLocations[1].ToString(), "SourceModel WindShielding Location was not copied to DestinationModel");
            Assert.AreEqual(7.77, destinationModelWindShieldingValues[1], "SourceModel WindShielding value was not copied to DestinationModel");
        }

        [Test]
        public void TestMergeCopiesSourceModelSalinityCoveragesToDestinationModel()
        {
            sourceWFM1D.UseSalt = true;
            sourceWFM1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic; // Needed for F3 and F4 DispersionCoverages
            
            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.InitialSaltConcentration, 2);
            var sourceModelInitialSaltConcentrationValues = sourceWFM1D.InitialSaltConcentration.Components[0].Values;
            sourceModelInitialSaltConcentrationValues[0] = 5.55;
            sourceModelInitialSaltConcentrationValues[1] = 7.77;

            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.DispersionCoverage, 1);
            var sourceModelDispersionCoverageValues = sourceWFM1D.DispersionCoverage.Components[0].Values;
            sourceModelDispersionCoverageValues[0] = 3.33;

            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.DispersionF3Coverage, 1);
            var sourceModelDispersionF3CoverageValues = sourceWFM1D.DispersionF3Coverage.Components[0].Values;
            sourceModelDispersionF3CoverageValues[0] = 2.22;

            WaterFlowModel1DModelMergeTestHelper.SetupCoverageLocations(sourceWFM1D.DispersionF4Coverage, 1);
            var sourceModelDispersionF4CoverageValues = sourceWFM1D.DispersionF4Coverage.Components[0].Values;
            sourceModelDispersionF4CoverageValues[0] = 1.11;

            Assert.IsFalse(destinationWFM1D.UseSalt);
            destinationWFM1D.Merge(sourceWFM1D, null);
            
            Assert.IsTrue(destinationWFM1D.UseSalt);
            Assert.AreEqual(DispersionFormulationType.KuijperVanRijnPrismatic, destinationWFM1D.DispersionFormulationType);
            
            var destinationModelInitialSaltConcentrationLocations = destinationWFM1D.InitialSaltConcentration.Arguments[0].Values;
            var destinationModelInitialSaltConcentrationValues = destinationWFM1D.InitialSaltConcentration.Components[0].Values;
            var destinationModelDispersionCoverageLocations = destinationWFM1D.DispersionCoverage.Arguments[0].Values;
            var destinationModelDispersionCoverageValues = destinationWFM1D.DispersionCoverage.Components[0].Values;
            var destinationModelDispersionF3CoverageLocations = destinationWFM1D.DispersionF3Coverage.Arguments[0].Values;
            var destinationModelDispersionF3CoverageValues = destinationWFM1D.DispersionF3Coverage.Components[0].Values;
            var destinationModelDispersionF4CoverageLocations = destinationWFM1D.DispersionF4Coverage.Arguments[0].Values;
            var destinationModelDispersionF4CoverageValues = destinationWFM1D.DispersionF4Coverage.Components[0].Values;
            
            Assert.AreEqual(2, destinationModelInitialSaltConcentrationLocations.Count);
            Assert.AreEqual(1, destinationModelDispersionCoverageLocations.Count);
            Assert.AreEqual("(Source0_channel, 0)", destinationModelInitialSaltConcentrationLocations[0].ToString(), "SourceModel InitialSaltConcentration Location was not copied to DestinationModel");
            Assert.AreEqual(5.55, destinationModelInitialSaltConcentrationValues[0], "SourceModel InitialSaltConcentration value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 75)", destinationModelInitialSaltConcentrationLocations[1].ToString(), "SourceModel InitialSaltConcentration Location was not copied to DestinationModel");
            Assert.AreEqual(7.77, destinationModelInitialSaltConcentrationValues[1], "SourceModel InitialSaltConcentration value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 0)", destinationModelDispersionCoverageLocations[0].ToString(), "SourceModel DispersionCoverage Location was not copied to DestinationModel");
            Assert.AreEqual(3.33, destinationModelDispersionCoverageValues[0], "SourceModel DispersionCoverage value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 0)", destinationModelDispersionF3CoverageLocations[0].ToString(), "SourceModel DispersionCoverage Location was not copied to DestinationModel");
            Assert.AreEqual(2.22, destinationModelDispersionF3CoverageValues[0], "SourceModel DispersionCoverage value was not copied to DestinationModel");
            Assert.AreEqual("(Source0_channel, 0)", destinationModelDispersionF4CoverageLocations[0].ToString(), "SourceModel DispersionCoverage Location was not copied to DestinationModel");
            Assert.AreEqual(1.11, destinationModelDispersionF4CoverageValues[0], "SourceModel DispersionCoverage value was not copied to DestinationModel");      
        }

        [SetUp]
        public void SetupModels()
        {
            destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100);
            destinationWFM1D.Name = "Destination";
            sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 250);
            sourceWFM1D.Name = "Source";
        }
        
        [Test]
		public void Given2ModelWithNetworkInWhich2NodesFromDestinationModelConnectAChannelFromTheSouceModelWhenMergeThenMergeThisChannelOnlyOnce()
        {
            var network = destinationWFM1D.Network;
            var node2 = network.Nodes.FirstOrDefault(n => n.Name == "node2");
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network, Geometry = new Point(new Coordinate(250, 0)) };

            network.Nodes.Add(node3);
            var channel = new Channel("channel", node2, node3);
            var vertices = new List<Coordinate>
            {
                new Coordinate(100, 0),
                new Coordinate(250, 0)
            };
            channel.Geometry = GeometryFactory.Default.CreateLineString(vertices.ToArray());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionZW.CreateDefault(), (250-100) / 2);

            network.Branches.Add(channel);

            destinationWFM1D.Merge(sourceWFM1D, null);
            Assert.That(destinationWFM1D.Network.Branches.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.Channels.Count(), Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.CrossSections.Count(), Is.EqualTo(3));
            Assert.That(network.Channels.FirstOrDefault(c => c.Name == "Source0_channel"), Is.Not.Null);
        }

        [Test]
		public void Given2ModelWithNetworkInWhich2NodesFromDestinationModelCouldConnectAChannelFromTheSouceModelWhenMergeThenMergeThisChannelOnlyOnce()
        {
            var network = destinationWFM1D.Network;
            var node2 = network.Nodes.FirstOrDefault(n => n.Name == "node2");
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network, Geometry = new Point(new Coordinate(250, 0)) };

            network.Nodes.Add(node3);
            var channel = new Channel("channel", node2, node3);
            var vertices = new List<Coordinate>
            {
                new Coordinate(100, 0),
                new Coordinate(250, 0)
            };
            channel.Geometry = GeometryFactory.Default.CreateLineString(vertices.ToArray());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionZW.CreateDefault(), (250-100) / 2);

            network.Branches.Add(channel);

            var srcNetwork = sourceWFM1D.Network;
            var srcChannel = srcNetwork.Channels.FirstOrDefault();
            Assert.That(srcChannel, Is.Not.Null);
            var srcNode1 = srcNetwork.Nodes.FirstOrDefault(n => n.Name == "node1");
            Assert.That(srcNode1, Is.Not.Null);
            var srcNode2 = srcNetwork.Nodes.FirstOrDefault(n => n.Name == "node2");
            Assert.That(srcNode2, Is.Not.Null);

            srcNode1.Geometry = new Point(new Coordinate(100 + (WaterFlowModel1DModelMergeHelper.metersInRange / 2), 0));
            srcNode2.Geometry = new Point(new Coordinate(250 - (WaterFlowModel1DModelMergeHelper.metersInRange / 2), 0));

            ChannelFromGisImporter.UpdateGeometry(srcChannel, new LineString(new[] { new Coordinate(100 + (WaterFlowModel1DModelMergeHelper.metersInRange / 2), 0), new Coordinate(250 - (WaterFlowModel1DModelMergeHelper.metersInRange / 2), 0) }));

            destinationWFM1D.Merge(sourceWFM1D, null);
            Assert.That(destinationWFM1D.Network.Branches.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.Channels.Count(), Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.Nodes.Count, Is.EqualTo(3));
            Assert.That(destinationWFM1D.Network.CrossSections.Count(), Is.EqualTo(3));
            Assert.That(network.Channels.FirstOrDefault(c => c.Name == "Source0_channel"), Is.Not.Null);
        }

        [Test]
        public void Given2ModelWithNetworkWithOutputCreatedOnSourceModelWhenMergeThenAfterMergeNodeMoveShouldNotCrash()
        {
            sourceWFM1D.NetworkDiscretization = WaterFlowModel1DModelMergeTestHelper.SetupUniformNetworkDiscretization(sourceWFM1D.Network, 11);
            var channel = sourceWFM1D.Network.Channels.FirstOrDefault();
            Assert.That(channel, Is.Not.Null);
            //HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionZW.CreateDefault(), (250 - 100) / 2);
            FileWriterTestHelper.AddObservationPoint(channel, 2, "ObsPnt2", (250 - 100)/2);
            var validationreport = sourceWFM1D.Validate();
            Assert.That(validationreport.AllErrors.Count(), Is.EqualTo(0));

            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(sourceWFM1D);

            //create output on source model
            Assert.That(sourceWFM1D.OutputFunctions.First().Components[0].Values.Count, Is.EqualTo(0), "Coverages not empty initially, unexpected!");
            ActivityRunner.RunActivity(sourceWFM1D);
            Assert.That(sourceWFM1D.Status, Is.Not.EqualTo(ActivityStatus.Failed), "Model run has failed");
            Assert.That(sourceWFM1D.OutputFunctions.First().Components[0].Values.Count, Is.GreaterThan(0), "Coverages empty initially, unexpected!");

            //merge
            destinationWFM1D.Merge(sourceWFM1D, null);
            
            //move a node in destination model... now no crash!
            var node = destinationWFM1D.Network.Nodes.FirstOrDefault(n => n.Name == "node2"); // the connected node
            Assert.That(node, Is.Not.Null);

            HydroRegionEditorHelper.MoveNodeTo(node, node.Geometry.Coordinate.X, node.Geometry.Coordinate.Y + 50);
        }
    }
}