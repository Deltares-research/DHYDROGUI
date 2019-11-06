using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DInterpolationExtrapolationCSTest
    {
        private IChannel branch1, branch2, branch3;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void YZ_ExtrapolateOver3BranchesBasedOn1CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 10.0),
                                        new Coordinate(10.0, 0.0),
                                        new Coordinate(20.0, 0.0),
                                        new Coordinate(30.0, 10.0),
                                    };

            var cs = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch3, offset, yzCoordinates);

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n =>n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5.0;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues =  flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value (all the same: should be 5.0)
            Assert.AreEqual(5.0, waterlevelValues[0],0.1);

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]// put WIP after TOOLS-6409
        public void YZ_InterpolateOver3BranchesBasedOn2CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 10.0),
                                        new Coordinate(10.0, 0.0),
                                        new Coordinate(20.0, 0.0),
                                        new Coordinate(30.0, 10.0),
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, offset, yzCoordinates);

            yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(10.0, -10.0),
                                        new Coordinate(20.0, -10.0),
                                        new Coordinate(30.0, 0.0),
                                    };

            var cs3 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, offset, yzCoordinates);

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value (half between 5 and -5 -> is 0) 
            // note: apparently the above reasoning does no longer hold with interpolation across nodes.
            // How could we even determine this expected value:
            Assert.AreEqual(0.0, waterlevelValues[0],0.2);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void YZ_ExtrapolateOver2BranchesAndDoNotInterpolateOver3Branches()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = -1; 

            // add cross-sections
            var offset = 50.0;

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 10.0),
                                        new Coordinate(10.0, 0.0),
                                        new Coordinate(20.0, 0.0),
                                        new Coordinate(30.0, 10.0),
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, offset, yzCoordinates);

            yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(10.0, -10.0),
                                        new Coordinate(20.0, -10.0),
                                        new Coordinate(30.0, 0.0),
                                    };

            var cs3 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch3, offset, yzCoordinates);

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value Constant extrapolation from branch1 -> = 5
            Assert.AreEqual(1.0, waterlevelValues[0], 0.1);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ZW_ExtrapolateOver3BranchesBasedOn1CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(0.0,10.0,10.0),
                                        new HeightFlowStorageWidth(10.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch3, offset, heightFlowStorageWidthData, "CS1");

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value (all the same: should be 5.0)
            Assert.AreEqual(5.0, waterlevelValues[0],0.1);

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]// put WIP after TOOLS-6409
        public void ZW_InterpolateOver3BranchesBasedOn2CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(0.0,10.0,10.0),
                                        new HeightFlowStorageWidth(10.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch1, offset, heightFlowStorageWidthData, "CS1");

            heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(-10.0,10.0,10.0),
                                        new HeightFlowStorageWidth(0.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch3, offset, heightFlowStorageWidthData, "CS3");

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>
            (
                new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2), 
                new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
            );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value (half between 5 and -5 -> is 0)
            // note: how could we even determine the expected value below?
            Assert.AreEqual(-3.05, waterlevelValues[0], 0.1);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ZW_ExtrapolateOver2BranchesAndDoNotInterpolateOver3Branches()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = -1;

            // add cross-sections
            var offset = 50.0;

            var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(0.0,10.0,10.0),
                                        new HeightFlowStorageWidth(10.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch1, offset, heightFlowStorageWidthData, "CS1");

            heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(-10.0,10.0,10.0),
                                        new HeightFlowStorageWidth(0.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch3, offset, heightFlowStorageWidthData, "CS3");

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value Constant extrapolation from branch1 -> = 0
            Assert.AreEqual(1.0, waterlevelValues[0], 0.1);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]//TOOLS-6653
        public void Rectangles_InterpolateOver3BranchesBasedOn2CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var cs1 = CrossSection.CreateDefault(CrossSectionType.Standard, branch1, offset);
            var cs1Definition = ((CrossSectionDefinitionStandard)cs1.Definition);
            ((CrossSectionDefinitionStandard)cs1.Definition).ShapeType = CrossSectionStandardShapeType.Rectangle;
            ((CrossSectionStandardShapeRectangle)cs1Definition.Shape).Width = 20;
            ((CrossSectionStandardShapeRectangle)cs1Definition.Shape).Height = 10;
            cs1Definition.ShiftLevel(0 - cs1Definition.LevelShift);


            var cs3 = CrossSection.CreateDefault(CrossSectionType.Standard, branch3, offset);
            ((CrossSectionDefinitionStandard)cs3.Definition).ShapeType = CrossSectionStandardShapeType.Rectangle;
            ((CrossSectionDefinitionStandard)cs3.Definition).Sections.Add(new CrossSectionSection { SectionType = new CrossSectionSectionType() });
            branch3.BranchFeatures.Add(cs3);

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;


            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new IVariableValueFilter[]{
                        new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2),
                        new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
                        }
                    );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            //Expected value (half between 5 and 5 -> is5)
            Assert.AreEqual(5.0, waterlevelValues[0], 0.1); //check expectations
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]// put WIP after TOOLS-6409
        public void Rectangle_ZW_InterpolateOver3BranchesBasedOn2CS()
        {
            var flowModel1D = GetModelWith3BranchesInARowA100Meter();

            branch1.OrderNumber = 1;
            branch2.OrderNumber = 1;
            branch3.OrderNumber = 1;

            // add cross-sections
            var offset = 50.0;

            var cs1 = CrossSection.CreateDefault(CrossSectionType.Standard, branch1, offset);
            var cs1Definition = (CrossSectionDefinitionStandard) cs1.Definition;
            ((CrossSectionDefinitionStandard)cs1.Definition).ShapeType = CrossSectionStandardShapeType.Rectangle;
            ((CrossSectionStandardShapeRectangle) cs1Definition.Shape).Width = 20;
            ((CrossSectionStandardShapeRectangle)cs1Definition.Shape).Height = 10;
            cs1Definition.ShiftLevel(0 - cs1Definition.LevelShift);

            ((CrossSectionDefinitionStandard)cs1.Definition).Sections.Add(new CrossSectionSection
            {
                SectionType = new CrossSectionSectionType(),
                MinY = 0.0,
                MaxY = 10.0
            });
            branch1.BranchFeatures.Add(cs1);

            var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
                                    {
                                        new HeightFlowStorageWidth(-10.0,10.0,10.0),
                                        new HeightFlowStorageWidth(0.0,30.0,30.0)
                                    };

            CrossSectionHelper.AddZWCrossSectionFromHeightWidthTable(branch3, offset, heightFlowStorageWidthData, "CS3");

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 1;
            flowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            flowModel1D.InitialConditions.DefaultValue = 5.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node1"));
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.WaterLevel = 5;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == "node4"));
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -5;

            GenerateDiscretisation(flowModel1D, 10.0);
            RunModelFor5Steps(flowModel1D);

            var timeStep2 = flowModel1D.StartTime.Add(flowModel1D.TimeStep);
            timeStep2 = timeStep2.Add(flowModel1D.TimeStep);

            var locationMiddleBranch2 = flowModel1D.NetworkDiscretization.Locations.Values[16];

            var waterlevelValues = flowModel1D.OutputWaterLevel.GetValues<double>
            (
                new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], timeStep2), 
                new VariableValueFilter<INetworkLocation>(flowModel1D.OutputWaterLevel.Arguments[1], locationMiddleBranch2)
            );

            //check if filter gives just one value
            Assert.AreEqual(1, waterlevelValues.Count);

            // how can we determine what to expect here, 
            // should this test even be in integration? 
            Assert.AreEqual(3.15, waterlevelValues[0], 0.1);
        }

        private void GenerateDiscretisation(WaterFlowModel1D flowModel1D, double distance)
        {
            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(flowModel1D.Network);

            foreach (var channel in flowModel1D.Network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 1.0, true, 0.5, true, false,
                                                          true, distance);
            }

            flowModel1D.NetworkDiscretization = networkDiscretization;
        }


        private WaterFlowModel1D GetModelWith3BranchesInARowA100Meter()
        {
            //Assert.Fail("Crashes all tests on server do not remove!");
            // create simplest network
            var network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "CrossSectionRoughnessSectionType" };
            network.CrossSectionSectionTypes.Add(crossSectionType);
            
            // add nodes and branches

            var startCoordinate = new Coordinate(0, 0);
            var secondCoordinate = new Coordinate(100, 0);
            var thirdCoordinate = new Coordinate(200, 0);
            var endCoordinate = new Coordinate(300, 0);

            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(secondCoordinate) };
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network, Geometry = new Point(thirdCoordinate) };
            IHydroNode node4 = new HydroNode { Name = "node4", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            network.Nodes.Add(node4);

            branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                            {
                                   startCoordinate,
                                   secondCoordinate
                            };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            branch2 = new Channel("branch2", node2, node3);
            vertices = new List<Coordinate>
                           {
                                   secondCoordinate,
                                   thirdCoordinate
                           };
            branch2.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            branch3 = new Channel("branch3", node3, node4);
            vertices = new List<Coordinate>
                           {
                                   thirdCoordinate,
                                   endCoordinate
                           };
            branch3.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Branches.Add(branch3);

            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D();

            // set network
            flowModel1D.Network = network;

            return flowModel1D;

        }

        private void RunModelFor5Steps(WaterFlowModel1D flowModel1D)
        {

            //use a fixed startdate for comparison.
            var t = new DateTime(2000, 1, 1);

            flowModel1D.StartTime = t;
            flowModel1D.StopTime = t.AddMinutes(5);
            flowModel1D.TimeStep = new TimeSpan(0, 1, 0);
            flowModel1D.OutputTimeStep = new TimeSpan(0, 1, 0);

            int timeStepCount = 0;

            flowModel1D.StatusChanged += (sender, args) =>
            {
                if (flowModel1D.Status == ActivityStatus.Initialized)
                {
                    Assert.IsTrue(File.Exists(WaterFlowModel1D.TemplateDataZipFile));
                }

                if (flowModel1D.Status == ActivityStatus.Executed || flowModel1D.Status == ActivityStatus.Done)
                {
                    timeStepCount++;
                }
            };
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(flowModel1D);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(flowModel1D.Network);

            ActivityRunner.RunActivity(flowModel1D);

            if (flowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }

            Assert.AreEqual(5, timeStepCount);

            Assert.IsTrue(flowModel1D.CurrentTime >= flowModel1D.StopTime);
        }

    }
}
