using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DObservationPointsTest
    {
        [Test]
        public void GivenNetworkWith2ObservationPoints()
        {
            // create simple network
            var network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            // add nodes and branches
            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(100, 0);

            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };
            branch1.Geometry = new LineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add cross-section
            CrossSectionHelper.AddCrossSection(branch1, 50.0d, 0.0d);

            //add observation point1
            var observationPoint1 = ObservationPoint.CreateDefault(branch1);
            const string observationPoint1Name = "OP1";
            observationPoint1.Name = observationPoint1Name;
            observationPoint1.Chainage = 90.0;
            branch1.BranchFeatures.Add(observationPoint1);
            //add observation point2
            var observationPoint2 = ObservationPoint.CreateDefault(branch1);
            const string observationPoint2Name = "OP2";
            observationPoint2.Name = observationPoint2Name;
            observationPoint2.Chainage = 10.0;
            branch1.BranchFeatures.Add(observationPoint2);
            
            
            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(network);

            // setup 1d flow model
            var t = new DateTime(2000, 1, 1);
            var flowModel1D = new WaterFlowModel1D
            {
                Network = network,
                NetworkDiscretization = networkDiscretization,
                StartTime = t,
                StopTime = t.AddMinutes(1),
                TimeStep = new TimeSpan(0, 0, 1),
                OutputTimeStep = new TimeSpan(0, 0, 1),
            };
            flowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 1);

            flowModel1D.ParameterSettings.FirstOrDefault(p => p.Name == "InterpolationType").Value = "Linear";
            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 0.0;
            flowModel1D.InitialConditions.DefaultValue = 0.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
            boundaryConditionInflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionInflow.WaterLevel = 5.0;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
            boundaryConditionOutflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 3.0;

            // set output coverages
            flowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
            flowModel1D.OutputSettings.BranchVelocity = AggregationOptions.Current;
            flowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Observations).AggregationOptions = AggregationOptions.Current;
            flowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.Observations).AggregationOptions = AggregationOptions.Current;
            flowModel1D.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).AggregationOptions = AggregationOptions.Current;
            flowModel1D.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.Observations).AggregationOptions = AggregationOptions.Current;

            RunModel(flowModel1D);

            // check water level
            var waterLevelGridPoints = flowModel1D.OutputWaterLevel;
            var valueFilterGP = new VariableValueFilter<DateTime>(waterLevelGridPoints.Arguments[0], flowModel1D.StopTime);
            var waterLevelsAtGridPoints = waterLevelGridPoints.GetValues<double>(valueFilterGP);
            var valueAtChainage90 = waterLevelsAtGridPoints[9]; // should be the same as the value at observation point 1
            var valueAtChainage10 = waterLevelsAtGridPoints[1]; // should be the same as the value at observation point 2

            // check values at observation point
            var waterLevel =
                flowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name.Contains("Water level (op)"));
            var valueFilter = new VariableValueFilter<DateTime>(waterLevel.Arguments[0], flowModel1D.StopTime);
            var waterLevelsAtObservationPoints = waterLevel.GetValues<double>(valueFilter);
            
            var indexOfWaterLevelOfObservationPoint1 = waterLevel.Features.IndexOf(waterLevel.Features.Cast<NetworkFeature>().First(f => f.Name == observationPoint1Name));
            var indexOfWaterLevelOfObservationPoint2 = waterLevel.Features.IndexOf(waterLevel.Features.Cast<NetworkFeature>().First(f => f.Name == observationPoint2Name));
            
            Assert.AreEqual(valueAtChainage90, waterLevelsAtObservationPoints[indexOfWaterLevelOfObservationPoint1], 0.00001d);
            Assert.AreEqual(valueAtChainage10, waterLevelsAtObservationPoints[indexOfWaterLevelOfObservationPoint2], 0.00001d);
        }

        private void RunModel(WaterFlowModel1D waterFlowModel1D)
        {
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(waterFlowModel1D);

            ActivityRunner.RunActivity(waterFlowModel1D);

            if (waterFlowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }

            Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
        }
    }

}