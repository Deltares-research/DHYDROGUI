using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DModelValidatorTest
    {
        [Test]
        public void BranchWithSameSourceAndTargetNode()
        {
            var network = new HydroNetwork();

            var node = new HydroNode { Name = "node", Network = network };
            network.Nodes.Add(node);

            var branch = new Channel("branch", node, node, 100.0);
            network.Branches.Add(branch);

            var model = new WaterFlowModel1D { Network = network };

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "Target and source node of branch 'branch' have the same id, 'node'. Circular branch?"));
        }

        [Test]
        public void GeographicalCoordinateSystemShouldGiveError()
        {
            var network = new HydroNetwork {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326)};

            var model = new WaterFlowModel1D { Network = network };

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "Cannot perform calculation in geographical coordinate system WGS 84"));
        }

        [Test]
        public void TestBoundaryConditions_FlowWaterLevel_ValuesInSequenceAreValid()
        {
            // Setup
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            IHydroNetwork network = model.Network;
            var nwNodes = network.HydroNodes.ToList();

            model.BoundaryConditions.ForEach(bc => bc.DataType = WaterFlowModel1DBoundaryNodeDataType.None);

            var argumentValues = new double[] { 0, 1, 2, 3, 4, 5, 6 };
            var componentValues = new double[] { 0, -1, -2, -3, -5, -8, -13 };

            var boundaryNodeData = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithFlowWaterLevelData(nwNodes[0].Name,
                WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, argumentValues, componentValues);

            var previous = model.BoundaryConditions.First(bc => bc.Feature == nwNodes[0]);
            model.BoundaryConditions.Remove(previous);
            model.BoundaryConditions.Add(boundaryNodeData);           

            // Check negative sequential values should be valid
            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            componentValues = new double[] { -13, -8, -5, -3, -2, -1, 0 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            // Check positive sequential values should be valid
            componentValues = new double[] { 0, 1, 2, 3, 5, 8, 13 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            componentValues = new double[] { 13, 8, 5, 3, 2, 1, 0 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            // check mix of positive and negative values in sequence should be valid
            componentValues = new double[] { -1, 0, 1, 2, 3, 5, 8 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            componentValues = new double[] { 1, 0, -1, -2, -3, -5, -8 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity());
        }

        [Test]
        public void TestBoundaryConditions_FlowWaterLevel_ValuesOutOfSequenceAreInvalid()
        {
            // Setup
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            IHydroNetwork network = model.Network;
            var nwNodes = network.HydroNodes.ToList();

            model.BoundaryConditions.ForEach(bc => bc.DataType = WaterFlowModel1DBoundaryNodeDataType.None);

            var argumentValues = new double[] { 0, 1, 2, 3, 4, 5, 6 };
            var componentValues = new double[] { 0, -1, 2, -3, 5, -8, 13 };

            var boundaryNodeData = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithFlowWaterLevelData(nwNodes[0].Name,
                WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, argumentValues, componentValues);

            var previous = model.BoundaryConditions.First(bc => bc.Feature == nwNodes[0]);
            model.BoundaryConditions.Remove(previous);
            model.BoundaryConditions.Add(boundaryNodeData);

            // Check values not in sequence should be invalid
            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity());

            var expectedWarning = string.Format(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_NonSequentialValues, boundaryNodeData.Name);
            Assert.IsTrue(ContainsWarning(report, expectedWarning));

            componentValues = new double[] { -1, -1, -1, -3, -5, -5, -13 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity());

            expectedWarning = string.Format(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_DuplicateValues, boundaryNodeData.Name);
            Assert.IsTrue(ContainsWarning(report, expectedWarning));

            componentValues = new double[] { 1, 1, 1, 3, 5, 5, 13 };
            boundaryNodeData.Data.Components[0].SetValues(componentValues);

            report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity());

            Assert.IsTrue(ContainsWarning(report, expectedWarning));
        }

        [Test]
        public void FlowBoundaryConditionsWithMultipleConnectingBranchesAreInvalid()
        {
            var network = new HydroNetwork();

            // Add nodes and branches
            var leftNode = new HydroNode { Name = "leftNode", Network = network };
            var rightNode = new HydroNode { Name = "rightNode", Network = network };
            var centerNode = new HydroNode {Name = "centerNode", Network = network};
            
            // Create a network with a central node in which both branches end
            network.Nodes.Add(leftNode);
            network.Nodes.Add(rightNode);
            network.Nodes.Add(centerNode);
            
            var leftBranch = new Channel("branch1", leftNode, centerNode, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };

            leftBranch.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(leftBranch, CrossSectionDefinitionYZ.CreateDefault(), 50.0);
            
            var rightBranch = new Channel("branch2", rightNode, centerNode, 100.0);
            var vertices2 = new List<Coordinate>
                               {
                                   new Coordinate(200, 0),
                                   new Coordinate(100, 0)
                               };

            rightBranch.Geometry = GeometryFactory.CreateLineString(vertices2.ToArray());

            var cs2 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(rightBranch, CrossSectionDefinitionYZ.CreateDefault(), 50.0);

            cs2.Name = "cs2";

            network.Branches.Add(leftBranch);
            network.Branches.Add(rightBranch);

            // Create a model for this network
            var model = new WaterFlowModel1D { Network = network };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 10.0, true, 1.0,
                                                      true, false, true, 10.0);

            Assert.AreEqual(3, model.BoundaryConditions.Count);

            var boundaryConditionCenterNode = model.BoundaryConditions.First(bc2 => bc2.Feature == centerNode);

            // Make the bc for the center node FlowConstant and assert the validation exception
            boundaryConditionCenterNode.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "The boundary condition centerNode - Q: 0 m^3/s has multiple connecting branches. This is only possible for waterlevel boundary conditions."));

            // Make the bc for the center node FlowTimeSeries and assert the validation exception
            boundaryConditionCenterNode.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "The boundary condition centerNode - Q(t) has multiple connecting branches. This is only possible for waterlevel boundary conditions."));

            // Make the bc for the center node FlowWaterLevelTable and assert the validation exception
            boundaryConditionCenterNode.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "The boundary condition centerNode - Q(h) has multiple connecting branches. This is only possible for waterlevel boundary conditions."));
        }

        [Test]
        public void FlowBoundaryConditionsWithNoSaltSpecifiedWhenSaltIsEnabledAreInvalid()
        {
            var network = new HydroNetwork();

            // Add nodes and branches
            var leftNode = new HydroNode { Name = "leftNode", Network = network };
            var rightNode = new HydroNode { Name = "rightNode", Network = network };
            
            // Create a network with a central node in which both branches end
            network.Nodes.Add(leftNode);
            network.Nodes.Add(rightNode);

            var branch = new Channel("branch1", leftNode, rightNode, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };

            branch.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, CrossSectionDefinitionYZ.CreateDefault(), 50.0);
            network.Branches.Add(branch);
            
            // Create a model for this network
            var model = new WaterFlowModel1D { Network = network };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 10.0, true, 1.0,
                                                      true, false, true, 10.0);
            model.UseSalt = true;

            Assert.IsTrue(model.BoundaryConditions.Count > 0);
            var boundaryConditionNode = model.BoundaryConditions.First();

            boundaryConditionNode.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionNode.SaltConditionType = SaltBoundaryConditionType.None;
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                "The boundary condition leftNode - Q: 0 m^3/s has a salinity type of None. All open boundaries must specify salinity values."));
        }

        [Test]
        public void MultipleCrossSectionsTypesOnChannelThrowsException()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var channel = hydroNetwork.Channels.First();
            
            channel.Name = "theName";

            var crossSectionYZ = new CrossSectionDefinitionYZ();
            var crossSectionHfsw = new CrossSectionDefinitionZW();
            
            var cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionYZ, 10.0);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionHfsw, 50.0);

            cs1.Name = "CS0001";

            var waterFlowModel1D = new WaterFlowModel1D {Network = hydroNetwork};

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(waterFlowModel1D),
                                        "Multiple cross-section-types (mix of Standard/ZW and Geometry/YZ) per branch(es) not supported.(theName)"));
        }

        [Test]
        public void QorHDependentRoughnessOnlySupportedForZWCrossSections()
        {
            // Create a network without CS
            var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork(false);

            // Add a YZ crossSection to branch1 and branch2 (otherwise we get a different exception
            var brach1 = waterFlowModel1D.Network.Branches.First();
            var brach2 = waterFlowModel1D.Network.Branches.ElementAt(1);
            var cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(brach1, CrossSectionDefinitionYZ.CreateDefault(), 50);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(brach2, CrossSectionDefinitionYZ.CreateDefault(), 50);

            cs1.Name = "CS00001";

            var branch = waterFlowModel1D.Network.Branches.FirstOrDefault();
            Assert.IsNotNull(branch);

            var mainRoughnessSection = waterFlowModel1D.RoughnessSections.FirstOrDefault();
            Assert.IsNotNull(mainRoughnessSection);

            mainRoughnessSection.AddQRoughnessFunctionToBranch(branch);
            mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 0.0)] = new object[] { 100.0, RoughnessType.Manning };
            mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 10.0)] = new object[] { 110.0, RoughnessType.Manning };

            var report = new WaterFlowModel1DModelValidator().Validate(waterFlowModel1D); 
            Assert.IsTrue(ContainsError(report,
                                        "Branch 'branch1' has Q/H dependent roughness defined on section 'Main'. Q/H dependent roughness is only supported on branches with ZW crosssections (tabulated)."));
        }

        [Test]
        public void QorHDependentRoughnessShouldHaveAtLeastTwoLines()
        {
            // Create a network without cross-sections. 
            var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork(false);

            // Add a YZ crossSection to both branches. 
            var branch1 = waterFlowModel1D.Network.Branches[0];
            var branch2 = waterFlowModel1D.Network.Branches[1];
            var cs1 = CrossSectionDefinitionZW.CreateDefault();
            var cs2 = CrossSectionDefinitionZW.CreateDefault();
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, cs1, 50);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch2, cs2, 50);

            // Add a Q-dependent roughness section with one row
            var mainRoughnessSection = waterFlowModel1D.RoughnessSections.FirstOrDefault();
            Assert.IsNotNull(mainRoughnessSection); 
            mainRoughnessSection.AddQRoughnessFunctionToBranch(branch1);
            mainRoughnessSection.FunctionOfQ(branch1)[20.0, 20.0] = 100.0;

            string errorMessage = "Branch 'branch1' has Q/H dependent roughness defined on section 'Main'. The Q/H dependent roughness table should have at least two rows.";

            var report = new WaterFlowModel1DModelValidator().Validate(waterFlowModel1D);
            Assert.IsTrue(ContainsError(report, errorMessage));

            // Add a second row. 
            mainRoughnessSection.FunctionOfQ(branch1)[20.0, 30.0] = 100.0;
            report = new WaterFlowModel1DModelValidator().Validate(waterFlowModel1D);
            Assert.IsFalse(ContainsError(report, errorMessage));
        }

        [Test]
        public void FeaturesShouldHaveUniqueId()
        {
            var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = waterFlowModel1D.Network.Branches[0];

            var lateral1 = new LateralSource { Name = "01" };
            var lateral2 = new LateralSource { Name = "01" };

            NetworkHelper.AddBranchFeatureToBranch(lateral1, branch, 5);
            NetworkHelper.AddBranchFeatureToBranch(lateral2, branch, 10);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(waterFlowModel1D),
                                        "Several lateral sources with the same id exist"));
        }

        [Test]
        public void ValidateDemoModel()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var report = new WaterFlowModel1DModelValidator().Validate(model);

            Assert.AreNotEqual(ValidationSeverity.Error, report.Severity(), "Demomodel should throw no validation exceptions");
        }

        [Test]
        public void ValidateDiscretizationWith2StructuresBetweenGridPoints()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var firstChannel = model.Network.Channels.First();

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 15.0 }, firstChannel);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 16.0 }, firstChannel);

            var report = WaterFlowModel1DDiscretizationValidator.Validate(model.NetworkDiscretization);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
        }

        [Test]
        //Is obsoleted, but changed so many times. Wait for a while before removing this test
        public void ValidateDiscretizationWithTwoPointsDistanceLessThen0point25()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;

            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.0));
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.2));

            var report = WaterFlowModel1DDiscretizationValidator.Validate(model.NetworkDiscretization);

            Assert.AreEqual(ValidationSeverity.None, report.Severity()); 
        }

        [Test]
        public void ValidateDiscretizationWith2StructuresInACompositeStructureBetweenGridPoints()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var firstChannel = model.Network.Channels.First();
            var weir1 = new Weir { Chainage = 15.0 ,Branch = firstChannel};
            var weir2 = new Weir { Chainage = 15.0, Branch = firstChannel};
            var compositeBranchStructure = new CompositeBranchStructure
                                               {
                                                   Branch = firstChannel,
                                                   Chainage = 15.0
                                               };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure,weir1);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir2);
            
            var report = WaterFlowModel1DDiscretizationValidator.Validate(model.NetworkDiscretization);
            Assert.AreNotEqual(ValidationSeverity.Error, report.Severity()); //should give no error?
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void OutputTimeStepSmallerCalcutionTimeStep()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            
            // First set the time step because time step can modify output time step (bad idea? move from model to property grid)
            model.TimeStep = new TimeSpan(0, 0, 30, 0);
            model.OutputTimeStep = new TimeSpan(0, 0, 10, 0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "The output time step should be a multiple of the calculation time step."),
                                        "Should not allow sub-sampling the calculation time step.");
        }

        [Test]
        public void OutputTimeStepForStructureShouldBeMultipleOfCalculationTimeStep()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.TimeStep = new TimeSpan(0, 0, 30, 0);
            model.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 31, 0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), 
                                        "The structures output time step should be a multiple of the calculation time step."));

            model.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 10, 0);
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), 
                                        "The structures output time step should be a multiple of the calculation time step."),
                                        "Should not allow sub-sampling the calculation time step.");
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void StopShouldBePastStart()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.StartTime = model.StopTime;

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "The calculation period must be positive."));
        }

        [Test]
        public void CalculationPointsShouldBeAtBranchEnds()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.NetworkDiscretization.Locations.Values.RemoveAt(0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), 
                          "Not enough grid points defined for branch branch1. Make sure you have at least gridpoints at start and end of branch."));
        }

        [Test]
        public void AlwaysCalcPointBetweenQBoundaryAndStructure()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();
            var weir = new Weir { Chainage = 1.0, Branch = firstChannel };
            var compositeBranchStructure = new CompositeBranchStructure
            {
                Branch = firstChannel,
                Chainage = 1.0
            };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            //A q boundary !!
            Assert.IsTrue(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model), 
                                        string.Format("A grid point should exist between structure {0} and Q-boundary {1}.", 
                                        compositeBranchStructure.Name, firstChannel.Source.Name)));

            weir.Chainage = firstChannel.Length - 1;
            compositeBranchStructure.Chainage = firstChannel.Length - 1;

            //Not a q boundary
            Assert.IsFalse(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model), 
                                         string.Format("A grid point should exist between structure {0} and Q-boundary {1}.", 
                                         compositeBranchStructure.Name, firstChannel.Target.Name)));

            firstChannel.BranchFeatures.Remove(compositeBranchStructure);
            compositeBranchStructure.Structures.Clear();
            firstChannel.BranchFeatures.Remove(weir);

            Assert.IsFalse(firstChannel.BranchFeatures.Contains(weir));

            var secondChannel = model.Network.Channels.Last();
            compositeBranchStructure.Branch = secondChannel;
            compositeBranchStructure.Chainage = secondChannel.Length - 1;
            weir.Chainage = secondChannel.Length - 1;
            secondChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            //Not a q boundary
            Assert.IsFalse(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model), 
                           string.Format("A grid point should exist between structure {0} and Q-boundary {1}.",
                                         compositeBranchStructure.Name, secondChannel.Target.Name)));
        }

        [Test]
        public void EmptyExtraResistanceFrictionTable()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();
            var extres = new ExtraResistance() { Chainage = 1.0, Branch = firstChannel };
            var compositeBranchStructure = new CompositeBranchStructure
            {
                Branch = firstChannel,
                Chainage = 1.0
            };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, extres);

            //Empty Table
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), string.Format("Empty roughness table")));

        }

        [Test]
        public void NonEmptyExtraResistanceFrictionTable()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();

            var extres = new ExtraResistance() { Chainage = 1.0, Branch = firstChannel };
            extres.FrictionTable[0.0] = 2.0;

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Branch = firstChannel,
                Chainage = 1.0
            };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, extres);

            //Empty Table
            Assert.False(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), string.Format("Empty roughness table")));

        }

        [Test]
        public void AlwaysCalcPointBetweenBoundaryAndExtraResistance()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();

            var extres = new ExtraResistance() { Chainage = 1.0, Branch = firstChannel };
            extres.FrictionTable[0.0] = 2.0;

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Branch = firstChannel,
                Chainage = 1.0
            };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, extres);

            //A q boundary !!
            Assert.IsTrue(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model),
                                        string.Format("A grid point should exist between Extra Resistance {0} and Boundary {1}.",
                                        extres.Name, firstChannel.Source.Name)));

            extres.Chainage = firstChannel.Length - 1;
            compositeBranchStructure.Chainage = firstChannel.Length - 1;

            //Not a q boundary
            Assert.IsTrue(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model),
                                        string.Format("A grid point should exist between Extra Resistance {0} and Boundary {1}.",
                                        extres.Name, firstChannel.Target.Name)));

            firstChannel.BranchFeatures.Remove(compositeBranchStructure);
            compositeBranchStructure.Structures.Clear();
            firstChannel.BranchFeatures.Remove(extres);

            Assert.IsFalse(firstChannel.BranchFeatures.Contains(extres));

            var secondChannel = model.Network.Channels.Last();
            compositeBranchStructure.Branch = secondChannel;
            compositeBranchStructure.Chainage = secondChannel.Length - 1;
            extres.Chainage = secondChannel.Length - 1;
            secondChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, extres);

            //Not a q boundary
            Assert.IsTrue(ContainsWarning(new WaterFlowModel1DModelValidator().Validate(model),
                                        string.Format("A grid point should exist between Extra Resistance {0} and Boundary {1}.",
                                        extres.Name, secondChannel.Target.Name)));
        }

        [Test]
        public void BranchesWithTheSameOrderNumberNeedJustOneCS()
        {
            var network = new HydroNetwork();
            WaterFlowModel1DDemoModelTestHelper.ConfigureDemoNetwork(network);

            var lstBranches = network.Branches.ToList();
            var lstCS = network.CrossSections.ToList();
            network.Branches[1].BranchFeatures.Remove(lstCS[1]);

            Assert.AreEqual(-1,lstBranches[0].OrderNumber);
            Assert.AreEqual(-1, lstBranches[1].OrderNumber);

            Assert.IsTrue(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network), 
                                        string.Format("No cross sections on channel(s) {0}; can not start calculation.", lstBranches[1].Name)));

            lstBranches[0].OrderNumber = 1;
            lstBranches[1].OrderNumber = 1;

            Assert.IsFalse(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                         string.Format("No cross sections on channel(s) {0}; can not start calculation.", lstBranches[1].Name)));
        }

        [Test]
        public void BranchesWithTheSameOrderNumberWithOnlyZWCSShouldBeValid()
        {
            var network = new HydroNetwork();
            WaterFlowModel1DDemoModelTestHelper.ConfigureDemoNetwork(network);

            #region remove all CS and add one ZW cross-section at branch2

                foreach (var cs in network.CrossSections.ToList())
                {
                    cs.Branch.BranchFeatures.Remove(cs);
                }

            var zwcs = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            NetworkHelper.AddBranchFeatureToBranch(zwcs,network.Branches.Last(), 50.0);


            #endregion

            var lstBranches = network.Branches.ToList();
            var lstCS = network.CrossSections.ToList();

            Assert.AreEqual(2, lstBranches.Count);
            Assert.AreEqual(1, lstCS.Count);

            lstBranches[0].OrderNumber = 1;
            lstBranches[1].OrderNumber = 1;

            Assert.IsFalse(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                         string.Format("No cross sections on channel {0}; can not start calculation.", lstBranches[1].Name)));
        }

        [Test]
        public void ThreeBranchesWithTheSameOrderNumber_ConnectedToOneNode_Should_Not_Be_Valid()
        {
            var network = new HydroNetwork();
            WaterFlowModel1DDemoModelTestHelper.ConfigureDemoNetwork(network);

            #region add third branche with CS

                    var node4 = new HydroNode { Name = "Node4", Network = network };
                    node4.Geometry = new Point(200.0, 0.0);
                    network.Nodes.Add(node4);

                    var branch3 = new Channel("branch3", network.Nodes[1], node4, 100.0);

                    branch3.Geometry = new LineString(new[]
                                                            {
                                                                new Coordinate(100, 0),
                                                                new Coordinate(200, 0)
                                                            });

                    network.Branches.Add(branch3);


                    var crossSection3 = new CrossSectionDefinitionXYZ("crs3");
                    var csFeature3 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch3, crossSection3, 50);
                    csFeature3.Name = "cs3";
                    csFeature3.Geometry = new LineString(new[] { new Coordinate(150, -5), new Coordinate(150, 5) });

            #endregion

            var lstBranches = network.Branches.ToList();
            const int orderNumber = 1;

            Assert.AreEqual(3, lstBranches.Count);

            Assert.AreEqual(-1, lstBranches[0].OrderNumber);
            Assert.AreEqual(-1, lstBranches[1].OrderNumber);
            Assert.AreEqual(-1, lstBranches[2].OrderNumber);

            Assert.IsFalse(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                        string.Format("More than two branches with the same ordernumber '{0}' are connected to node {1}; can not start calculation.",
                                        orderNumber, network.Nodes[1].Name)));

            lstBranches[0].OrderNumber = orderNumber;
            lstBranches[1].OrderNumber = orderNumber;
            lstBranches[2].OrderNumber = orderNumber;

            Assert.IsTrue(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                        string.Format("More than two branches with the same ordernumber '{0}' are connected to node {1}; can not start calculation.",
                                        orderNumber, network.Nodes[1].Name)));
        }

        [Test]
        public void MultiTypeOfCS_OnBranchesWithTheSameOrderNumber_Should_Not_Be_Valid()
        {
            var network = new HydroNetwork();
            WaterFlowModel1DDemoModelTestHelper.ConfigureDemoNetwork(network);

            #region Remove first cs and add ZW crossSection

                var cs = network.CrossSections.First();
                cs.Branch.BranchFeatures.Remove(cs);

                var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(0.0,10.0,10.0),
                                        new HeightFlowStorageWidth(10.0,30.0,30.0)
                                    };

                var crossSectionDefinition = new CrossSectionDefinitionZW{Name = "ZW1"};
                var zwCS = new CrossSection(crossSectionDefinition);
                zwCS.Name = "zwCS";
                zwCS.Branch = cs.Branch;
                zwCS.Chainage = 50.0;

                crossSectionDefinition.ZWDataTable.Set(heightFlowStorageWidthData);

                NetworkHelper.AddBranchFeatureToBranch(zwCS, zwCS.Branch, zwCS.Chainage);

            #endregion

            var lstBranches = network.Branches.ToList();
            var lstCS = network.CrossSections.ToList();

            Assert.AreEqual(2, lstBranches.Count);
            Assert.AreEqual(2, lstCS.Count);
            Assert.AreNotEqual(lstCS[0].CrossSectionType, lstCS[1].CrossSectionType);

            lstBranches[0].OrderNumber = 1;
            lstBranches[1].OrderNumber = 1;

            Assert.IsTrue(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                        string.Format("Multiple cross-section-types (mix of Standard/ZW and Geometry/YZ) per branch(es) not supported.({0})",
                                        lstBranches[1].Name + "," + lstBranches[0].Name)));

        }

        [Test]
        public void ZWCrossSectionShouldHaveAtMostOneEntryWithWidthEqualToZero()
        {
            var network = new HydroNetwork();
            WaterFlowModel1DDemoModelTestHelper.ConfigureDemoNetwork(network);

            #region remove all CS and add one ZW cross-section at both branches

            foreach (var cs in network.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            var zwcs = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            zwcs.Name = "zwcs";
            ((CrossSectionDefinitionZW)zwcs.Definition).ZWDataTable.AddCrossSectionZWRow(-15.0, 0.0, 0.0); // add first row of zero width
            NetworkHelper.AddBranchFeatureToBranch(zwcs, network.Branches.First(), 50.0);
            var zwcs2 = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            zwcs2.Name = "zwcs2";
            NetworkHelper.AddBranchFeatureToBranch(zwcs2, network.Branches.Last(), 50.0);

            #endregion

            Assert.IsFalse(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                         string.Format("tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", zwcs.Name)));

            ((CrossSectionDefinitionZW)zwcs.Definition).ZWDataTable.AddCrossSectionZWRow(-20.0, 0.0, 0.0); // add second row of zero width (no points in between)

            Assert.IsTrue(ContainsError(WaterFlowModel1DHydroNetworkValidator.Validate(network),
                                 string.Format("tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", zwcs.Name)));

        }

        [Test]
        public void ValidateFlowModelWithTimestepsEqualToZero()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var oldvalueGridOutputTimeStep = new TimeSpan(model.OutputSettings.GridOutputTimeStep.Ticks);
            model.OutputSettings.GridOutputTimeStep = new TimeSpan(0);
            
            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "Grid output time step must be positive value."));

            model.OutputSettings.GridOutputTimeStep = oldvalueGridOutputTimeStep;
            var oldvalueStructureOutputTimeStep = new TimeSpan(model.OutputSettings.StructureOutputTimeStep.Ticks);
            model.OutputSettings.StructureOutputTimeStep = new TimeSpan(0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "Structures output time step must be positive value."));

            model.OutputSettings.StructureOutputTimeStep = oldvalueStructureOutputTimeStep;
            var oldValueOutputTimeStep = model.OutputTimeStep;
            model.OutputTimeStep = new TimeSpan(0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "The output time step must be positive."));

            model.OutputTimeStep = oldValueOutputTimeStep;
            model.TimeStep = new TimeSpan(0);

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "The calculation time step must be positive."));
        }

        [Test]
        public void ValidateFlowModelInputRestartStatePathIncorect()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            const string invalidPath = "invalidPath";
            var fileBasedRestartState = new FileBasedRestartState("test", invalidPath);
            ((IFileBased)fileBasedRestartState).Path = invalidPath;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                "Model state file does not exist: " + invalidPath));
        }

        [Test]
        public void ValidateFlowModelInputRestartStatePathToNonZip()
        {
            var filePathToNonZipFile = TestHelper.GetTestFilePath("parameters.xml");
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            var fileBasedRestartState = new FileBasedRestartState("test", filePathToNonZipFile);
            ((IFileBased)fileBasedRestartState).Path = filePathToNonZipFile;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "Model state file should be zip file and have the extension .zip"));
        }

        [Test]
        public void ValidateFlowModelInputRestartStateConsistenWithoutMetadata()
        {
            var validRestartFilePath = TestHelper.GetTestFilePath("valid_state_flow model 1d (demo network).zip");
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var fileBasedRestartState = new FileBasedRestartState("test", validRestartFilePath);
            ((IFileBased)fileBasedRestartState).Path = validRestartFilePath;
            model.RestartInput = fileBasedRestartState;

            model.UseRestart = true;

            Assert.AreEqual(0, new WaterFlowModel1DModelValidator().Validate(model).Issues.Count());
        }

        [Test]
        public void ValidateFlowModelInputRestartStateNotConsistentWithModel()
        {
            var validRestartFilePath = TestHelper.GetTestFilePath("valid_state_flow model 1d (demo network).zip");
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var fileBasedRestartState = new FileBasedRestartState("test", validRestartFilePath);
            ((IFileBased)fileBasedRestartState).Path = validRestartFilePath;
            model.RestartInput = fileBasedRestartState;

            model.UseRestart = true;

            Assert.AreEqual(0, new WaterFlowModel1DModelValidator().Validate(model).Issues.Count());

            var invalidRestartFilePath = TestHelper.GetTestFilePath("invalid_state_flow model 1d (demo network).zip");

            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);

            var validationReport = new WaterFlowModel1DModelValidator().Validate(model);
            Assert.AreEqual(7, validationReport.ErrorCount);
            var validationIssues = validationReport.AllErrors;
            Assert.IsTrue(validationIssues.All(vi => vi.Subject.Equals("Input restart state")));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfGridPoints: Value of '1' in restart state not matching expected value of '27' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfPumps: Missing"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfWeirs: Value of '3' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfBridges: Value of '4' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfChannels: Value of '5' in restart state not matching expected value of '2' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfCulverts: Value of '8' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfHydroNodes: Value of '11' in restart state not matching expected value of '3' of current situation"));

            model.UseRestart = false;

            Assert.AreEqual(0, new WaterFlowModel1DModelValidator().Validate(model).Issues.Count());
        }

        [Test]
        public void ValidateFlowModelInputRestartStateInvalidModelType()
        {
            var invalidRestartFilePath =
                TestHelper.GetTestFilePath("invalid_ModelType_state_flow model 1d (demo network).zip");

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);
            model.UseRestart = true;

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "Model type of 'test' is not compatible."));
        }

        [Test]
        public void ValidateFlowModelInputRestartStateInvalidVersion()
        {
            var invalidRestartFilePath =
                TestHelper.GetTestFilePath("invalid_Version_state_flow model 1d (demo network).zip");

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);
            model.UseRestart = true;

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model), "Version 2 is not supported."));
        }

        [Test]
        public void ValidatePumpHasPositiveCapacity()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = model.Network.Branches[0];
            var pump = new Pump("A"){Capacity = -1.2};
            var compStructure = new CompositeBranchStructure();
            compStructure.Structures.Add(pump);
            NetworkHelper.AddBranchFeatureToBranch(compStructure, branch, 10.0);

            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.IsTrue(ContainsError(report, "pump 'A': Capacity must be greater than or equal to 0."));
        }

        [Test]
        public void ValidatePumpHasProperSuctionAndDeliverySideControls()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = model.Network.Branches[0];
            var pump = new Pump("A")
            {
                ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl,
                StartDelivery = 1.2, StopDelivery = -1.2,
                StartSuction = -1.2, StopSuction = 1.2
            };
            var compStructure = new CompositeBranchStructure();
            compStructure.Structures.Add(pump);
            NetworkHelper.AddBranchFeatureToBranch(compStructure, branch, 10.0);

            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.IsTrue(ContainsError(report, "pump 'A': Delivery start level must be less than or equal to delivery stop level."));
            Assert.IsTrue(ContainsError(report, "pump 'A': Suction start level must be greater than or equal to suction stop level."));
        }

        [Test]
        public void ValidateCrestWidthNotNegative()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = model.Network.Branches[0];
            var pump = new Weir("A") { CrestWidth = -1.2 };
            var compStructure = new CompositeBranchStructure();
            compStructure.Structures.Add(pump);
            NetworkHelper.AddBranchFeatureToBranch(compStructure, branch, 10.0);

            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.IsTrue(ContainsError(report, "weir 'A': Crest width must be greater than or equal to 0."));
        }

        [Test]
        public void ValidateGateOpeningNotNegative()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = model.Network.Branches[0];
            var weir = new Weir("W")
            {
                CrestLevel = 1.2,
                WeirFormula = new GatedWeirFormula
                {
                    GateOpening = -1.2
                }
            };
            var compStructure = new CompositeBranchStructure();
            compStructure.Structures.Add(weir);
            NetworkHelper.AddBranchFeatureToBranch(compStructure, branch, 10.0);

            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.IsTrue(ContainsError(report, "weir 'W': Gate opening must be greater than or equal to 0."));
        }

        [Test]
        public void ValidateGatedWeirFlowLimitationsAreNotNegative()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var branch = model.Network.Branches[0];
            var gatedWeirFormula = new GatedWeirFormula(true)
            {
                UseMaxFlowPos = true,
                MaxFlowPos = -1.2,
                UseMaxFlowNeg = true,
                MaxFlowNeg = -3.4
            };
            var weir = new Weir("W", true) { Branch = null, WeirFormula = gatedWeirFormula };
            var compStructure = new CompositeBranchStructure();
            compStructure.Structures.Add(weir);
            NetworkHelper.AddBranchFeatureToBranch(compStructure, branch, 10.0);

            var report = WaterFlowModel1DModelDataValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.IsTrue(ContainsError(report, "weir 'W': Maximum positive flow restrictions must be greater than or equal to 0."));
            Assert.IsTrue(ContainsError(report, "weir 'W': Maximum negative flow restrictions must be greater than or equal to 0."));

            gatedWeirFormula.UseMaxFlowNeg = false;
            gatedWeirFormula.UseMaxFlowPos = false;
            report = WaterFlowModel1DModelDataValidator.Validate(model);

            Assert.IsFalse(ContainsError(report, "weir 'W': Maximum positive flow restrictions must be greater than or equal to 0."));
            Assert.IsFalse(ContainsError(report, "weir 'W': Maximum negative flow restrictions must be greater than or equal to 0."));
        }

        [Test]
        public void ModelParameterLimtyphu1DTest()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1", Network = network };
            var node2 = new HydroNode { Name = "node2", Network = network };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch", node1, node2, 100.0);
            network.Branches.Add(branch);

            var model = new WaterFlowModel1D { Network = network };

            model.ParameterSettings.First(p => p.Name == "Limtyphu1D").Value = "21";

            Assert.IsTrue(ContainsError(new WaterFlowModel1DModelValidator().Validate(model),
                                        "Numerical Parameter Limtyphu1D must be 1 - 3. Given Value is: 21"));
        }

        [Test]
        public void ValidateSalinityIsValidWhenSaltIsDisabled()
        {
            var model = new WaterFlowModel1D
            {
                UseSalt = false
            };
            
            var validationReport = WaterFlowModel1DSalinityValidator.Validate(model);
            Assert.IsFalse(validationReport.AllErrors.Any());
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticMouthNodeIdValidationTestFailsWhenNoSpecifiedNode()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;
            
            //No specified node, should fail
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 1, Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_No_Estuary_mouth_node_specified_);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticMouthNodeIdValidationTestWithValidNodeId()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            Assert.NotNull(model.Network);
            Assert.NotNull(model.Network.Nodes);
            var node1 = model.Network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);
            
            //Valid node, should not fail.        
            model.SalinityEstuaryMouthNodeId = node1.Name;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticMouthNodeIdValidationTestFailsWhenNodeDoesNotExist()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            var network = model.Network;
            Assert.NotNull(network);
            Assert.NotNull(network.Nodes);
            var node1 = network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);

            //Node does not exist, should fail.
            model.SalinityEstuaryMouthNodeId = "test";
            var expectedErrorMessage = string.Format(
                Resources
                    .WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Can_not_find_specified_estuary_mouth_node__0__,
                model.SalinityEstuaryMouthNodeId);
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 1, expectedErrorMessage);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticMouthNodeIdValidationTestFailsWhenNodeExistsButIsNotValid()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            var network = model.Network;
            Assert.NotNull(network);
            Assert.NotNull(network.Nodes);
            var node1 = network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);

            //Node exists, but it's not valid, should fail.
            model.SalinityEstuaryMouthNodeId = node1.Name;
            var node3 = new HydroNode("node3");
            var channel2 = new Channel(node1, node3);
            network.Nodes.Add(node3);
            network.Branches.Add(channel2);

            var expectedErrorMessage = string.Format(
                Resources
                    .WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Estuary_mouth_node__0__is_not_a_boundary_node_,
                model.SalinityEstuaryMouthNodeId);
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 1, expectedErrorMessage);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticDispersionF4CoverageValidationTestPassesWhenNoF4CoverageValues()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            var network = model.Network;
            Assert.NotNull(network);
            Assert.NotNull(network.Nodes);
            var node1 = network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);

            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);
            
            //Valid node, should not fail (dedicated test above)
            model.SalinityEstuaryMouthNodeId = node1.Name;

            //Wihout F4 Coverages it should be valid
            model.DispersionF4Coverage.Clear();
            Assert.IsFalse(model.DispersionF4Coverage.GetValues<double>().Any());
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticDispersionF4CoverageValidationTestPassesWithValidValues()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            var network = model.Network;
            Assert.NotNull(network);
            Assert.NotNull(network.Nodes);
            var node1 = network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);

            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);

            //Valid node, should not fail (dedicated test above)
            model.SalinityEstuaryMouthNodeId = node1.Name;

            //If the values are different from 0, it should be valid.
            model.DispersionF4Coverage.Locations.AddValues(new[] { new NetworkLocation(branch, 0) });
            model.DispersionF4Coverage.SetValues(new[] { -0.1 }); /* Not all values can be 0 validation.*/
            Assert.IsTrue(model.DispersionF4Coverage.GetValues<double>().Any());
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        [Test]
        public void SaltKuijperVanRijnPrismaticDispersionF4CoverageValidationTestFailsWithAllValuesSetToZero()
        {
            var model = GetFlow1DSalinityKuijperModel();
            var expectedErrors = 0;

            var network = model.Network;
            Assert.NotNull(network);
            Assert.NotNull(network.Nodes);
            var node1 = network.Nodes.FirstOrDefault();
            Assert.NotNull(node1);

            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);

            //Valid node, should not fail (dedicated test above)
            model.SalinityEstuaryMouthNodeId = node1.Name;

            //If all the values are 0, then it should fail.
            model.DispersionF4Coverage.Clear();
            Assert.IsFalse(model.DispersionF4Coverage.GetValues<double>().Any());

            model.DispersionF4Coverage.Locations.AddValues(new[] { new NetworkLocation(branch, 0) });
            model.DispersionF4Coverage.SetValues(new[] { 0.0 });
            Assert.IsTrue(model.DispersionF4Coverage.GetValues<double>().Any());
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 1, Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_F4_Coverage_values_cannot_all_be_set_to_0__Either_remove_them_or_set_a_valid_value_);

            //Turn off salinity -> no errors.
            model.UseSalt = false;
            CheckSalinityValidationErrorsAsExpected(model, expectedErrors = 0);
        }

        private static WaterFlowModel1D GetFlow1DSalinityKuijperModel()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode {Name = "node1", Network = network};
            var node2 = new HydroNode {Name = "node2", Network = network};
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch", node1, node2, 100.0);
            network.Branches.Add(branch);

            var model = new WaterFlowModel1D
            {
                Network = network,
                UseSalt = true,
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic,
                UseSaltInCalculation = true
            };
            return model;
        }

        private static void CheckSalinityValidationErrorsAsExpected(WaterFlowModel1D model, int expectedErrors, string expectedErrorMessage = null)
        {
            var validationReport = WaterFlowModel1DSalinityValidator.Validate(model);
            Assert.AreEqual(expectedErrors, validationReport.AllErrors.Count());

            if (expectedErrors == 0) return;

            var salinityCategory = Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity;
            
            //For the moment we only test one error at a time, if needed in the future this can be extended.
            var salinityReport =
                validationReport.SubReports.FirstOrDefault(sr => sr.Category == salinityCategory && sr.ErrorCount == expectedErrors);
            Assert.NotNull(salinityReport);

            //Check the error is as we expect.
            var errorFound = salinityReport.AllErrors.FirstOrDefault();
            Assert.NotNull(errorFound);

            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
            Assert.AreEqual(model, errorFound.Subject);
        }

        [TestCase(false, DispersionFormulationType.Constant, false, false)]
        [TestCase(false, DispersionFormulationType.Constant, true, false)]
        [TestCase(false, DispersionFormulationType.KuijperVanRijnPrismatic, false, true)]
        [TestCase(false, DispersionFormulationType.KuijperVanRijnPrismatic, true, false)]

        [TestCase(true, DispersionFormulationType.Constant, false, false)]
        [TestCase(true, DispersionFormulationType.Constant, true, false)]
        [TestCase(true, DispersionFormulationType.KuijperVanRijnPrismatic, false, true)]
        [TestCase(true, DispersionFormulationType.KuijperVanRijnPrismatic, true, false)]
        public void ModelSalinityIniFileExist1DTest(bool salinityValidNonConstantFormulation, DispersionFormulationType dispersionFormulationType, bool hasNode, bool shouldThrowSalinityIniError)
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1", Network = network };
            var node2 = new HydroNode { Name = "node2", Network = network };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch", node1, node2, 100.0);
            network.Branches.Add(branch);

            var model = new WaterFlowModel1D
            {
                Network = network,
                UseSalt = true,
                DispersionFormulationType = dispersionFormulationType
            };

            if (salinityValidNonConstantFormulation)
            {
                model.UseSaltInCalculation = true;

                if (dispersionFormulationType == DispersionFormulationType.KuijperVanRijnPrismatic)
                {
                    model.DispersionF4Coverage.Locations.AddValues(new[] { new NetworkLocation(branch, 0) });
                    model.DispersionF4Coverage.SetValues(new[] { -0.1 });
                }
            }

            if (hasNode)
            {
                model.SalinityEstuaryMouthNodeId = node1.Name;
            }

            var validationReport = WaterFlowModel1DSalinityValidator.Validate(model);
            Assert.AreEqual(shouldThrowSalinityIniError, validationReport.AllErrors.Any());
        }

        [Test]
        public void ModelValidationSalinityTest()
        {
            /* Basic model */
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.UseSalt = true;
            var validationReport = WaterFlowModel1DSalinityValidator.Validate(model);

            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void ModelValidationTemperatureTest()
        {
            /* Basic model */
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.UseTemperature = true;
            model.TemperatureModelType = TemperatureModelType.Composite;
            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);

            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void ModelValidateInitialTemperatureWaterFlowModel1DFailsWithWrongTemperatureTest()
        {
            /* Basic model */
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.UseTemperature = true;
            var destinationChannel = model.Network.Channels.First();
            model.InitialTemperature[new NetworkLocation(destinationChannel, 0.0)] = 85.0;
            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);

            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void ModelValidationTemperatureTestFailsWhenValuesOutOfRange()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.UseTemperature = true;
            model.TemperatureModelType = TemperatureModelType.Composite;
            model.DaltonNumber = -3;
            model.StantonNumber = -3;
            model.BackgroundTemperature = 80;
            model.SurfaceArea = -30;

            /**/
            var startTime = DateTime.Now;
            model.StartTime = startTime;
            var meteoDataArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2) };

            model.MeteoData.Clear();
            model.MeteoData.Arguments[0].SetValues(meteoDataArguments);
            /**/

            model.MeteoData.AirTemperature.SetValues(new[] { -1.0, 61.0 });
            model.MeteoData.RelativeHumidity.SetValues(new[] { -1.0, 101.0 });
            model.MeteoData.Cloudiness.SetValues(new[] { -1.0, 101.1 });

            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);

            Assert.That(validationReport.ErrorCount, Is.EqualTo(10));
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenBoundaryConditionsTimeDependentInRange()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.UseTemperature = true;
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);

            WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent;
            qBoundary.TemperatureTimeSeries[startTime] = 30.0;
            qBoundary.TemperatureTimeSeries[stopTime] = 10.0;
            qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
            var errorInBoundary = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(qBoundary));
            Assert.That(errorInBoundary, Is.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void ModelValidationTemperatureTestFailsWhenBoundaryConditionsTimeDependentOutOfRange()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.UseTemperature = true;
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);

            WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent;
            qBoundary.TemperatureTimeSeries[startTime] = 300.0;
            qBoundary.TemperatureTimeSeries[stopTime] = 10.0;
            qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
            var errorInBoundary = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(qBoundary));
            Assert.That(errorInBoundary, Is.Not.Null);
            Assert.That(errorInBoundary.Subject, Is.EqualTo(qBoundary));
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenBoundaryConditionsConstantInRange()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.UseTemperature = true;

            WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.TemperatureConditionType = TemperatureBoundaryConditionType.Constant;
            qBoundary.TemperatureConstant = 30;
            qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
            var errorInBoundary = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(qBoundary));
            Assert.That(errorInBoundary, Is.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenBoundaryConditionsConstantOutOfRange()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            model.UseTemperature = true;

            WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.TemperatureConditionType = TemperatureBoundaryConditionType.Constant;
            qBoundary.TemperatureConstant = 300;
            qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
            var errorInBoundary = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(qBoundary));
            Assert.That(errorInBoundary, Is.Not.Null);
            Assert.That(errorInBoundary.Subject, Is.EqualTo(qBoundary));
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenLateralSourceDataTimeDependentInRange()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            using (var model = new WaterFlowModel1D { Network = network })
            {
                network.Branches[0].BranchFeatures.Add(new LateralSource());

                var startTime = new DateTime(2000, 1, 1);
                var stopTime = new DateTime(2000, 1, 10);
                model.UseTemperature = true;
                WaterFlowModel1DLateralSourceData lateralSourceData = model.LateralSourceData[0];
                lateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent;
                lateralSourceData.TemperatureTimeSeries[startTime] = 30.0;
                lateralSourceData.TemperatureTimeSeries[stopTime] = 10.0;

                Assert.IsTrue(model.LateralSourceData.All(bc => bc.UseTemperature));
                var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
                var errorInLateral = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(lateralSourceData));
                Assert.That(errorInLateral, Is.Null);
                Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void ModelValidationTemperatureTestFailsWhenLateralSourceDataTimeDependentOutOfRange()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            using (var model = new WaterFlowModel1D { Network = network })
            {
                network.Branches[0].BranchFeatures.Add(new LateralSource());

                var startTime = new DateTime(2000, 1, 1);
                var stopTime = new DateTime(2000, 1, 10);
                model.UseTemperature = true;
                WaterFlowModel1DLateralSourceData lateralSourceData = model.LateralSourceData[0];
                lateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent;
                lateralSourceData.TemperatureTimeSeries[startTime] = 300.0;
                lateralSourceData.TemperatureTimeSeries[stopTime] = 10.0;

                Assert.IsTrue(model.LateralSourceData.All(bc => bc.UseTemperature));
                var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
                var errorInLateral = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(lateralSourceData));
                Assert.That(errorInLateral, Is.Not.Null);
                Assert.That(errorInLateral.Subject, Is.EqualTo(lateralSourceData));
                Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenLateralSourceDataConstantInRange()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            using (var model = new WaterFlowModel1D { Network = network })
            {
                network.Branches[0].BranchFeatures.Add(new LateralSource());
                
                model.UseTemperature = true;
                WaterFlowModel1DLateralSourceData lateralSourceData = model.LateralSourceData[0];
                lateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType.Constant;
                lateralSourceData.TemperatureConstant = 30.0;

                Assert.IsTrue(model.LateralSourceData.All(bc => bc.UseTemperature));
                var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
                var errorInLateral = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(lateralSourceData));
                Assert.That(errorInLateral, Is.Null);
                Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void ModelValidationTemperatureTestSucceedsWhenLateralSourceDataConstantOutOfRange()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            using (var model = new WaterFlowModel1D { Network = network })
            {
                network.Branches[0].BranchFeatures.Add(new LateralSource());

                model.UseTemperature = true;
                WaterFlowModel1DLateralSourceData lateralSourceData = model.LateralSourceData[0];
                lateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType.Constant;
                lateralSourceData.TemperatureConstant = 300.0;

                Assert.IsTrue(model.LateralSourceData.All(bc => bc.UseTemperature));
                var validationReport = WaterFlowModel1DTemperatureValidator.Validate(model);
                var errorInLateral = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(lateralSourceData));
                Assert.That(errorInLateral, Is.Not.Null);
                Assert.That(errorInLateral.Subject, Is.EqualTo(lateralSourceData));
                Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            }
        }

        private static bool ContainsError(ValidationReport report, string errorMessage)
        {
            return ContainsValidationIssue(report, errorMessage, ValidationSeverity.Error);
        }

        private static bool ContainsWarning(ValidationReport report, string errorMessage)
        {
            return ContainsValidationIssue(report, errorMessage, ValidationSeverity.Warning);
        }
        
        private static bool ContainsValidationIssue(ValidationReport report, string errorMessage, ValidationSeverity severity)
        {
            foreach (var issue in report.Issues.Where(i => i.Severity == severity))
            {
                Console.WriteLine(issue.Message);

                if (issue.Message == errorMessage) return true;
            }

            return report.SubReports.Any(subReport => ContainsValidationIssue(subReport, errorMessage, severity));
        }

         
    }
}
