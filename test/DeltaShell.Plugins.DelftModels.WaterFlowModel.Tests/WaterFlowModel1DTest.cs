using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpTestsEx;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DTest));

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

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void GivenFmModelWhenConnectingToOutputThenCorrectRetentionFeatureIsAdded()
        {
            //Setup network
            //needs 2 nodes, branch, computational nodes, crosssection, retention and 2 boundary conditions

            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(0, 0) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(100, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());
            var yzCoordinates = new List<Coordinate>
            {
                new Coordinate(0.0, 1.0),
                new Coordinate(1.0, 0.0),
                new Coordinate(2.0, 0.0),
                new Coordinate(3.0, 1.0),
            };

            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch, 50.0, yzCoordinates, "mycs");

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = network,
                Geometry = new Point(5, 0),
                Chainage = 5,
            };

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch, 20.0);
            var retention = new Retention()
            {
                Name = "1",
                LongName = "retentionlongname",
                Branch = branch,
                Chainage = compositeBranchStructure.Chainage,
                Type = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = 1,
                StorageArea = 10,
                StreetLevel = 2,
                StreetStorageArea = 100
            };

            var pump = new Pump();
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            NetworkHelper.AddBranchFeatureToBranch(retention, branch, 20.0);
            network.Branches.Add(branch);
            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(network);

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network, NetworkDiscretization = networkDiscretization })
            {
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(1);

                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Retentions).AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var outputRetentionDataItem = waterFlowModel1D.DataItems.FirstOrDefault(
                    di => di.Name == "Water level (rt)"
                          && (di.Role & DataItemRole.Output) == DataItemRole.Output
                          && di.Value is IFunction
                          && ((IFunction)di.Value).Store is WaterFlowModel1DNetCdfFunctionStore);
                Assert.IsNotNull(outputRetentionDataItem);
                var outputRetentionFeatureCoverage = outputRetentionDataItem.Value as FeatureCoverage;
                Assert.IsNotNull(outputRetentionFeatureCoverage);
                Assert.That(outputRetentionFeatureCoverage.Features.Count, Is.EqualTo(1));
                Assert.That(outputRetentionFeatureCoverage.Features[0], Is.EqualTo(retention));
            }
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void GivenFmModelWhenConnectingToOutputThenCorrectPumpFeatureIsAdded()
        {
            //Setup network
            //needs 2 nodes, branch, computational nodes, crosssection, pump and 2 boundary conditions

            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(0,0)};
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(100, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());
            var yzCoordinates = new List<Coordinate>
            {
                new Coordinate(0.0, 1.0),
                new Coordinate(1.0, 0.0),
                new Coordinate(2.0, 0.0),
                new Coordinate(3.0, 1.0),
            };
            
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch, 50.0, yzCoordinates,"mycs");

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = network,
                Geometry = new Point(5, 0),
                Chainage = 5,
            };

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch, 20.0);
            var pump = new Pump();
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
           
            network.Branches.Add(branch);
            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);
            
            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(network);
            
            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network, NetworkDiscretization = networkDiscretization })
            {
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(1);
                

                // set boundary conditions
                var boundaryConditionInflow = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
                boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                boundaryConditionInflow.Data[waterFlowModel1D.StartTime] = 1000.0;
                boundaryConditionInflow.Data[waterFlowModel1D.StartTime.AddHours(1)] = 500.0;
                boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

                var boundaryConditionOutflow = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
                boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                boundaryConditionOutflow.WaterLevel = 0;

                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.SuctionSideLevel, ElementSet.Pumps).AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var outputPumpSuctionsideDataItem = waterFlowModel1D.DataItems.FirstOrDefault(
                    di => di.Name == "Suction side (p)"
                          && (di.Role & DataItemRole.Output) == DataItemRole.Output
                          && di.Value is IFunction
                          && ((IFunction)di.Value).Store is WaterFlowModel1DNetCdfFunctionStore);
                Assert.IsNotNull(outputPumpSuctionsideDataItem);
                var outputPumpSuctionSideFeatureCoverage = outputPumpSuctionsideDataItem.Value as FeatureCoverage;
                Assert.IsNotNull(outputPumpSuctionSideFeatureCoverage);
                Assert.That(outputPumpSuctionSideFeatureCoverage.Features.Count, Is.EqualTo(1));
                Assert.That(outputPumpSuctionSideFeatureCoverage.Features[0], Is.EqualTo(pump));
            }
        }

        [Test]
        public void VerifyChangingDispersionFormulationTypeCachesF3AndF4Values()
        {
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());
            network.Branches.Add(branch);

            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(network);

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network, NetworkDiscretization = networkDiscretization })
            {
                waterFlowModel1D.UseSalt = true;
                waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;
                var startOfBranch = new NetworkLocation(branch, 0.0);
                var endOfBranch = new NetworkLocation(branch, 100.0);

                // Action: Set values. 
                var dispersionCoverage = waterFlowModel1D.DispersionCoverage;
                dispersionCoverage[startOfBranch] = 4.0;
                dispersionCoverage[endOfBranch] = 1.0;

                var dispersionF3Coverage = waterFlowModel1D.DispersionF3Coverage;
                dispersionF3Coverage[startOfBranch] = 2.0;
                dispersionF3Coverage[endOfBranch] = 3.0;

                var dispersionF4Coverage = waterFlowModel1D.DispersionF4Coverage;
                dispersionF4Coverage[startOfBranch] = 5.0;
                dispersionF4Coverage[endOfBranch] = 7.0;

                // Verify: Get values. 
                var f1Values = waterFlowModel1D.DispersionCoverage.Components.FirstOrDefault();
                Assert.That(f1Values, Is.Not.Null);
                Assert.That(f1Values.Values.Count, Is.EqualTo(2));
                Assert.That(f1Values.Values[0], Is.EqualTo(4));
                Assert.That(f1Values.Values[1], Is.EqualTo(1));
                var f3Values = waterFlowModel1D.DispersionF3Coverage.Components.FirstOrDefault();
                Assert.That(f3Values, Is.Not.Null);

                var f4Values = waterFlowModel1D.DispersionF4Coverage.Components.FirstOrDefault();
                Assert.That(f4Values, Is.Not.Null);

                // Action: Unset dispersion formulation type TH. 
                waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.Constant;

                // Verify: F1 is still there, F3 and F4 are null. 
                var dispersionCoefficient = waterFlowModel1D.DispersionCoverage.Components.FirstOrDefault();
                Assert.That(dispersionCoefficient, Is.Not.Null);
                Assert.That(dispersionCoefficient.Values.Count, Is.EqualTo(2));
                Assert.That(dispersionCoefficient.Values[0], Is.EqualTo(4));
                Assert.That(dispersionCoefficient.Values[1], Is.EqualTo(1));
                Assert.That(waterFlowModel1D.DispersionF3Coverage, Is.Null);
                Assert.That(waterFlowModel1D.DispersionF4Coverage, Is.Null);

                // Action: Set dispersion to Savenije
                waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;

                // Verify: F1 is still there, F3 and F4 contain cached values. 
                f1Values = waterFlowModel1D.DispersionCoverage.Components.FirstOrDefault();
                Assert.That(f1Values, Is.Not.Null);
                Assert.That(f1Values.Values.Count, Is.EqualTo(2));
                Assert.That(f1Values.Values[0], Is.EqualTo(4));
                Assert.That(f1Values.Values[1], Is.EqualTo(1));
                f3Values = waterFlowModel1D.DispersionF3Coverage.Components.FirstOrDefault();
                Assert.That(f3Values, Is.Not.Null);
                Assert.That(f3Values.Values[0], Is.EqualTo(2));   // Cached value
                Assert.That(f3Values.Values[1], Is.EqualTo(3));   // Cached value 
                f4Values = waterFlowModel1D.DispersionF4Coverage.Components.FirstOrDefault();
                Assert.That(f4Values, Is.Not.Null);
                Assert.That(f4Values.Values[0], Is.EqualTo(5));   // Cached value
                Assert.That(f4Values.Values[1], Is.EqualTo(7));   // Cached value 
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithSimpleNetwork()
        {
            // create simplest network
            var network = new HydroNetwork();

            // add nodes and branches
            var startCoordinate = new Coordinate(0, 0);
            var midCoordinate = new Coordinate(100, 0);
            var endCoordinate = new Coordinate(100, 250);


            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(midCoordinate) };
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   midCoordinate
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            var branch2 = new Channel("branch2", node2, node3);
            vertices = new List<Coordinate>
                           {
                                   midCoordinate,
                                   endCoordinate
                           };
            branch2.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            // add cross-sections
            var chainage1 = 50.0;
            var chainage2 = 75.0;

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(100.0, 0.0),
                                        new Coordinate(150.0, -10.0),
                                        new Coordinate(300.0, -10.0),
                                        new Coordinate(350.0, 0.0),
                                        new Coordinate(500.0, 0.0)
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, chainage1, yzCoordinates);
            var cs2 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, chainage2, yzCoordinates);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // add discretization
            Discretization networkDiscretization = WaterFlowModel1DTestHelper.GetNetworkDiscretization(network);

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { NetworkDiscretization = networkDiscretization })
            {


                //flowModel1D.RunInSeparateProcess = true;

                //use a fixed startdate for comparison.
                var t = new DateTime(2000, 1, 1);

                waterFlowModel1D.StartTime = t;
                waterFlowModel1D.StopTime = t.AddMinutes(5);
                waterFlowModel1D.TimeStep = new TimeSpan(0, 0, 1);
                waterFlowModel1D.OutputTimeStep = new TimeSpan(0, 0, 1);

                // set network
                waterFlowModel1D.Network = network;

                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                // set boundary conditions
                var boundaryConditionInflow = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
                boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                boundaryConditionInflow.Data[t] = 1.0;
                boundaryConditionInflow.Data[t.AddSeconds(30)] = 1.0;
                boundaryConditionInflow.Data[t.AddSeconds(60)] = 1.5;
                boundaryConditionInflow.Data[t.AddSeconds(120)] = 1.0;
                boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;
                boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;
                boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;



                var boundaryConditionOutflow = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == node3);
                boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                boundaryConditionOutflow.Data[t] = 0.1;
                boundaryConditionOutflow.Data[t.AddSeconds(30)] = 0.1;
                boundaryConditionOutflow.Data[t.AddSeconds(60)] = 0.2;
                boundaryConditionOutflow.Data[t.AddSeconds(120)] = 0.3;
                boundaryConditionOutflow.Data[t.AddSeconds(180)] = 0.1;
                boundaryConditionOutflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

                waterFlowModel1D.Initialize();

                Assert.IsTrue(File.Exists(WaterFlowModel1D.TemplateDataZipFile));

                Assert.AreEqual(ActivityStatus.Initialized, waterFlowModel1D.Status,
                            "Model should be in initialized state after it is created.");
                
                //cleanup for dimr...
                waterFlowModel1D.Finish();
                waterFlowModel1D.Cleanup();

                t = DateTime.Now;

                int timeStepCount = 0;
                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                // expected number of timesteps is 10
                Assert.AreEqual(300, timeStepCount);

                log.DebugFormat("It took {0} sec to run waterFlowModel1D", (DateTime.Now - t).TotalSeconds);

                Assert.IsTrue(waterFlowModel1D.CurrentTime >= waterFlowModel1D.StopTime);
            }

            // TODO: add asserts on output values, migrate it to FitNesse when it is connected to TeamCity
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestSimpleModelWithMultipleCrossSection()
        {
            // create simplest network
            var network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(100, 0);

            // add nodes and branches
            var node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            var node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2);

            network.Branches.Add(branch1);

            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };

            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            // add 2 cross-sections
            CrossSectionHelper.AddCrossSection(branch1, 10, -10);

            // note if cross sections are identical (or only 1 cs) run is succesfull
            CrossSectionHelper.AddCrossSection(branch1, 90, -15);

            branch1.CrossSections.First().Name = "cs";

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, branch1, 0, true, 0.5, true, false, false, -1);

            // cell boundaries at cross section
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);


            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = networkDiscretization,
                StartTime = startTime,
                StopTime = startTime.AddMinutes(5),
                TimeStep = new TimeSpan(0, 0, 30),
                OutputTimeStep = new TimeSpan(0, 0, 30),
                Network = network
            })
            {
                waterFlowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
                WaterFlowModel1DTestHelper.AddFlowTimeBoundaryCondition(node1, waterFlowModel1D, startTime);
                WaterFlowModel1DTestHelper.AddFlowDepthBoundary(node2, waterFlowModel1D, startTime);

                int timeStepCount = 0;
                var stepTimes = new List<DateTime>();
                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        stepTimes.Add(waterFlowModel1D.CurrentTime);
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                foreach (var stepTime in stepTimes)
                {
                    // get values from waterFlowModel1D for the last time step
                    IList<double> values = waterFlowModel1D.OutputDepth
                        .GetValues<double>(new VariableValueFilter<DateTime>(waterFlowModel1D.OutputDepth.Arguments[0],
                                                                             stepTime));
                    Assert.Greater(values.Count, 0);
                    log.Debug(new List<double>(values).ToArray());
                }

                // expected number of timesteps is 10
                Assert.AreEqual(10, timeStepCount);
            }
        }

        /// <summary>
        ///  Run the FlowModel1DNameDemoNetwork and check if it has actually calculated values.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunDemoModelTest()
        {
            // use a valid network for the calculation
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                int timeStepCount = 0;

                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                Assert.AreEqual(10, timeStepCount, "expected number of timesteps should be 10");
                Assert.AreEqual(11, waterFlowModel1D.OutputFlow.Arguments[0].Values.Count,
                                "expected number of results is 11 (timesteps)");
            }
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestSobekLogIsRetrievedAfterModelRun()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                ActivityRunner.RunActivity(waterFlowModel1D);

                var sobekLogDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1D.SobekLogfileDataItemTag);
                Assert.NotNull(sobekLogDataItem, "SobekLog not retrieved after model run, check WaterFlowModel1D.SobekLogfileDataItemTag");
                Assert.NotNull(sobekLogDataItem.Value, "SobekLog not retrieved after model run, check WaterFlowModel1D.SobekLogfileDataItemTag");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestWarningGivenIfSobekLogFileNotFound()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var outputDirectory = FileUtils.CreateTempDirectory();
                const string SobekLogFileName = "sobek.log";
                var sobekLogFilePath = Path.Combine(outputDirectory, SobekLogFileName);

                TestHelper.AssertAtLeastOneLogMessagesContains(() => 
                    TypeUtils.CallPrivateMethod(waterFlowModel1D, "ReadSobekLogFile", new[] { outputDirectory }),
                    string.Format(WaterFlowModel.Properties.Resources.WaterFlowModel1D_ReadSobekLogFile_Could_not_find_log_file___0__at_expected_path___1_, SobekLogFileName, sobekLogFilePath)
                );
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunDemoModelRemoteTest()
        {
            // use a valid network for the calculation
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                int timeStepCount = 0;

                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                Assert.AreEqual(10, timeStepCount, "expected number of timesteps should be 10");
                Assert.AreEqual(11, waterFlowModel1D.OutputFlow.Arguments[0].Values.Count,
                                "expected number of results is 11 (timesteps)");
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunSimpleModelAddBranchAndLateralRunAgainTools20417()
        {
            using (var flow = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // run once
                ActivityRunner.RunActivity(flow);
                Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);

                // add branch:
                var coordinate = new Coordinate(150, 150);

                var node = new HydroNode("new") { Geometry = new Point(coordinate)};
                flow.Network.Nodes.Add(node);
                var newChannel = new Channel(flow.Network.Nodes[2], node)
                    {
                        Geometry = new LineString(new[] {new Coordinate(100, 150), coordinate})
                    };
                flow.Network.Branches.Add(newChannel);
                
                // add lateral:
                var lateral = new LateralSource {Branch = newChannel, Geometry = newChannel.Geometry.Centroid};
                NetworkHelper.UpdateBranchFeatureChainageFromGeometry(lateral);
                newChannel.BranchFeatures.Add(lateral);

                // add cross-section:
                CrossSectionHelper.AddCrossSection(newChannel, 10, -10);

                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(flow.Network);

                // fix comp grid:
                HydroNetworkHelper.GenerateDiscretization(flow.NetworkDiscretization, true, false, 100.0, false, 0.0,
                                                          true, false, true, 100.0);

                // run again
                ActivityRunner.RunActivity(flow);
                Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);
            }
        }

        /// <summary>
        /// Tests the number of locations in the output coverage of the energy head
        /// </summary>
        [Test]
        public void CreateEnergyHeadDiscretizationForDemoModel()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                waterFlowModel1D.Initialize();
                var energyHeadDiscretization =
                    WaterFlowModel1D.CreateEnergyHeadDiscretization(waterFlowModel1D.NetworkDiscretization);
                var segments = waterFlowModel1D.NetworkDiscretization.Segments.Values;
                var energyPoints = energyHeadDiscretization.Locations.Values;
                Assert.AreEqual(3 * segments.Count, energyPoints.Count,
                                "incorrect number of energy head coverage grid points");
            }
        }

        /// <summary>
        /// Run the FlowModel1DNameDemoNetwork and add weir. The expected discharge at the weir should equal
        /// the discharge at the weir location. Data in 2 coverages overlaps.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunDemoModelWithWeirTest()
        {
            // Create waterFlowModel1D
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // Add weir to branch 1
                var weirBranch = waterFlowModel1D.Network.Branches[1];
                var weirOfset = weirBranch.Length / 2;
                var weir = new Weir("weir") { Chainage = weirOfset, CrestLevel = 0.1 };

                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, weirBranch);

                // Add structures discharge to output (using OutputSettings) and set timestep to 2 * GridOutputTimeStep
                var settings = waterFlowModel1D.OutputSettings;

                settings.StructureOutputTimeStep = waterFlowModel1D.OutputTimeStep + waterFlowModel1D.OutputTimeStep;
                settings.GetEngineParameter(QuantityType.Discharge, ElementSet.Structures).AggregationOptions =
                    AggregationOptions.Current;
                waterFlowModel1D.Initialize();

                var timeStepCount = 0;
                var stepTimes = new List<DateTime>();
                while (waterFlowModel1D.Status != ActivityStatus.Done)
                {
                    // Execute waterFlowModel1D
                    log.DebugFormat("Executing waterFlowModel1D, time = {0}", waterFlowModel1D.CurrentTime);
                    stepTimes.Add(waterFlowModel1D.CurrentTime);
                    waterFlowModel1D.Execute();
                    
                    timeStepCount++;

                    if (waterFlowModel1D.Status == ActivityStatus.Failed)
                    {
                        Assert.Fail("Model run has failed");
                    }
                }

                waterFlowModel1D.Finish();
                waterFlowModel1D.Cleanup();

                // Find output coverages
                var dischargeAtStaggeredGrid =
                    (INetworkCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.BranchDischarge));

                var dischargeAtStructures =
                    (IFeatureCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.StructureDischarge));

                var dischargesAtWeir = new List<double>();
                var dischargesAtWeirLocation = new List<double>();
                var networkLocation = new NetworkLocation(weirBranch, weirOfset);

                foreach (var currentTime in stepTimes)
                {
                    // Add values to dischargesAtWeir & dischargesAtWeirLocation lists
                    var valueFilter = new VariableValueFilter<DateTime>(dischargeAtStructures.Arguments[0], currentTime);
                    var dischargeAtStucturesValues = dischargeAtStructures.GetValues<double>(valueFilter);

                    if (dischargeAtStucturesValues.Count != 0)
                    {
                        dischargesAtWeir.Add(dischargeAtStucturesValues[0]);
                    }

                    double valueAtLocation = dischargeAtStaggeredGrid.Evaluate(currentTime, networkLocation);
                    dischargesAtWeirLocation.Add(valueAtLocation);
                }


                for (var i = 0; i < dischargesAtWeir.Count; i++)
                {
                    Assert.AreEqual(dischargesAtWeirLocation[i], dischargesAtWeir[i], 1.0e-4);
                }

                Assert.AreEqual(10, timeStepCount);
                Assert.AreEqual(11, waterFlowModel1D.OutputFlow.Arguments[0].Values.Count,
                                "expected number of results is 11 (timesteps)");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunModelAndSetPreviousOutputAsInitialCondition()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                waterFlowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
                ActivityRunner.RunActivity(waterFlowModel1D);

                var sampleTime = waterFlowModel1D.StopTime - waterFlowModel1D.OutputTimeStep;
                waterFlowModel1D.SetInitialConditionsFromPreviousOutput(sampleTime);

                var newValue = waterFlowModel1D.InitialFlow.Components[0].Values[0];

                var filteredOutputFlow = waterFlowModel1D.OutputFlow.FilterTime(sampleTime);
                var outputValue = filteredOutputFlow.Components[0].Values[0];

                Assert.AreEqual(outputValue, newValue);
                Assert.AreEqual(waterFlowModel1D.OutputFlow.Locations.Values.Count, waterFlowModel1D.InitialFlow.Locations.Values.Count);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunModelWithNegativeDischargeLateralSourceHasCorrectOutputDischargeWithJustTwoGridpoints()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {

                //enable discharge output at laterals
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions
                    = AggregationOptions.Current;

                //set a nice output timestep
                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = waterFlowModel1D.TimeStep;

                waterFlowModel1D.BoundaryConditions[0].DataType = Model1DBoundaryNodeDataType.FlowConstant;
                waterFlowModel1D.BoundaryConditions[0].Flow = 50.0;

                var branch = waterFlowModel1D.Network.Branches[0];
                waterFlowModel1D.NetworkDiscretization.Clear();
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch, 0)] = 0.0;
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch, branch.Length)] = 0.0;
                var branch2 = waterFlowModel1D.Network.Branches[1];
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch2, 0)] = 0.0;
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch2, branch2.Length / 2.0)] = 0.0;
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch2, branch2.Length)] = 0.0;

                //add lateral source with negative discharge
                var lateralSource = new LateralSource();
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, branch, 15.0);
                var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                const double negativeDischarge = -15.0;
                flowTimeSeries[waterFlowModel1D.StartTime] = negativeDischarge;
                flowTimeSeries[waterFlowModel1D.StopTime] = negativeDischarge;
                waterFlowModel1D.LateralSourceData.First(d => d.Feature == lateralSource).Data = flowTimeSeries;

                RunModel(waterFlowModel1D);

                var dischargeAtLateral = waterFlowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name == Model1DParameterNames.LateralActualDischarge);
                foreach (var value in dischargeAtLateral.Components[0].Values.OfType<double>().Skip(1))
                //not the first timestep
                {
                    Assert.AreEqual(negativeDischarge, value);
                }

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)] //TOOLS-5082
        [Ignore]
        public void RunModelWithNegativeDischargeLateralSourceHasCorrectOutputDischargeWhenNearSourceNode()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                //enable discharge output at laterals
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions
                    = AggregationOptions.Current;

                //set a nice output timestep
                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = waterFlowModel1D.TimeStep;

                waterFlowModel1D.BoundaryConditions[0].DataType = Model1DBoundaryNodeDataType.FlowConstant;
                waterFlowModel1D.BoundaryConditions[0].Flow = 50.0;

                var branch = waterFlowModel1D.Network.Branches[0];

                //add lateral source with negative discharge
                var lateralSource = new LateralSource();
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, branch, 0.5); //add it <10% of 2nd comp grid point
                var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                const double negativeDischarge = -15.0;
                flowTimeSeries[waterFlowModel1D.StartTime] = negativeDischarge;
                flowTimeSeries[waterFlowModel1D.StopTime] = negativeDischarge;
                waterFlowModel1D.LateralSourceData.First(d => d.Feature == lateralSource).Data = flowTimeSeries;

                RunModel(waterFlowModel1D);

                var dischargeAtLateral = waterFlowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name == Model1DParameterNames.LateralActualDischarge);
                foreach (var value in dischargeAtLateral.Components[0].Values.OfType<double>().Skip(1))
                //not the first timestep
                {
                    Assert.AreEqual(negativeDischarge, value);
                }

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration), Category(TestCategory.WorkInProgress)] //TOOLS-5482
        public void LateralNearOutgoingQNodeWithoutGridPointInBetweenDoesNothing()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                var network = HydroNetworkHelper.GetSnakeHydroNetwork(new[] { new Point(0, 0), new Point(0, 100) });
                waterFlowModel1D.Network = network;

                var branch1 = waterFlowModel1D.Network.Branches[0];

                NetworkHelper.AddBranchFeatureToBranch(CrossSection.CreateDefault(CrossSectionType.YZ, branch1, 0), branch1, 50);
                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                waterFlowModel1D.NetworkDiscretization.Clear();
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch1, 0)] = 0.0;
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch1, 50)] = 0.0;
                //waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch1, 75)] = 0.0; //without this gridpoint, the lateral stops working
                waterFlowModel1D.NetworkDiscretization[new NetworkLocation(branch1, 100)] = 0.0;

                //set a nice output timestep
                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = waterFlowModel1D.TimeStep;

                const double initialDepth = 3.0;
                waterFlowModel1D.DefaultInitialDepth = initialDepth;

                waterFlowModel1D.BoundaryConditions[1].DataType = Model1DBoundaryNodeDataType.FlowConstant;
                waterFlowModel1D.BoundaryConditions[1].Flow = 0.0;

                //add lateral source with discharge
                var lateralSource = new LateralSource();
                const int lateralChainage = 70;
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, branch1, lateralChainage);
                var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                const double discharge = 5.0;
                flowTimeSeries[waterFlowModel1D.StartTime] = discharge;
                flowTimeSeries[waterFlowModel1D.StopTime] = discharge;
                waterFlowModel1D.LateralSourceData.First(d => d.Feature == lateralSource).Data = flowTimeSeries;

                waterFlowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
                RunModel(waterFlowModel1D);

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var waterDepth = waterFlowModel1D.OutputDepth;
                var waterDepthAtCenter = waterDepth.GetTimeSeries(new NetworkLocation(branch1, 50));
                var waterDepthValues = waterDepthAtCenter.Components[0].Values.OfType<double>().ToList();

                Assert.AreEqual(initialDepth, waterDepthValues[0]);
                for (int i = 1; i < waterDepthValues.Count; i++) //water depth should be increasing continiously
                {
                    Assert.Greater(waterDepthValues[i], waterDepthValues[i - 1] + 0.5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore]
        public void RunModelWithNegativeDischargeLateralSourceHasCorrectOutputDischarge()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {

                //enable discharge output at laterals
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions
                    = AggregationOptions.Current;

                //set a nice output timestep
                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = waterFlowModel1D.TimeStep;

                waterFlowModel1D.BoundaryConditions[0].DataType = Model1DBoundaryNodeDataType.FlowConstant;
                waterFlowModel1D.BoundaryConditions[0].Flow = 50.0;

                //add lateral source with negative discharge
                var lateralSource = new LateralSource();
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, waterFlowModel1D.Network.Branches[0], 10.0);
                var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                const double negativeDischarge = -15.0;
                flowTimeSeries[waterFlowModel1D.StartTime] = negativeDischarge;
                flowTimeSeries[waterFlowModel1D.StopTime] = negativeDischarge;
                waterFlowModel1D.LateralSourceData.First(d => d.Feature == lateralSource).Data = flowTimeSeries;

                RunModel(waterFlowModel1D);

                var dischargeAtLateral = waterFlowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name == Model1DParameterNames.LateralActualDischarge);
                foreach (var value in dischargeAtLateral.Components[0].Values.OfType<double>().Skip(1))
                //not the first timestep
                {
                    Assert.AreEqual(negativeDischarge, value);
                }

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetCulvertInputChildDataItems()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {

                var branch = waterFlowModel1D.Network.Branches[0];
                var culvert = Culvert.CreateDefault();
                culvert.Geometry = new Point(branch.Geometry.Coordinates[0]);
                NetworkHelper.AddBranchFeatureToBranch(culvert, branch, 32.0);

                var culvertDataItems =
                    waterFlowModel1D.GetChildDataItems(culvert)
                    .Where(di => ((di.Role & DataItemRole.Input) == DataItemRole.Input))
                    .ToList();

                Assert.AreEqual(0, culvertDataItems.Count);
                
                culvert.IsGated = true;
                
                culvertDataItems =
                    waterFlowModel1D.GetChildDataItems(culvert)
                                    .Where(di => ((di.Role & DataItemRole.Input) == DataItemRole.Input))
                                    .ToList();

                Assert.AreEqual(1, culvertDataItems.Count);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunModelAndShowResultsOnMapUsingFeatureCoverage()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                //waterFlowModel1D.RunInSeparateProcess = true;

                RunModel(waterFlowModel1D);

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                //todo : second part of this test fails (in release mode!!!!). Split test in two parts to solve 
                //todo this. For now second part is ignored
                //INetworkCoverageLayer outputDepthLayer = new NetworkCoverageLayer();
                //var timeFilter = new VariableValueFilter(waterFlowModel1D.OutputDepth.Time, waterFlowModel1D.OutputDepth.Time.Values[0]);
                //INetworkCoverage filteredCoverage = (INetworkCoverage) waterFlowModel1D.OutputDepth.Filter(timeFilter);
                //outputDepthLayer.NetworkCoverage = filteredCoverage;

                //var map = new Map();
                //map.Layers.Add(outputDepthLayer);

                //// show time tracker and add register theme dialog on right-click
                //// show time selection slider
                //var timeSelector = new TrackBar {Dock = DockStyle.Bottom};
                //timeSelector.SetRange(0, waterFlowModel1D.OutputDepth.Time.Values.Count - 1);
                //var mapTestHelper = new MapTestHelper();
                //mapTestHelper.MapControl.Controls.Add(timeSelector);

                //var timeLabel = new Label { Dock = DockStyle.Right };
                //mapTestHelper.Controls.Add(timeLabel);

                //timeSelector.ValueChanged += delegate
                //{
                //    timeFilter.Values[0] = waterFlowModel1D.OutputDepth.Time.Values[timeSelector.Value];
                //    outputDepthLayer.RenderRequired = true;
                //    mapTestHelper.MapControl.Update();
                //    timeLabel.Text = waterFlowModel1D.OutputDepth.Time.Values[timeSelector.Value].ToString();
                //};

                //mapTestHelper.ShowMap(map);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunModelWithObservationPointShouldSetCoordinateSystemOnOutputWaterLevel()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                if (Map.CoordinateSystemFactory == null)
                {
                    Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
                }

                var networkCoordinateSystemRDNew = Map.CoordinateSystemFactory.CreateFromEPSG(28992);

                waterFlowModel1D.Network.CoordinateSystem = networkCoordinateSystemRDNew;
                waterFlowModel1D.Network.Branches.First().BranchFeatures.Add(new ObservationPoint() {Name = "myObservationPoint"});
                RunModel(waterFlowModel1D);

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var outputWaterLevelDataItem = waterFlowModel1D.DataItems.FirstOrDefault(
                    di => di.Name == "Water level (op)"
                          && (di.Role & DataItemRole.Output) == DataItemRole.Output
                          && di.Value is IFunction
                          && ((IFunction) di.Value).Store is WaterFlowModel1DNetCdfFunctionStore);
                Assert.IsNotNull(outputWaterLevelDataItem);
                var outputWaterLevelFeatureCoverage = outputWaterLevelDataItem.Value as FeatureCoverage;
                Assert.IsNotNull(outputWaterLevelFeatureCoverage);

                Assert.IsTrue(outputWaterLevelFeatureCoverage.CoordinateSystem.EqualsTo(networkCoordinateSystemRDNew));
                var networkCoordinateSystemRDOld = new OgrCoordinateSystemFactory().CreateFromEPSG(28991);
                waterFlowModel1D.Network.CoordinateSystem = networkCoordinateSystemRDOld;
                Assert.IsTrue(outputWaterLevelFeatureCoverage.CoordinateSystem.EqualsTo(networkCoordinateSystemRDOld));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunDemoModelWithConstantHOnNodeThatIsConnectedToMultipleBranches()
        {
            // Use a valid network for the calculation
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // Adjust the boundary conditions so that a constant H resides on a node that is connected to multiple branches
                waterFlowModel1D.BoundaryConditions[1].DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;

                int timeStepCount = 0;

                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                Assert.AreEqual(10, timeStepCount, "expected number of timesteps should be 10");
                Assert.AreEqual(11, waterFlowModel1D.OutputFlow.Arguments[0].Values.Count,
                                "expected number of results is 11 (timesteps)");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunDemoModelWithHTimeSeriesOnNodeThatIsConnectedToMultipleBranches()
        {
            // Use a valid network for the calculation
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // Adjust the boundary conditions so that a H time series resides on a node that is connected to multiple branches
                waterFlowModel1D.BoundaryConditions[1].DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;

                int timeStepCount = 0;

                waterFlowModel1D.StatusChanged += (sender, args) =>
                {
                    if (waterFlowModel1D.Status == ActivityStatus.Executed || waterFlowModel1D.Status == ActivityStatus.Done)
                    {
                        timeStepCount++;
                    }
                };

                ActivityRunner.RunActivity(waterFlowModel1D);

                if (waterFlowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }

                Assert.AreEqual(10, timeStepCount, "expected number of timesteps should be 10");
                Assert.AreEqual(11, waterFlowModel1D.OutputFlow.Arguments[0].Values.Count,
                                "expected number of results is 11 (timesteps)");
            }
        }

        private void RunModel(WaterFlowModel1D waterFlowModel1D, bool replaceStore = true)
        {
            if (replaceStore)
            {
                WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(waterFlowModel1D);
            }

            ActivityRunner.RunActivity(waterFlowModel1D);

            if (waterFlowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }

            Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithLateralSources()
        {
            // create L shaped network
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));

            var branch1 = network.Channels.First();
            var branch2 = network.Channels.ElementAt(1);

            AddCrossSection(branch1, 40, -10);
            AddCrossSection(branch1, 60, -10);
            AddCrossSection(branch2, 50, -10);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // setup 1d flow waterFlowModel1D
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            using (var waterFlowModel1D = new WaterFlowModel1D
                                  {
                                      NetworkDiscretization = GetNetworkDiscretization(network),
                                      StartTime = startTime,
                                      StopTime = startTime.AddMinutes(5),
                                      TimeStep = new TimeSpan(0, 0, 1),
                                      OutputTimeStep = new TimeSpan(0, 0, 1),
                                      Network = network
                                  })
            {

                // add lateral sources data to the 2 branches
                var lateralSourceOnBranch2 = new LateralSource { Name = "branch2", Chainage = 150 };
                NetworkHelper.AddBranchFeatureToBranch(lateralSourceOnBranch2, branch2, lateralSourceOnBranch2.Chainage);

                var lateralSourceOnBranch1 = new LateralSource { Name = "branch1", Chainage = 150 };
                NetworkHelper.AddBranchFeatureToBranch(lateralSourceOnBranch1, branch1, lateralSourceOnBranch1.Chainage);

                //set constant discharges for the sources
                const double branch1Discharge = 11.0;
                const double branch2Discharge = 22.0;

                SetConstantFlowDischarge(waterFlowModel1D, lateralSourceOnBranch2, branch2Discharge);
                SetConstantFlowDischarge(waterFlowModel1D, lateralSourceOnBranch1, branch1Discharge);

                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 1);
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals,DataItemRole.Output).
                    AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var lateralDischargeCoverage =
                    (FeatureCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.LateralActualDischarge));
                //check the 2nd timestep should be equal to what is set on the lateral..
                Assert.AreEqual(branch1Discharge,
                                lateralDischargeCoverage[startTime.AddSeconds(1), lateralSourceOnBranch1]);
                Assert.AreEqual(branch2Discharge,
                                lateralDischargeCoverage[startTime.AddSeconds(1), lateralSourceOnBranch2]);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)] //TOOLS-4869
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithLateralSourcesFirstTimeStepShouldNotBeZero()
        {
            // create L shaped network
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));

            var branch1 = network.Channels.First();
            var branch2 = network.Channels.ElementAt(1);

            AddCrossSection(branch1, 40, -10);
            AddCrossSection(branch1, 60, -10);
            AddCrossSection(branch2, 50, -10);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // setup 1d flow waterFlowModel1D
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = GetNetworkDiscretization(network),
                StartTime = startTime,
                StopTime = startTime.AddMinutes(5),
                TimeStep = new TimeSpan(0, 0, 1),
                OutputTimeStep = new TimeSpan(0, 0, 1),
                Network = network
            })
            {
                // add lateral source data to a branch
                var lateralSourceOnBranch1 = new LateralSource { Name = "branch1", Chainage = 150 };
                NetworkHelper.AddBranchFeatureToBranch(lateralSourceOnBranch1, branch1, lateralSourceOnBranch1.Chainage);

                //set constant discharge for the source
                const double branch1Discharge = 1.0;

                SetConstantFlowDischarge(waterFlowModel1D, lateralSourceOnBranch1, branch1Discharge);

                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 1);
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals, DataItemRole.Output).
                    AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var lateralDischargeCoverage =
                    (FeatureCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.LateralActualDischarge));
                //check the 2nd timestep, should be equal to what is set on the lateral
                Assert.AreEqual(branch1Discharge,
                                lateralDischargeCoverage[startTime.AddSeconds(1), lateralSourceOnBranch1]);
                //check the 1st timestep, should be equal to what is set on the lateral as well
                Assert.AreEqual(branch1Discharge, lateralDischargeCoverage[startTime, lateralSourceOnBranch1]);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithLateralSourceCheckOutputSettingsLateralSourceWaterLevel()
        {
            // create L shaped network
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));

            var branch1 = network.Channels.First();
            var branch2 = network.Channels.ElementAt(1);

            AddCrossSection(branch1, 40, -10);
            AddCrossSection(branch1, 60, -10);
            AddCrossSection(branch2, 50, -10);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // setup 1d flow waterFlowModel1D
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = GetNetworkDiscretization(network),
                StartTime = startTime,
                StopTime = startTime.AddMinutes(5),
                TimeStep = new TimeSpan(0, 0, 1),
                OutputTimeStep = new TimeSpan(0, 0, 1),
                Network = network
            })
            {
                // add lateral source data to branch 2
                var lateralSourceOnBranch = new LateralSource { Name = "lateralSource", Chainage = 150 };
                NetworkHelper.AddBranchFeatureToBranch(lateralSourceOnBranch, branch2, lateralSourceOnBranch.Chainage);

                //set constant discharges for the source
                const double branchDischarge = 22.0;

                SetConstantFlowDischarge(waterFlowModel1D, lateralSourceOnBranch, branchDischarge);

                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 1);
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals, DataItemRole.Output).
                    AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var lateralWaterLevelCoverage =
                    (FeatureCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.LateralWaterLevel));

                Assert.AreEqual(1, lateralWaterLevelCoverage.Features.Count);
                Assert.AreNotEqual(0, lateralWaterLevelCoverage.Components[0].Values.OfType<double>().Sum());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void ExecuteWithDiffuseLateralSourceCheckLateralSourceWaterLevel()
        {
            // create L shaped network
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));

            var branch1 = network.Channels.First();
            var branch2 = network.Channels.ElementAt(1);

            AddCrossSection(branch1, 40, -10);
            AddCrossSection(branch1, 60, -5);
            AddCrossSection(branch2, 50, -10);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // setup 1d flow waterFlowModel1D
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = GetNetworkDiscretization(network),
                StartTime = startTime,
                StopTime = startTime.AddMinutes(5),
                TimeStep = new TimeSpan(0, 0, 1),
                OutputTimeStep = new TimeSpan(0, 0, 1),
                Network = network
            })
            {
                // add lateral source data to branch 1
                var lateralSourceOnBranch = new LateralSource
                                                {
                                                    Name = "lateralSource",
                                                    Chainage = 0,
                                                    Length = branch1.Length
                                                };
                NetworkHelper.AddBranchFeatureToBranch(lateralSourceOnBranch, branch1, lateralSourceOnBranch.Chainage);

                //set constant discharges for the source
                const double branchDischarge = 222.0;

                SetConstantFlowDischarge(waterFlowModel1D, lateralSourceOnBranch, branchDischarge);

                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 1d;
                waterFlowModel1D.InitialConditions.DefaultValue = 1d;

                waterFlowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 0, 1);
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals, DataItemRole.Output).
                    AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D);

                var lateralWaterLevelCoverage =
                    (FeatureCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.LateralWaterLevel));
                var gridpointsWaterLevelCoverage =
                    (NetworkCoverage)
                    waterFlowModel1D.OutputFunctions.First(
                        c => c.Name.StartsWith(Model1DParameterNames.LocationWaterLevel));

                var calculationPointsBranch1 =
                    gridpointsWaterLevelCoverage.Locations.Values.Where(l => l.Branch == branch1).ToArray();

                Assert.AreEqual(1, lateralWaterLevelCoverage.Features.Count);
                Assert.Greater(calculationPointsBranch1.Length, 1); //mean of one makes no sense for this test

                //check foreach timestep if h lateral is mean h of calculation points

                foreach (var time in lateralWaterLevelCoverage.Time.Values)
                {
                    var lateralH =
                        lateralWaterLevelCoverage.GetValues<double>(
                            new VariableValueFilter<DateTime>(lateralWaterLevelCoverage.Time, new[] { time }))[0];
                    var valuesHGridpoints = gridpointsWaterLevelCoverage.GetValues<double>(
                        new VariableValueFilter<DateTime>(gridpointsWaterLevelCoverage.Time, new[] { time }),
                        new VariableValueFilter<INetworkLocation>(gridpointsWaterLevelCoverage.Locations,
                                                                  calculationPointsBranch1)
                        );
                    var sumValuesHGridpoints = valuesHGridpoints.Sum();
                    Assert.AreEqual(lateralH, sumValuesHGridpoints / calculationPointsBranch1.Length, 0.00000001);
                }
            }
        }

        private static void SetConstantFlowDischarge(WaterFlowModel1D waterFlowModel1D, LateralSource lateralSource, double discharge)
        {
            waterFlowModel1D.LateralSourceData.First(l => l.Feature == lateralSource).DataType = Model1DLateralDataType.FlowConstant;
            waterFlowModel1D.LateralSourceData.First(l => l.Feature == lateralSource).Flow = discharge;
        }

        private static Discretization GetNetworkDiscretization(IHydroNetwork network)
        {
            var networkDiscretization = new Discretization
                                            {
                                                Network = network,
                                                SegmentGenerationMethod =
                                                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                            };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, 200);
            return networkDiscretization;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithSimpleNetworkMultipleTimes()
        {
            for (int i = 0; i < 5; i++)
            {
                log.WarnFormat("Run: {0}", i);
                ExecuteWithSimpleNetwork();
            }
        }

        private static void AddCrossSection(IChannel branch1, double chainage, double depth)
        {
            string name = "crs1" + branch1.Name + "_" + chainage;

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(100.0, 0.0),
                                        new Coordinate(150.0, depth),
                                        new Coordinate(300.0, depth),
                                        new Coordinate(350.0, 0.0),
                                        new Coordinate(500.0, 0.0)
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, chainage, yzCoordinates);
            cs1.Name = name;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithEmptyBoundaryConditions()
        {
            INode inflowNode;
            INode outflowNode;
            HydroNetwork network = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);
            var networkDiscretization = new Discretization
                {
                    Network = network,
                    SegmentGenerationMethod =
                        SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false,
                                                          true,
                                                          channel.Length / 2.0);
            }

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D
                {
                    Network = network,
                    NetworkDiscretization = networkDiscretization,
                    StartTime = new DateTime(2000, 1, 1),
                    StopTime = new DateTime(2000, 1, 2),
                    TimeStep = new TimeSpan(0, 0, 30),
                    // 30 min
                    OutputTimeStep = new TimeSpan(0, 0, 30)
                })
            {

                //waterFlowModel1D.RunInSeparateProcess = true;


                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                waterFlowModel1D.Initialize();

                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count());

                // run waterFlowModel1D
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status,
                                "Model should be in running state (computed 1 time step)");
            }
        }

        [Test] // TODO: SPLIT THIS TEST!
        public void IsLinkAllowed()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                NetworkHelper.AddBranchFeatureToBranch(new LateralSource(), waterFlowModel1D.Network.Branches.First());

                var timeSeriesDataItem = new DataItem(new TimeSeries());

                var waterFlowTimeSeriesDataItem = new DataItem(HydroTimeSeriesFactory.CreateFlowTimeSeries());
                var flowWaterLevelTableDataItem = new DataItem(new FlowWaterLevelTable());
                var waterLevelTimeSeriesDataItem = new DataItem(HydroTimeSeriesFactory.CreateWaterLevelTimeSeries());

                // Should not be able to link inside of waterFlowModel1D (handled in ModelBase)
                Assert.IsFalse(
                    waterFlowModel1D.IsLinkAllowed(waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.InitialFlow),
                                                   waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.OutputFlow)));

                // HydroNetwork can be linked to another network
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(new DataItem(new HydroNetwork(), DataItemRole.None),
                                                             waterFlowModel1D.GetDataItemByValue(
                                                                 waterFlowModel1D.Network)));

                // Linking of network to network coverage should not be possible
                Assert.IsFalse(waterFlowModel1D.IsLinkAllowed(new DataItem(new HydroNetwork(), DataItemRole.None),
                                                              waterFlowModel1D.GetDataItemByValue(
                                                                  waterFlowModel1D.OutputFlow)));

                // Boundary node data => node not connected to multiple branches => time series, Q(h), H and Q are supported
                var boundaryNodeDataDataItem1 = waterFlowModel1D.BoundaryConditions[0].SeriesDataItem;
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(timeSeriesDataItem, boundaryNodeDataDataItem1));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterFlowTimeSeriesDataItem, boundaryNodeDataDataItem1));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(flowWaterLevelTableDataItem, boundaryNodeDataDataItem1));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterLevelTimeSeriesDataItem, boundaryNodeDataDataItem1));

                // Boundary node data  => node connected to multiple branches => only time series and H are supported
                var boundaryNodeDataDataItem2 = waterFlowModel1D.BoundaryConditions[1].SeriesDataItem;
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(timeSeriesDataItem, boundaryNodeDataDataItem2));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterFlowTimeSeriesDataItem, boundaryNodeDataDataItem2));
                Assert.IsFalse(waterFlowModel1D.IsLinkAllowed(flowWaterLevelTableDataItem, boundaryNodeDataDataItem2));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterLevelTimeSeriesDataItem, boundaryNodeDataDataItem2));

                // Lateral source data => only time series, Q(h) and Q are supported
                var lateralsSourceDataDataItem = waterFlowModel1D.LateralSourceData[0].SeriesDataItem;
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(timeSeriesDataItem, lateralsSourceDataDataItem));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterFlowTimeSeriesDataItem, lateralsSourceDataDataItem));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(flowWaterLevelTableDataItem, lateralsSourceDataDataItem));
                Assert.IsTrue(waterFlowModel1D.IsLinkAllowed(waterLevelTimeSeriesDataItem, lateralsSourceDataDataItem));
            }
        }

        /// <summary>
        /// Test whether the update of a network in the WaterFlowModel1D is properly processed.
        /// ie the dataitem is updated.
        /// </summary>
        [Test]
        public void NetworkDataItemTest()
        {
            var network = new HydroNetwork();

            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.Network = network;

                Assert.IsTrue(waterFlowModel1D.DataItems.Any(di => ReferenceEquals(di.Value, network)));
            }
        }

        /// <summary>
        /// Create a network with branch and nodes and add it to a 1dflowmodel. 
        /// Boundary condition objects (default dead ends) are expected to be generated automatically for all boundary conditions.
        /// Remove the network from the 1dflowmodel.
        /// The boundary conditions should be automatically removed.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void WaterFlowModel1DBoundaryConditionsAreCreatedAndRemovedAutomatically()
        {
            var network = new HydroNetwork();
            var branch1 = new Channel();
            var branch2 = new Channel();
            var node1 = new HydroNode { Network = network };
            var node2 = new HydroNode { Network = network };
            var node3 = new HydroNode { Network = network };

            branch1.Source = node1;
            branch2.Target = node2;
            branch1.Source = node2;
            branch2.Target = node3;

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                Assert.AreEqual(3, waterFlowModel1D.BoundaryConditions.Count());
                Assert.AreEqual(Model1DBoundaryNodeDataType.None,
                                waterFlowModel1D.BoundaryConditions[0].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.None,
                                waterFlowModel1D.BoundaryConditions[1].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.None,
                                waterFlowModel1D.BoundaryConditions[2].DataType);

                waterFlowModel1D.Network = null;
                Assert.AreEqual(0, waterFlowModel1D.BoundaryConditions.Count());
            }
        }

        /// <summary>
        /// Create a network and add it to a 1dflowmodel. 
        /// Create nodes and branch and add them to network.
        /// Connect nodes to branch.
        /// Boundary conditions are expected to be generated automatically.
        /// Remove nodes from branch.
        /// Boundary conditions are expected to be removed automatically.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void WaterFlowModel1DBoundaryConditionsAreUpdatedAutomaticallyAfterAddingOrRemovingNodes()
        {
            var network = new HydroNetwork();
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                var branch1 = new Channel();
                var branch2 = new Channel();
                var node1 = new HydroNode { Network = network };
                var node2 = new HydroNode { Network = network };
                var node3 = new HydroNode { Network = network };

                network.Branches.Add(branch1);
                network.Branches.Add(branch2);
                network.Nodes.Add(node1);
                network.Nodes.Add(node2);
                network.Nodes.Add(node3);

                Assert.AreEqual(3, waterFlowModel1D.BoundaryConditions.Count());

                branch1.Source = node1;
                branch1.Target = node2;
                branch2.Source = node2;
                branch2.Target = node3;

                Assert.AreEqual(3, waterFlowModel1D.BoundaryConditions.Count());

                network.Branches.Remove(branch2);
                network.Nodes.Remove(node3);

                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReplaceBoundaryCondition()
        {
            var network = new HydroNetwork();
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                var branch1 = new Channel();
                var branch2 = new Channel();
                var node1 = new HydroNode { Network = network };
                var node2 = new HydroNode { Network = network };
                var node3 = new HydroNode { Network = network };

                network.Branches.Add(branch1);
                network.Branches.Add(branch2);
                network.Nodes.Add(node1);
                network.Nodes.Add(node2);
                network.Nodes.Add(node3);

                var boundaryConditions = waterFlowModel1D.BoundaryConditions;
                Assert.AreEqual(3, boundaryConditions.Count());
                Assert.AreEqual(Model1DBoundaryNodeDataType.None, boundaryConditions[0].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.None, boundaryConditions[1].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.None, boundaryConditions[2].DataType);

                var newBoundaryCondition1 = new Model1DBoundaryNodeData
                                                {
                                                    Feature = node1,
                                                    DataType = Model1DBoundaryNodeDataType.FlowConstant
                                                };

                var newBoundaryCondition2 = new Model1DBoundaryNodeData
                                                {
                                                    Feature = node2,
                                                    DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries
                                                };

                var newBoundaryCondition3 = new Model1DBoundaryNodeData
                                                {
                                                    Feature = node3,
                                                    DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable
                                                };

                var newBoundaryCondition4 = new Model1DBoundaryNodeData
                                                {
                                                    Feature = new Node(),
                                                    DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable
                                                };


                waterFlowModel1D.ReplaceBoundaryCondition(null);
                // Nothing should happen (no new boundary conditions provided)
                waterFlowModel1D.ReplaceBoundaryCondition(newBoundaryCondition1);
                waterFlowModel1D.ReplaceBoundaryCondition(newBoundaryCondition2);
                waterFlowModel1D.ReplaceBoundaryCondition(newBoundaryCondition3);
                waterFlowModel1D.ReplaceBoundaryCondition(newBoundaryCondition4);
                // Nothing should happen (node of new boundary conditions not found)

                boundaryConditions = waterFlowModel1D.BoundaryConditions;
                Assert.AreEqual(3, boundaryConditions.Count());
                Assert.AreEqual(Model1DBoundaryNodeDataType.FlowConstant, boundaryConditions[0].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.WaterLevelTimeSeries,
                                boundaryConditions[1].DataType);
                Assert.AreEqual(Model1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryConditions[2].DataType);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RoughnessSectionsAreCreatedAutomaticallyForNewCrossSectionSectionTypes()
        {
            var network = new HydroNetwork();
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network, UseReverseRoughness = false})
            {
                var branch = new Channel();
                var node1 = new HydroNode { Network = network };
                var node2 = new HydroNode { Network = network };

                branch.Source = node1;
                branch.Target = node2;

                network.Branches.Add(branch);
                network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "FloodPlain1" });
                Assert.AreEqual(network.CrossSectionSectionTypes.Count, waterFlowModel1D.RoughnessSections.Count());
                Assert.IsNotNull(waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[0]) && !rs.Reversed));
                Assert.IsNotNull(waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[1]) && !rs.Reversed));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RoughnessSectionsAreMaintainedAutomaticallyForReverse()
        {
            var network = new HydroNetwork();
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network, UseReverseRoughness = true, UseReverseRoughnessInCalculation = true})
            {
                var branch = new Channel();
                var node1 = new HydroNode { Network = network };
                var node2 = new HydroNode { Network = network };

                branch.Source = node1;
                branch.Target = node2;

                waterFlowModel1D.UseReverseRoughness = true;

                network.Branches.Add(branch);
                network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "FloodPlain1" });

                Assert.AreEqual(2 * network.CrossSectionSectionTypes.Count, waterFlowModel1D.RoughnessSections.Count());
                Assert.IsNotNull(waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[0]) && !rs.Reversed));
                var reverseRoughnessSection = waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[0]) && rs.Reversed);
                Assert.IsNotNull(reverseRoughnessSection);
                Assert.IsFalse(((ReverseRoughnessSection)reverseRoughnessSection).UseNormalRoughness);

                Assert.IsNotNull(waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[1]) && !rs.Reversed));
                reverseRoughnessSection = waterFlowModel1D.RoughnessSections.SingleOrDefault(rs => Equals(rs.CrossSectionSectionType, network.CrossSectionSectionTypes[1]) && rs.Reversed);
                Assert.IsNotNull(reverseRoughnessSection);
                Assert.IsFalse(((ReverseRoughnessSection)reverseRoughnessSection).UseNormalRoughness);

                waterFlowModel1D.UseReverseRoughness = false;

                Assert.AreEqual(network.CrossSectionSectionTypes.Count, waterFlowModel1D.RoughnessSections.Count());
            }
        }

        /// <summary>
        /// Create a network and add it to a 1dflowmodel. 
        /// Create nodes and branch and Connect nodes to branch.
        /// Add nodes and branch to network.
        /// Boundary conditions are expected to be generated automatically.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void FlowBoundaryConditionsAreCreatedAutomaticallyForNewNodes()
        {
            var network = new HydroNetwork();
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                var branch = new Channel();
                var node1 = new HydroNode { Network = network };
                var node2 = new HydroNode { Network = network };

                branch.Source = node1;
                branch.Target = node2;

                network.Branches.Add(branch);
                network.Nodes.Add(node1);
                network.Nodes.Add(node2);
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count());
            }
        }

        [Test]
        public void InitialConditionsInterpolation()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode();
            var node2 = new HydroNode();
            var node3 = new HydroNode();
            var branch = new Channel("branch", node1, node2);
            var branch2 = new Channel("branch", node2, node3);

            network.Branches.Add(branch);
            network.Branches.Add(branch2);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var wktReader = new WKTReader();
            branch.Geometry = wktReader.Read("LINESTRING(0 0,0 100)");
            branch2.Geometry = wktReader.Read("LINESTRING(0 100,500 100)");
            node1.Geometry = wktReader.Read("POINT(0 0)");
            node2.Geometry = wktReader.Read("POINT(0 100)");
            node3.Geometry = wktReader.Read("POINT(500 100)");

            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.Network = network;
                const double initialDefault = 2.0d;

                waterFlowModel1D.InitialConditions.DefaultValue = initialDefault;
                waterFlowModel1D.InitialConditions[new NetworkLocation(network.Branches[0], 0)] = 0.0;
                waterFlowModel1D.InitialConditions[new NetworkLocation(network.Branches[0], 80)] = 20.0;

                Assert.AreEqual(10, waterFlowModel1D.InitialConditions.Evaluate(new NetworkLocation(network.Branches[0], 40)));
                Assert.AreEqual(initialDefault,
                                waterFlowModel1D.InitialConditions.Evaluate(new NetworkLocation(network.Branches[1], 50)));
            }
        }

        [Test] // very buggy network construction
        public void ModifyingLinkedNetworkRefreshesNetworkRelatedData()
        {
            var network1 = new HydroNetwork();
            var network2 = new HydroNetwork();
            var branch1 = new Channel();
            var branch2 = new Channel();
            var node1 = new HydroNode { Network = network1 };
            var node2 = new HydroNode { Network = network1 };
            var node3 = new HydroNode { Network = network1 };

            branch1.Source = node1;
            branch1.Target = node2;
            branch2.Source = node2;
            branch2.Target = node3;

            network1.Branches.Add(branch1);
            network1.Nodes.Add(node1);
            network1.Nodes.Add(node2);

            network2.Branches.Add(branch1);
            network2.Branches.Add(branch2);
            network2.Nodes.Add(node1);
            network2.Nodes.Add(node2);

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network1 })
            {
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);

                waterFlowModel1D.BoundaryConditions.First().DataType =
                    Model1DBoundaryNodeDataType.FlowWaterLevelTable;

                var networkDataItem = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network);
                var sourceDataItem = new DataItem(network2);

                // Link the waterFlowModel1D network to another network - refreshes all data (including boundary conditions)
                networkDataItem.LinkTo(sourceDataItem);

                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);

                network2.Nodes.Add(node3);

                Assert.AreEqual(3, waterFlowModel1D.BoundaryConditions.Count);
                Assert.AreEqual(3,
                                waterFlowModel1D.BoundaryConditions.Count(
                                    bc => bc.DataType == Model1DBoundaryNodeDataType.None));
            }
        }

        [Test]
        public void LinkingOrUnlinkingOtherNetworkToOrFromModelNetworkRefreshesNetworkRelatedData()
        {
            // create network
            var network1 = new HydroNetwork
                {
                    Name = "network1",
                    Nodes = { new HydroNode(), new HydroNode() },
                    Branches = { new Channel() }
                };
            network1.Branches[0].Source = network1.Nodes[0];
            network1.Branches[0].Target = network1.Nodes[1];

            // create waterFlowModel1D with network1
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network1 })
            {
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);

                waterFlowModel1D.BoundaryConditions.First().DataType =
                    Model1DBoundaryNodeDataType.FlowWaterLevelTable;

                // link waterFlowModel1D network data item to new network data item
                var networkDataItem = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network);

                var network2 = new HydroNetwork
                    {
                        Name = "network2",
                        Nodes = { new HydroNode(), new HydroNode(), new HydroNode() },
                        Branches = { new Channel(), new Channel() }
                    };
                network2.Branches[0].Source = network2.Nodes[0];
                network2.Branches[0].Target = network2.Nodes[1];
                network2.Branches[1].Source = network2.Nodes[1];
                network2.Branches[1].Target = network2.Nodes[2];

                var sourceDataItem = new DataItem(network2);

                // Link the waterFlowModel1D network to another network
                networkDataItem.LinkTo(sourceDataItem);
                Assert.AreEqual(3, waterFlowModel1D.BoundaryConditions.Count);
                Assert.AreEqual(3,
                                waterFlowModel1D.BoundaryConditions.Count(
                                    bc => bc.DataType == Model1DBoundaryNodeDataType.None));

                // Unlink the waterFlowModel1D network from the other network
                networkDataItem.Unlink();
                Assert.AreEqual(0, waterFlowModel1D.BoundaryConditions.Count);
                // A new waterFlowModel1D network is created (=> network1 is lost)
            }
        }

        [Test]
        public void LinkingOrUnlinkingModelNetworkToOrFromOtherNetworkDoesNotRefreshNetworkRelatedData()
        {
            var network1 = new HydroNetwork();
            var network2 = new HydroNetwork();
            var branch1 = new Channel();
            var branch2 = new Channel();
            var node1 = new HydroNode { Network = network1 };
            var node2 = new HydroNode { Network = network1 };
            var node3 = new HydroNode { Network = network1 };

            branch1.Source = node1;
            branch1.Target = node2;
            branch2.Source = node2;
            branch2.Target = node3;

            network1.Branches.Add(branch1);
            network1.Nodes.Add(node1);
            network1.Nodes.Add(node2);

            network2.Branches.Add(branch1);
            network2.Branches.Add(branch2);
            network2.Nodes.Add(node1);
            network2.Nodes.Add(node2);
            network2.Nodes.Add(node3);

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network1 })
            {
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);

                waterFlowModel1D.BoundaryConditions.First().DataType =
                    Model1DBoundaryNodeDataType.FlowWaterLevelTable;

                var networkDataItem = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network);
                var targetDataItem = new DataItem(network2);

                // Link another network to the waterFlowModel1D network
                targetDataItem.LinkTo(networkDataItem);
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);
                Assert.AreEqual(Model1DBoundaryNodeDataType.FlowWaterLevelTable,
                                waterFlowModel1D.BoundaryConditions.First().DataType);

                // Unlink the other network from the waterFlowModel1D network
                targetDataItem.LinkTo(networkDataItem);
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);
                Assert.AreEqual(Model1DBoundaryNodeDataType.FlowWaterLevelTable,
                                waterFlowModel1D.BoundaryConditions.First().DataType);
            }
        }

        [Test]
        public void LinkingOrUnlinkingOtherNetworkToLinkedFlowDoesNotMessUpModelLinking()
        {
            var network = new HydroNetwork();
            var sourceDataItem = new DataItem(network);
            using (var waterFlowModel1 = new WaterFlowModel1D())
            {
                var networkDataItem1 = waterFlowModel1.GetDataItemByValue(waterFlowModel1.Network);
                using (var waterFlowModel2 = new WaterFlowModel1D())
                {
                    var networkDataItem2 = waterFlowModel2.GetDataItemByValue(waterFlowModel2.Network);

                    // Link the network of waterFlowModel1D 2 to the network of waterFlowModel1D 1
                    networkDataItem2.LinkTo(networkDataItem1);

                    Assert.IsNotNull(networkDataItem2.LinkedTo);
                    Assert.AreSame(networkDataItem1, networkDataItem2.LinkedTo);
                    Assert.AreSame(networkDataItem2, networkDataItem1.LinkedBy[0]);

                    // Link the network of waterFlowModel1D 1 to another network
                    networkDataItem1.LinkTo(sourceDataItem);

                    Assert.IsNotNull(networkDataItem1.LinkedTo);
                    Assert.AreSame(sourceDataItem, networkDataItem1.LinkedTo);
                    Assert.AreSame(networkDataItem1, sourceDataItem.LinkedBy[0]);

                    Assert.IsNotNull(networkDataItem2.LinkedTo);
                    Assert.AreSame(networkDataItem1, networkDataItem2.LinkedTo);
                    Assert.AreSame(networkDataItem2, networkDataItem1.LinkedBy[0]);

                    // Unlink the network of waterFlowModel1D 2
                    networkDataItem2.Unlink();

                    Assert.IsNull(networkDataItem2.LinkedTo);
                    Assert.IsNotNull(networkDataItem1.LinkedTo);
                    Assert.AreSame(sourceDataItem, networkDataItem1.LinkedTo);
                    Assert.AreSame(networkDataItem1, sourceDataItem.LinkedBy[0]);

                    // Unlink the network of waterFlowModel1D 1
                    networkDataItem1.Unlink();

                    Assert.IsNull(networkDataItem2.LinkedTo);
                    Assert.IsNull(networkDataItem1.LinkedTo);
                }
            }
        }

        private static WaterFlowModel1D GetFinishedDemoModel()
        {
            // use a valid network for the calculation
            var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            ActivityRunner.RunActivity(waterFlowModel1D);

            return waterFlowModel1D;
        }

        [Test]
        public void LateralSourceDataIsKeptOnBranchSplitAndMerge()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(1) })
            {
                var channel = waterFlowModel1D.Network.Channels.First();

                // Add a LSD to the branch
                NetworkHelper.AddBranchFeatureToBranch(new LateralSource(), waterFlowModel1D.Network.Branches[0], 80);

                // Setup waterFlowModel1D data
                waterFlowModel1D.LateralSourceData[0].DataType = Model1DLateralDataType.FlowConstant;
                waterFlowModel1D.LateralSourceData[0].Flow = 22.0;

                // Split the branch so the lateral is placed on a new branch
                HydroNetworkHelper.SplitChannelAtNode(channel, 50);

                // Verify waterFlowModel1D data is maintained
                Assert.AreEqual(Model1DLateralDataType.FlowConstant, waterFlowModel1D.LateralSourceData[0].DataType);
                Assert.AreEqual(22.0d, waterFlowModel1D.LateralSourceData[0].Flow);

                // Merge the branch so the lateral is placed on a new branch
                NetworkHelper.MergeNodeBranches(waterFlowModel1D.Network.Nodes.First(n => n.IsConnectedToMultipleBranches),
                                                waterFlowModel1D.Network);

                // Verify waterFlowModel1D data is maintained
                Assert.AreEqual(Model1DLateralDataType.FlowConstant, waterFlowModel1D.LateralSourceData[0].DataType);
                Assert.AreEqual(22.0d, waterFlowModel1D.LateralSourceData[0].Flow);
            }
        }

        [Test]
        public void LaterSourceDataIsCreatedForNewLateralSources()
        {
            //single branch network
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(1) })
            {
                //add a LSD to the branch
                waterFlowModel1D.Network.Branches[0].BranchFeatures.Add(new LateralSource());

                Assert.AreEqual(1, waterFlowModel1D.LateralSourceData.Count);
            }
        }

        [Test]
        public void LaterSourceDataIsCreatedForExistingLateralSources()
        {
            //single branch network with a source
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            //add a LSD to the branch
            network.Branches[0].BranchFeatures.Add(new LateralSource());

            //set an existing network..this should create LSDs
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                Assert.AreEqual(1, waterFlowModel1D.LateralSourceData.Count);
            }
        }

        [Test]
        public void WFM1DDataItemsShouldNotBeRemovable()
        {
            //single branch network, so we have 2 bound nodes in the bound dataItemset
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(1) })
            {
                //add a LSD to the branch, so we have 1 source node in the source dataItemset
                waterFlowModel1D.Network.Branches[0].BranchFeatures.Add(new LateralSource());
                foreach (IDataItem dataItem in waterFlowModel1D.DataItems)
                {
                    if (dataItem.Value is IHydroNetwork)
                    {
                        Assert.IsFalse(dataItem.IsRemoveable);
                    }
                    if (dataItem.Value is Discretization)
                    {
                        Assert.IsFalse(dataItem.IsRemoveable);
                    }
                    var dataItemSet = dataItem as DataItemSet;
                    if (dataItemSet != null)
                    {
                        if (!dataItem.IsRemoveable)
                        {
                            foreach (var item in dataItemSet.DataItems)
                            {
                                Assert.IsFalse(item.IsRemoveable);
                            }
                        }
                    }
                    if (dataItem.Value is NetworkCoverage)
                    {
                        Assert.IsFalse(dataItem.IsRemoveable);
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RoughnesSectionAfterClone()
        {
            INode inflowNode;
            INode outflowNode;
            var network = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);
            var crossSection = network.CrossSections.First();

            crossSection.Definition.Sections.Add(new CrossSectionSection
            {
                MinY = crossSection.Definition.Profile.Select(c => c.X).Min(),
                MaxY = crossSection.Definition.Profile.Select(c => c.X).Max(),
            });

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                Assert.AreEqual(1, waterFlowModel1D.RoughnessSections.Count);

                var clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();
                Assert.AreEqual(1, clonedModel.RoughnessSections.Count);
                var clonedRoughnessSection = clonedModel.RoughnessSections[0];
                Assert.AreEqual(waterFlowModel1D.RoughnessSections[0].Name, clonedRoughnessSection.Name);
            }
        }


        [Test]
        public void SwitchInitialConditions()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                DefaultInitialWaterLevel = 33.0,
                DefaultInitialDepth = 13.0,
                InitialConditionsType = InitialConditionsType.Depth
            })
            {
                Assert.AreEqual(InitialConditionsType.Depth, waterFlowModel1D.InitialConditionsType);
                //check the default initial depth made it to the coverage..
                Assert.AreEqual(13.0, waterFlowModel1D.InitialConditions.DefaultValue);

                waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
                Assert.AreEqual(InitialConditionsType.WaterLevel, waterFlowModel1D.InitialConditionsType);

                //check the coverage name changed
                Assert.AreEqual("Initial Water Level", waterFlowModel1D.InitialConditions.Name);
                //check the default changed
                Assert.AreEqual(33.0, waterFlowModel1D.InitialConditions.DefaultValue);
            }
        }

        [Test]
        public void ChangeValueOfInitialConditionsCoverageSyncsWithModel()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;

                waterFlowModel1D.InitialConditions.DefaultValue = 33.4;
                //depth should be set on waterFlowModel1D as well.
                Assert.AreEqual(33.4, waterFlowModel1D.DefaultInitialDepth);

                //and the other way..set on the waterFlowModel1D should set the coverage
                waterFlowModel1D.DefaultInitialDepth = 15.5;
                Assert.AreEqual(15.5, waterFlowModel1D.InitialConditions.DefaultValue);

                //change the initial water should not have an effect now
                waterFlowModel1D.DefaultInitialWaterLevel = 14.4;
                Assert.AreEqual(15.5, waterFlowModel1D.InitialConditions.DefaultValue);
                //change the type to waterlevel
                waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
                Assert.AreEqual(14.4, waterFlowModel1D.InitialConditions.DefaultValue);
                //assert the depth stays the same
                Assert.AreEqual(15.5, waterFlowModel1D.DefaultInitialDepth);
            }
        }

        [Test]
        public void CheckNetworkRelatedDataSetsNetwork()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = new HydroNetwork() })
            {
                var inputCoverages =
                    waterFlowModel1D.DataItems.Where(
                            di => (((di.Role & DataItemRole.Input) == DataItemRole.Input) && (di.Value is INetworkCoverage)))
                            .Select(di => di.Value);
                waterFlowModel1D.UseSalt = true;
                foreach (INetworkCoverage networkCoverage in inputCoverages)
                {
                    Assert.AreEqual(waterFlowModel1D.Network, networkCoverage.Network);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void CloneIncludesOutput()
        {
            // use a valid network for the calculation
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {

                RunModel(waterFlowModel1D);

                var clone = (WaterFlowModel1D) waterFlowModel1D.Clone();

                Assert.IsNotNull(clone.OutputWaterLevel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetNetworkFromSecondModel()
        {
            var project = new Project();

            using (var waterFlowModel1D1 = new WaterFlowModel1D())
            {
                project.RootFolder.Add(new DataItem(waterFlowModel1D1));
                waterFlowModel1D1.Network.Name = "network1";

                using (var waterFlowModel1D2 = new WaterFlowModel1D())
                {
                    project.RootFolder.Add(new DataItem(waterFlowModel1D2));
                    waterFlowModel1D2.Network.Name = "network2";

                    var models = NetworkEditorHelper.GetAllModelsContainingHydroNetwork(waterFlowModel1D2.Network, project);

                    Assert.AreEqual(1, models.Count);
                    Assert.AreEqual(waterFlowModel1D2.Network, ((WaterFlowModel1D)models[0]).Network);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreatedOutputCoverageDataItemsAreNotCopyable()
        {
            //because of problems with copying coverages in netcdf this functionality has been disabled
            //enable once it is clear how this should work
            using (var flowModel1D = GetFinishedDemoModel())
            {
                //all output dataitems containing a coverage are not copyable
                Assert.IsTrue(
                    flowModel1D.DataItems.Where(
                        di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is ICoverage).All(
                            di => !di.IsCopyable));
            }
        }

        [Test]
        public void NamesOfDataItemsAreReadOnlyByDefault()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.DataItems.All(i => i.NameIsReadOnly)
                    .Should("all data items of flow waterFlowModel1D have read-only names").Be.True();
            }
        }

        [Test]
        public void ReadParametersAfterInitializeWFM1D()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                Assert.Greater(waterFlowModel1D.ParameterSettings.Count, 0);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        //Needed for the testbench tests
        public void OnInitializingTheTemplateDataDirectoryShouldSet()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                Assert.IsTrue(string.IsNullOrEmpty(waterFlowModel1D.WorkingDirectory));

                int count = 0;
                waterFlowModel1D.StatusChanged += (sender, e) =>
                                       {
                                           if (e.NewStatus == ActivityStatus.Initializing)
                                           {
                                               Assert.IsFalse(string.IsNullOrEmpty(waterFlowModel1D.WorkingDirectory));
                                               count++;
                                           }
                                       };

                waterFlowModel1D.Initialize();

                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void ChangingOutputParametersAddsDataItems()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                var outputDataItems = waterFlowModel1D.DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
                var initialOutputDataItemsCount = outputDataItems.Count();
                var firstNotEnabledOutputItem =
                    waterFlowModel1D.OutputSettings.EngineParameters.First(p => p.AggregationOptions == AggregationOptions.None);

                // enable it
                firstNotEnabledOutputItem.AggregationOptions = AggregationOptions.Current;

                outputDataItems.Count().Should("enabling output should add output data item").Be.EqualTo(
                    initialOutputDataItemsCount + 1);
            }
        }

        [Test]
        public void ChangingOutputParameterOfExistingOuputCoverageCreatesNewDataItem()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                var waterLevelEngineParameter =
                    waterFlowModel1D.OutputSettings.EngineParameters.First(
                        ep => ep.Name == Model1DParameterNames.LocationWaterLevel);
                Assert.AreEqual(AggregationOptions.Current, waterLevelEngineParameter.AggregationOptions);

                var waterLevelOutputDataItem =
                    waterFlowModel1D.DataItems.Single(di => di.Tag == Model1DParameterNames.LocationWaterLevel);
                Assert.AreEqual("Water level", waterLevelOutputDataItem.Name);
                Assert.AreEqual("Water level", ((INameable)waterLevelOutputDataItem.Value).Name);
                Assert.AreEqual("Water level", waterFlowModel1D.OutputWaterLevel.Name);
                Assert.AreEqual("Water level", waterLevelOutputDataItem.Tag);

                // Change OutputParameter:
                waterLevelEngineParameter.AggregationOptions = AggregationOptions.Maximum;

                var waterLevelOutputDataItemAfterChange =
                    waterFlowModel1D.DataItems.Single(di => di.Tag == Model1DParameterNames.LocationWaterLevel);

                // There should be only 1 data item with this tag!
                Assert.AreEqual("Water level (Maximum)", waterLevelOutputDataItemAfterChange.Name);
                Assert.AreEqual("Water level (Maximum)",
                                ((INameable)waterLevelOutputDataItemAfterChange.Value).Name);
                Assert.AreEqual("Water level (Maximum)", waterFlowModel1D.OutputWaterLevel.Name);
                Assert.AreEqual("Water level", waterLevelOutputDataItemAfterChange.Tag);

                Assert.IsTrue(!ReferenceEquals(waterLevelOutputDataItem, waterLevelOutputDataItemAfterChange),
                              "A new DataItem should have been created");
            }
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void AddingStructureWhenStructuresOutputIsEnabledResizesOutputFeatureCoverages()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // enable output on structures
                waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Structures).
                    AggregationOptions = AggregationOptions.Current;

                var outputFeatureCoverage = (IFeatureCoverage)waterFlowModel1D.DataItems.Last().Value;

                outputFeatureCoverage.FeatureVariable.Values.Count
                    .Should("check if output coverage is empty").Be.EqualTo(0);

                // add structure
                var weirBranch = waterFlowModel1D.Network.Branches[0];
                var weirOfset = weirBranch.Length / 2;
                var weir = new Weir("weir") { Chainage = weirOfset, CrestLevel = 0.1 };

                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, weirBranch);

                RunModel(waterFlowModel1D);

                outputFeatureCoverage.FeatureVariable.Values.Count
                    .Should("check if output coverage is resized").Be.EqualTo(1);
            }
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void AddingComputationalPointsAddsLocationsToTheOutputNetworkCoverages()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                // enable output on structures
                var outputNetworkCoverage = waterFlowModel1D.OutputFlow;

                // check1
                outputNetworkCoverage.Locations.Values.Count
                    .Should("check if output coverage is empty").Be.EqualTo(0);

                RunModel(waterFlowModel1D);

                // check2
                outputNetworkCoverage.Locations.Values.Count
                        .Should("check if output coverage is resized").Be.EqualTo(
                            waterFlowModel1D.NetworkDiscretization.Locations.Values.Count - 2);
            }
        }

        [Test]
        public void SalinityEstuaryMouthNodeIdIsSynchronizedWithRenaming()
        {
            var model = new WaterFlowModel1D();
            var hydroNode = new HydroNode("test");

            model.Network.Nodes.Add(hydroNode);
            model.SalinityEstuaryMouthNodeId = "test";

            hydroNode.Name = "Test 2";

            Assert.AreEqual("Test 2", model.SalinityEstuaryMouthNodeId);
        }

        [Test]
        public void SalinityEstuaryMouthNodeIsRemovedIfNodeIsRemovedFromNetwork()
        {
            var model = new WaterFlowModel1D();
            var hydroNode = new HydroNode("test");

            var network = model.Network;

            network.Nodes.Add(hydroNode);
            model.SalinityEstuaryMouthNodeId = "test";

            network.BeginEdit(new DefaultEditAction("removing node"));
            network.Nodes.Remove(hydroNode);
            network.EndEdit();

            Assert.IsNullOrEmpty(model.SalinityEstuaryMouthNodeId);
        }

        [Test]
        public void SalinityEstuaryMouthNodeIsRemovedIfNodeIsNoLongerValid()
        {
            var model = new WaterFlowModel1D();
            var hydroNode1 = new HydroNode("test");
            var hydroNode2 = new HydroNode("test2");
            var branch= new Channel(hydroNode1, hydroNode2);

            var network = model.Network;

            network.Nodes.Add(hydroNode1);
            network.Nodes.Add(hydroNode2);
            network.Branches.Add(branch);

            model.SalinityEstuaryMouthNodeId = "test";

            network.BeginEdit(new DefaultEditAction("removing branch"));
            network.Branches.Remove(branch);
            network.EndEdit();

            Assert.IsNullOrEmpty(model.SalinityEstuaryMouthNodeId);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void RunModelWithStructureOutputTwice()
        {
            for (int i = 0; i < 2; i++)
            {
                using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
                {

                    waterFlowModel1D.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Structures).
                        AggregationOptions = AggregationOptions.Current;

                    var weirBranch = waterFlowModel1D.Network.Branches[1];
                    var weirOfset = weirBranch.Length / 2;
                    var weir = new Weir("weir") { Chainage = weirOfset, CrestLevel = 0.1 };

                    HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, weirBranch);

                    log.Debug("Before run");
                    RunModel(waterFlowModel1D);

                    Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
                }
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteWithThatcherHarlemanDispersion()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true,
                                                            channel.Length / 2.0);
            }

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D
                                       {
                                           Network = network,
                                           NetworkDiscretization = networkDiscretization,
                                           StartTime = new DateTime(2000, 1, 1),
                                           StopTime = new DateTime(2000, 1, 2),
                                           TimeStep = new TimeSpan(0, 0, 30),
                                           // 30 min
                                           OutputTimeStep = new TimeSpan(0, 0, 30),
                                           UseSalt = true,
                                           DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic
                                       })
            {
                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;


                var branch = waterFlowModel1D.Network.Channels.First();
                CrossSectionHelper.AddCrossSection(branch, 10, -10);

                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                var startOfBranch = new NetworkLocation(branch, 0.0);
                var endOfBranch = new NetworkLocation(branch, 100.0);

                var dispersionF1Coverage = waterFlowModel1D.DispersionCoverage;
                dispersionF1Coverage[startOfBranch] = 0.0;
                dispersionF1Coverage[endOfBranch] = 1.0;
                var dispersionF3Coverage = waterFlowModel1D.DispersionCoverage;
                dispersionF3Coverage[startOfBranch] = 2.0;
                dispersionF3Coverage[endOfBranch] = 3.0;

                // set estuaryMouth
                waterFlowModel1D.SalinityEstuaryMouthNodeId = network.Nodes[0].Name;

                waterFlowModel1D.Initialize();
                
                // run waterFlowModel1D
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status,
                                "Model should be in running state (computed 1 time step)");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteAndCheckSalinityParametersFileGeneratedIfF4HasValues(bool f4HasValues)
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true, channel.Length / 2.0);
            }

            // setup 1d flow waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                Network = network,
                NetworkDiscretization = networkDiscretization,
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                TimeStep = new TimeSpan(0, 0, 30),
                // 30 min
                OutputTimeStep = new TimeSpan(0, 0, 30),
                UseSalt = true,
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic,
                SalinityEstuaryMouthNodeId = network.HydroNodes.First().Name,
                UseSaltInCalculation = true
            })
            {
                // set initial conditions
                waterFlowModel1D.InitialFlow.DefaultValue = 0.1;
                waterFlowModel1D.InitialConditions.DefaultValue = 0.1;

                var branch = waterFlowModel1D.Network.Channels.First();
                CrossSectionHelper.AddCrossSection(branch, 10, -10);

                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                if (f4HasValues)
                {
                    var startOfBranch = new NetworkLocation(branch, 0.0);
                    var endOfBranch = new NetworkLocation(branch, 100.0);
                    var dispersionF4Coverage = waterFlowModel1D.DispersionF4Coverage;
                    dispersionF4Coverage[startOfBranch] = 2.0;
                    dispersionF4Coverage[endOfBranch] = 3.0;
                }

                waterFlowModel1D.Initialize();

                var md1dPath = waterFlowModel1D.GetExporterPath(Path.Combine(waterFlowModel1D.WorkingDirectory ,waterFlowModel1D.DirectoryName));
                
                var categories = new DelftIniReader().ReadDelftIniFile(md1dPath);
                if (categories.Count == 0)
                    throw new FileReadingException(String.Format("Could not read file {0} properly, it seems empty", md1dPath));

                var fileSection = categories.Where(category => category.Name == ModelDefinitionsRegion.FilesIniHeader).ToList();
                if (fileSection.Count > 1 && fileSection.Any())
                    throw new FileReadingException(String.Format("Could not read files section {0} properly", md1dPath));

                string generatedSalinityPath = String.Empty;

                if (f4HasValues)
                {
                    var salinityFile = fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.SalinityParametersFile.Key);
                    generatedSalinityPath = Path.Combine(Path.GetDirectoryName(md1dPath), salinityFile);
                }

                Assert.True(waterFlowModel1D.UseSalt);
                Assert.True(waterFlowModel1D.UseSaltInCalculation);
                Assert.True(waterFlowModel1D.DispersionFormulationType == DispersionFormulationType.KuijperVanRijnPrismatic);

                Assert.True(File.Exists(md1dPath));
                Assert.That(File.Exists(generatedSalinityPath), Is.EqualTo(f4HasValues));
            }
        }
        
        # region Input / Output sync tests

        [Test]
        [Category((TestCategory.Integration))]
        public void ModifyingBranchesCollectionShouldClearOutputCoverages()
        {
            Assert.AreEqual(EffectOnOutput.Cleared,
                            GetEffectOfActionOnOutputCoverages(model => model.Network.Branches.RemoveAt(0)));
        }

        [Test]
        [Category((TestCategory.Integration))]
        public void ModifyingBranchGeometryShouldClearOutputCoverages()
        {
            Assert.AreEqual(EffectOnOutput.Cleared,
                            GetEffectOfActionOnOutputCoverages(
                                model =>
                                model.Network.Branches[0].Geometry =
                                new LineString(new[] { new Coordinate(0, 0), new Coordinate(25, 0) })));
        }

        [Test]
        [Category((TestCategory.Integration))]
        public void ModifyingBranchFeatureGeometryShouldClearOutputCoverages()
        {
            Assert.AreEqual(EffectOnOutput.Cleared,
                            GetEffectOfActionOnOutputCoverages(
                                prepareAction: model =>
                                {
                                    var lateral = new LateralSource { Chainage = 25.0 };
                                    var branch = model.Network.Branches[0];
                                    branch.BranchFeatures.Add(lateral);
                                    lateral.Branch = branch;
                                },
                                action: model =>
                                {
                                    model.Network.LateralSources.First().Chainage = 30.0;
                                }
                                ));
        }

        [Test]
        [Category((TestCategory.Integration))]
        public void ModifyingBranchFeatureCollectionShouldClearOutputCoverages()
        {
            Assert.AreEqual(EffectOnOutput.Cleared,
                            GetEffectOfActionOnOutputCoverages(
                                prepareAction: model =>
                                {
                                    var lateral = new LateralSource { Chainage = 25.0 };
                                    var branch = model.Network.Branches[0];
                                    branch.BranchFeatures.Add(lateral);
                                    lateral.Branch = branch;
                                },
                                action: model => model.Network.Branches[0].BranchFeatures.RemoveAt(0)));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OutputDataIsClearedIfModelNetworkChanges()
        {
            Assert.AreEqual(EffectOnOutput.Cleared,
                            GetEffectOfActionOnOutputCoverages(m => m.Network.Nodes[0].Geometry = new Point(20, 20)));
        }

        [Test]
        [Category((TestCategory.Integration))]
        public void OutputDataIsMarkedAsDisconnectedIfNetworkRouteIsAdded()
        {
            Assert.AreEqual(EffectOnOutput.Disconnected,
                            GetEffectOfActionOnOutputCoverages(
                                model =>
                                {
                                    var branch = model.Network.Branches[0];
                                    var route = HydroNetworkHelper.AddNewRouteToNetwork(model.Network);

                                    route.Locations.AddValues(new[]
                                                                      {
                                                                          new NetworkLocation(branch, 0),
                                                                          new NetworkLocation(branch, 50)
                                                                      });

                                    var segments = route.Segments.Values; //force segment update
                                }));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OutputDataIsMarkedAsDisconnectedIfModelChanges()
        {
            Assert.AreEqual(EffectOnOutput.Disconnected,
                            GetEffectOfActionOnOutputCoverages(m => m.StartTime = DateTime.Now));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OutputDataIsMarkedAsDisconnectedIfModelOutputSettingChanges()
        {
            Assert.AreEqual(EffectOnOutput.Disconnected,
                            GetEffectOfActionOnOutputCoverages(m => m.OutputSettings.GridOutputTimeStep = new TimeSpan()));
        }

        private static EffectOnOutput GetEffectOfActionOnOutputCoverages(Action<WaterFlowModel1D> action, Action<WaterFlowModel1D> prepareAction = null)
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                if (prepareAction != null)
                {
                    prepareAction(waterFlowModel1D);
                }
                WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(waterFlowModel1D);

                ActivityRunner.RunActivity(waterFlowModel1D);

                Assert.Greater(waterFlowModel1D.OutputFunctions.First().Components[0].Values.Count, 0,
                               "Coverages empty initially, unexpected!");

                action(waterFlowModel1D);

                var coverage = waterFlowModel1D.OutputFunctions.First();
                var hasValues = coverage.Components[0].Values.Count > 0;

                if (waterFlowModel1D.OutputOutOfSync)
                {
                    if (!hasValues)
                    {
                        Assert.Fail("Expected coverage to still have values");
                    }
                    return EffectOnOutput.Disconnected;
                }

                return hasValues ? EffectOnOutput.None : EffectOnOutput.Cleared;
            }
        }

        private enum EffectOnOutput
        {
            None,
            Cleared,
            Disconnected
        }

        # endregion

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void PointLateralBetweenCalculationGridPointsTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             point lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is a correct net flow of 0 in this specific configuration.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(branchlength / 4, 0, branchlength))
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceOverSingleCalculationGridPointTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is a correct net flow of 0 in this specific configuration.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(branchlength / 4, branchlength / 10, branchlength)) // Over 1 grid point
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceOverMultipleCalculationGridPointsTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is a correct net flow of 0 in this specific configuration.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(branchlength / 4, branchlength * (3 / 10), branchlength)) // Over 3 grid points
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceBetweenCalculationGridPointsLeftOfSegmentMidpointTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is no net flow of 0 in this specific configuration, while it is modeled to have this.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(202.0, branchlength / (5 * 10), branchlength)) // Between 2 grid points
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceBetweenCalculationGridPointsRightOfSegmentMidpointTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is no net flow of 0 in this specific configuration, while it is modeled to have this.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(262.0, branchlength / (5 * 10), branchlength)) // Between 2 grid points
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceBetweenCalculationGridPointsOverSegmentMidpointTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *  Q=+1           default ZW cross-section            Q=-3
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate there is no net flow of 0 in this specific configuration, while it is modeled to have this.
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(240.0, branchlength / (5 * 10), branchlength)) // Between 2 grid points
            {
                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                foreach (var timeValue in timeValues)
                {
                    Assert.AreEqual(-2.0, (double)waterFlowModel1D.OutputWaterLevel[timeValue, location], 1e-5);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void DiffuseLateralSourceBetweenCalculationGridPointsShouldInduceWaterForPositiveQTools7106()
        {
            const double branchlength = 1000.0;
            /*
             * Creates following network:
             *             diffuse lateral source, Q = +2
             * node1        /                                      node2
             *  O----------v-------------|-------------------------->O
             *               default ZW cross-section
             *  
             * Initial flow = 0;
             * Initial water level = -2 (default ZW gives lowest water level of -10)
             * 
             * Goal of this test is to demonstrate the small diffuse lateral is consuming water, instead of being ignored by calculation engine
             */

            // Create a network with net flow of 0, with initial water level at -2m.
            // Lateral source is a point lateral
            using (var waterFlowModel1D = CreateSmallDiffuseLateralNetwork(branchlength / 4, branchlength / (5 * 10), branchlength)) // Between 2 grid points
            {
                var bndNode1 = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == waterFlowModel1D.Network.Nodes[0]);
                bndNode1.DataType = Model1DBoundaryNodeDataType.None;
                var bndNode2 = waterFlowModel1D.BoundaryConditions.First(bc => bc.Feature == waterFlowModel1D.Network.Nodes[1]);
                bndNode2.DataType = Model1DBoundaryNodeDataType.None;

                RunModel(waterFlowModel1D);
                var timeValues = waterFlowModel1D.OutputWaterLevel.Arguments[0].Values;
                int locationIndex =
                    Convert.ToInt32(Math.Ceiling((double)waterFlowModel1D.OutputWaterLevel.Arguments[1].Values.Count / 2));
                var location = waterFlowModel1D.OutputWaterLevel.Arguments[1].Values[locationIndex];

                // Output water level should remain -2m as the net volume of water should not change.
                for (int i = 1; i < timeValues.Count; i++)
                {
                    var currentLevel = (double)waterFlowModel1D.OutputWaterLevel[timeValues[i], location];
                    var previousLevel = (double)waterFlowModel1D.OutputWaterLevel[timeValues[i - 1], location];
                    Assert.Greater(currentLevel, previousLevel);
                }
            }
        }

        private static WaterFlowModel1D CreateSmallDiffuseLateralNetwork(double lateralSourceChainage, double lateralSourceLength, double branchLength)
        {
            // Define general structure...
            var network = new HydroNetwork();
            var node1 = new HydroNode { Name = "Node1", Network = network, Geometry = new Point(0.0, 0.0) };
            var node2 = new HydroNode { Name = "Node2", Network = network, Geometry = new Point(branchLength, 0.0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            var branch1 = new Channel("branch1", node1, node2)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(branchLength, 0) })
            };
            network.Branches.Add(branch1);

            // Add network features
            var crossSection1 = CrossSection.CreateDefault(CrossSectionType.ZW, branch1, branchLength / 2);
            var csDef = crossSection1.Definition as CrossSectionDefinitionZW;
            crossSection1.Name = "crs1";
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, csDef.FlowWidth());
            branch1.BranchFeatures.Add(crossSection1);

            var waterFlowModel1D = new WaterFlowModel1D("flow waterFlowModel1D")
            {
                NetworkDiscretization = new Discretization
                {
                    Name = WaterFlowModel1DDataSet.DiscretizationDataObjectName,
                    Network = network,
                    SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                }
            };

            var calcGridLocations = new NetworkLocation[11];
            for (var i = 0; i < 11; i++)
            {
                calcGridLocations[i] = new NetworkLocation(branch1, branchLength * (i / 10.0));

            }
            waterFlowModel1D.NetworkDiscretization.Locations.AddValues(calcGridLocations);

            // Configure timers
            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            waterFlowModel1D.StartTime = t;
            waterFlowModel1D.StopTime = t.AddHours(12);
            waterFlowModel1D.TimeStep = new TimeSpan(1, 0, 0);
            waterFlowModel1D.OutputTimeStep = waterFlowModel1D.TimeStep;

            // Initial conditions
            waterFlowModel1D.InitialFlow.DefaultValue = 0.0;
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
            waterFlowModel1D.InitialConditions.DefaultValue = -2.0;
            waterFlowModel1D.DefaultInitialWaterLevel = -2.0;

            waterFlowModel1D.Network = network;

            // Set boundary Conditions
            var boundaryConditionNode1 = waterFlowModel1D.BoundaryConditions.First(bc => ReferenceEquals(bc.Feature, node1));
            boundaryConditionNode1.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionNode1.Flow = 1.0;

            var boundaryConditionNode2 = waterFlowModel1D.BoundaryConditions.First(bc => ReferenceEquals(bc.Feature, node2));
            boundaryConditionNode2.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionNode2.Flow = -3.0;

            //  Add 1m diffuse lateral with Q = 2
            var dls = new LateralSource { Name = "dls1", Length = lateralSourceLength, Branch = branch1, Chainage = lateralSourceChainage };
            branch1.BranchFeatures.Add(dls);

            HydroRegionEditorHelper.UpdateBranchFeatureGeometry(dls, lateralSourceLength);
            waterFlowModel1D.LateralSourceData[0].DataType = Model1DLateralDataType.FlowConstant;
            waterFlowModel1D.LateralSourceData[0].Flow = 2.0;

            return waterFlowModel1D;
        }

        [Test]
        public void NetworkDataItemIsNotRenamedWhenSourceNetworkDataItemIsRenamed()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                var modelNetworkDataItem = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network);

                var network = new HydroNetwork();

                var networkDataItem = new DataItem(network);

                modelNetworkDataItem.LinkTo(networkDataItem);

                network.Name = "new name";

                modelNetworkDataItem.Name
                        .Should("waterFlowModel1D network data item name is not changed").Be.EqualTo("Network");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ChangeInitialConditionsTypeShouldUpdateDataItemName()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
                var nameBefore = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.InitialConditions).Name;

                waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
                var nameAfter = waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.InitialConditions).Name;

                Assert.AreNotEqual(nameBefore, nameAfter);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyPasteModelAndReplaceNetworkByLinkingShouldClearOldBoundaryConditions()
        {
            // create network
            var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };
            var branch1 = new Channel("branch1", node1, node2) { Geometry = new LineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(100, 0) }) };
            var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };

            // create waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                // create empty network
                var emptyNetwork = new HydroNetwork();
                var emptyNetworkDataItem = new DataItem(emptyNetwork);

                // clone
                var modelClone = (WaterFlowModel1D)waterFlowModel1D.DeepClone();

                // link network
                modelClone.GetDataItemByValue(modelClone.Network).LinkTo(emptyNetworkDataItem);

                // asserts
                modelClone.BoundaryConditions.Count.Should().Be.EqualTo(0);
            }
        }

        [Test]
        public void ReuseChildDataItems()
        {
            // create network
            var weir = new Weir { Name = "weir1", Geometry = new Point(10, 0) };
            var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };
            var branch1 = new Channel("branch1", node1, node2)
                {
                    Geometry = new LineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(100, 0) }),
                    BranchFeatures = { weir }
                };
            var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };

            // create waterFlowModel1D
            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                // query child item
                var items = waterFlowModel1D.GetChildDataItems(weir).ToList(); 
                
                var childDataItem =
                    waterFlowModel1D.GetChildDataItems(weir).First(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);

                // add it under network data item (usually happens on linking)
                waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network).Children.Add(childDataItem);

                // assert
                waterFlowModel1D.GetChildDataItems(weir).First(di => (di.Role & DataItemRole.Input) == DataItemRole.Input)
                .Should("child data items are reused").Be.SameInstanceAs(childDataItem);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyPasteModelWithLinkedNetworkShouldNotDamageBoundaryConditionTypes()
        {
            // link network
            var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };
            var branch1 = new Channel("branch1", node1, node2) { Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(100, 0) }) };
            var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };
            var networkDataItem = new DataItem(network);

            // create waterFlowModel1D, link network and change bc type
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network).LinkTo(networkDataItem);

                // add network and waterFlowModel1D to project
                var project = new Project();
                project.RootFolder.Add(waterFlowModel1D);
                project.RootFolder.Add(networkDataItem);

                // change 1st bc type to flow time series
                waterFlowModel1D.BoundaryConditions[0].DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;

                // add waterFlowModel1D clone to project
                var modelClone = (WaterFlowModel1D)waterFlowModel1D.DeepClone();
                project.RootFolder.Add(modelClone);

                // asserts
                modelClone.BoundaryConditions[0].DataType.Should().Be.EqualTo(
                    Model1DBoundaryNodeDataType.FlowTimeSeries);
            }
        }
        [Test]
        public void AttributeInitialWaterDepth()
        {
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                Assert.AreEqual(InitialConditionsType.Depth, model.InitialConditionsType);
                Assert.AreEqual(FunctionAttributes.StandardNames.WaterDepth, model.InitialConditions.Components[0].Attributes[FunctionAttributes.StandardName]);

                model.InitialConditionsType = InitialConditionsType.WaterLevel;

                Assert.AreEqual(FunctionAttributes.StandardNames.WaterLevel, model.InitialConditions.Components[0].Attributes[FunctionAttributes.StandardName]);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ValuesAtObservationPointShouldBeInterpolated()
        {
            // Interpolation in cf.dll was adjusted such that output coverage values at observation
            // points are interpolated instead of being taken from the nearest calculation (grid) point.
            //
            // The network consists of 1 branch of length 100, with distance between grid points of 10.
            // The observation point lies at chainage 52. Nearest grid points are at chainages 50 and 60.
            // Value at observation point is interpolated with respect to these grid points.
            // This holds for coverage values water level and water depth.
            //
            // Because of the use of a staggered grid in cf.dll, the remaining coverage values velocity
            // and discharge are interpolated with respect to intermediate calculation points at
            // chainages 45 and 55.

            // create simple network
            var network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(100, 0);

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add cross-section
            CrossSectionHelper.AddCrossSection(branch1, 50.0d, 0.0d);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            //add observation point
            var observationPoint = ObservationPoint.CreateDefault(branch1);
            observationPoint.Name = "OP";
            observationPoint.Chainage = 52.0;
            branch1.BranchFeatures.Add(observationPoint);

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
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionInflow.WaterLevel = 5.0;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
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
            var valueAtChainage50 = waterLevelsAtGridPoints[5];
            var valueAtChainage60 = waterLevelsAtGridPoints[6];

            // check values at observation point
            var waterLevel =
                flowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name.Contains("Water level (op)"));
            var valueFilter = new VariableValueFilter<DateTime>(waterLevel.Arguments[0], flowModel1D.StopTime);
            var waterLevelsAtObservationPoint = waterLevel.GetValues<double>(valueFilter);

            var valueAtChainage52 = 0.8 * valueAtChainage50 + 0.2 * valueAtChainage60;
            Assert.AreEqual(valueAtChainage52, waterLevelsAtObservationPoint[0], 0.00001d);

            // check water depth
            var waterDepthGridPoints = flowModel1D.OutputDepth;
            valueFilterGP = new VariableValueFilter<DateTime>(waterDepthGridPoints.Arguments[0], flowModel1D.StopTime);
            var waterDepthsAtGridPoints = waterDepthGridPoints.GetValues<double>(valueFilterGP);
            valueAtChainage50 = waterDepthsAtGridPoints[5];
            valueAtChainage60 = waterDepthsAtGridPoints[6];

            // check values at observation point
            var waterDepth =
                flowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name.Contains("Water depth (op)"));
            valueFilter = new VariableValueFilter<DateTime>(waterDepth.Arguments[0], flowModel1D.StopTime);
            var waterDepthsAtObservationPoint = waterDepth.GetValues<double>(valueFilter);

            valueAtChainage52 = 0.8 * valueAtChainage50 + 0.2 * valueAtChainage60;
            Assert.AreEqual(valueAtChainage52, waterDepthsAtObservationPoint[0], 0.00001d);

            // check velocity
            var velocityPoints = flowModel1D.OutputVelocity;
            var valueFilterP = new VariableValueFilter<DateTime>(velocityPoints.Arguments[0], flowModel1D.StopTime);
            var velocityAtPoints = velocityPoints.GetValues<double>(valueFilterP);
            var valueAtChainage45 = velocityAtPoints[4];
            var valueAtChainage55 = velocityAtPoints[5];

            // check values at observation point
            var velocity =
                flowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name.Contains("Velocity (op)"));
            valueFilter = new VariableValueFilter<DateTime>(velocity.Arguments[0], flowModel1D.StopTime);
            var velocityAtObservationPoint = velocity.GetValues<double>(valueFilter);

            valueAtChainage52 = 0.3 * valueAtChainage45 + 0.7 * valueAtChainage55;
            Assert.AreEqual(valueAtChainage52, velocityAtObservationPoint[0], 0.00001d);

            // check discharge
            var dischargePoints = flowModel1D.OutputFlow;
            valueFilterP = new VariableValueFilter<DateTime>(dischargePoints.Arguments[0], flowModel1D.StopTime);
            var dischargeAtPoints = dischargePoints.GetValues<double>(valueFilterP);
            valueAtChainage45 = dischargeAtPoints[4];
            valueAtChainage55 = dischargeAtPoints[5];

            // check values at observation point
            var discharge =
                flowModel1D.OutputFunctions.OfType<FeatureCoverage>().First(c => c.Name.Contains("Discharge (op)"));
            valueFilter = new VariableValueFilter<DateTime>(discharge.Arguments[0], flowModel1D.StopTime);
            var dischargeAtObservationPoint = discharge.GetValues<double>(valueFilter);

            valueAtChainage52 = 0.3 * valueAtChainage45 + 0.7 * valueAtChainage55;
            Assert.AreEqual(valueAtChainage52, dischargeAtObservationPoint[0], 0.00001d);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ObservationPointsLinearOrNearest_Jira_Tools_8102()
        {
            var network = new HydroNetwork();

            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(4550, 0);

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(endCoordinate) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add cross-section
            var definitionYZ = new CrossSectionDefinitionYZ();
            definitionYZ.YZDataTable.Clear();
            definitionYZ.YZDataTable.AddCrossSectionYZRow(0, 1.0, 0);
            definitionYZ.YZDataTable.AddCrossSectionYZRow(10, 1.0, 0);
            definitionYZ.YZDataTable.AddCrossSectionYZRow(11, -1.0, 0);
            definitionYZ.YZDataTable.AddCrossSectionYZRow(15, -1.0, 0);
            definitionYZ.YZDataTable.AddCrossSectionYZRow(16, 1.0, 0);
            definitionYZ.YZDataTable.AddCrossSectionYZRow(26, 1.0, 0);
            var crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, definitionYZ, 105.0d);
            crossSection.Name = HydroNetworkHelper.GetUniqueFeatureName(network, crossSection);

            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            // add weir
            var weir = new Weir { CrestWidth = 5, CrestLevel = 1, FlowDirection = FlowDirection.Both };
            ((SimpleWeirFormula) weir.WeirFormula).DischargeCoefficient = 0.8;
            var compositeStructure = new CompositeBranchStructure { Chainage = 2518.0 };
            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch1, compositeStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeStructure, weir);

            //add lateral
            var lateral = new LateralSource { Chainage = 1218.0, Name = "myLateral" };
            branch1.BranchFeatures.Add(lateral);

            //add observation points
            var observationPoint = ObservationPoint.CreateDefault(branch1);
            observationPoint.Name = "VoorLateral";
            observationPoint.Chainage = 1111.0;
            branch1.BranchFeatures.Add(observationPoint);

            observationPoint = ObservationPoint.CreateDefault(branch1);
            observationPoint.Name = "NaLateral";
            observationPoint.Chainage = 1332.0;
            branch1.BranchFeatures.Add(observationPoint);

            observationPoint = ObservationPoint.CreateDefault(branch1);
            observationPoint.Name = "VoorStructure";
            observationPoint.Chainage = 2400.0;
            branch1.BranchFeatures.Add(observationPoint);

            observationPoint = ObservationPoint.CreateDefault(branch1);
            observationPoint.Name = "NaStructure";
            observationPoint.Chainage = 2615.0;
            branch1.BranchFeatures.Add(observationPoint);

            // add discretization
            var networkDiscretization = new Discretization { Network = network };

            networkDiscretization[new NetworkLocation(branch1, 0.0)] = 0.0;
            networkDiscretization[new NetworkLocation(branch1, 909.0)] = 0.0;
            networkDiscretization[new NetworkLocation(branch1, 1818.0)] = 0.0;
            networkDiscretization[new NetworkLocation(branch1, 2727.0)] = 0.0;
            networkDiscretization[new NetworkLocation(branch1, 3636.0)] = 0.0;
            networkDiscretization[new NetworkLocation(branch1, 4550.0)] = 0.0;

            // setup 1d flow model
            var t = new DateTime(2012, 12, 13);
            var flowModel1D = new WaterFlowModel1D
                                  {
                                      Network = network,
                                      NetworkDiscretization = networkDiscretization,
                                      StartTime = t,
                                      StopTime = t.AddDays(1),
                                      TimeStep = new TimeSpan(1, 0, 0),
                                      OutputTimeStep = new TimeSpan(1, 0, 0),
                                  };
            flowModel1D.OutputSettings.StructureOutputTimeStep = new TimeSpan(1, 0, 0);
            flowModel1D.OutputSettings.BranchVelocity = AggregationOptions.Current;

            //Lateral : constant Q = 5
            var lateralSourceData = flowModel1D.LateralSourceData.First();
            lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
            lateralSourceData.Flow = 5.0;

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 0.0;
            flowModel1D.InitialConditions.DefaultValue = 0.0;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.Flow = 10.0;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = -0.5;

            // set output coverages
            var paramWaterLevelObservation = flowModel1D.OutputSettings.GetEngineParameter(QuantityType.WaterLevel,
                                                                                           ElementSet.Observations);
            paramWaterLevelObservation.AggregationOptions = AggregationOptions.Current;
            var paramVelocityObservation = flowModel1D.OutputSettings.GetEngineParameter(QuantityType.Velocity,
                                                                                         ElementSet.Observations);
            paramVelocityObservation.AggregationOptions = AggregationOptions.Current;

            //----------------------------------------------------------------
            //
            // LINEAR
            //
            //----------------------------------------------------------------

            var settingObervationPointsInterpolation = flowModel1D.ParameterSettings.First(p => p.Category == ParameterCategory.ObservationPoints && p.Name == "InterpolationType");
            settingObervationPointsInterpolation.Value = "Linear";

            var iadvec1D = flowModel1D.ParameterSettings.FirstOrDefault(s => s.Name == "Iadvec1D");
            if (iadvec1D != null)
            {
                iadvec1D.Value = "1";
            }

            RunModel(flowModel1D);

            var waterLevelValues = flowModel1D.OutputWaterLevel.GetValues<double>();
            var velocityValues = flowModel1D.OutputVelocity.GetValues<double>();
            var waterLevelObservationPointValues =
                flowModel1D.OutputFunctions.First(oc => oc.Name == paramWaterLevelObservation.Name).GetValues<double>();
            var velocityObservationPointValues =
                flowModel1D.OutputFunctions.First(oc => oc.Name == paramVelocityObservation.Name).GetValues<double>();

            //Last TimeStep

            //waterlevel

            Assert.AreEqual(2.7699851, waterLevelValues[144], 0.001);
            Assert.AreEqual(2.7699851, waterLevelValues[145], 0.001);
            Assert.AreEqual(2.6913069, waterLevelValues[146], 0.001);
            Assert.AreEqual(1.1576315, waterLevelValues[147], 0.001);
            Assert.AreEqual(0.6796524, waterLevelValues[148], 0.001);
            Assert.AreEqual(-0.5, waterLevelValues[149], 0.001);

            //velocity 

            Assert.AreEqual(0.1785544, velocityValues[120], 0.001);
            Assert.AreEqual(0.2678317, velocityValues[121], 0.001);
            Assert.AreEqual(3.3258302, velocityValues[122], 0.001);
            Assert.AreEqual(1.0650292, velocityValues[123], 0.001);
            Assert.AreEqual(1.8459033, velocityValues[124], 0.001);

            //waterlevel (op)

            Assert.AreEqual(2.7525011, waterLevelObservationPointValues[96], 0.001);
            Assert.AreEqual(2.7333725, waterLevelObservationPointValues[97], 0.001);
            Assert.AreEqual(2.6913069, waterLevelObservationPointValues[98], 0.001);
            Assert.AreEqual(1.1576315, waterLevelObservationPointValues[99], 0.001);

            //velocity(op)

            Assert.AreEqual(0.2678317, velocityObservationPointValues[96], 0.001);
            Assert.AreEqual(0.2678317, velocityObservationPointValues[97], 0.001);
            Assert.AreEqual(3.3258302, velocityObservationPointValues[98], 0.001);
            Assert.AreEqual(3.3258302, velocityObservationPointValues[99], 0.001);


            //----------------------------------------------------------------
            //
            // NEAREST
            //
            //----------------------------------------------------------------

            settingObervationPointsInterpolation.Value = "Nearest";


            RunModel(flowModel1D,false);

            waterLevelValues = flowModel1D.OutputWaterLevel.GetValues<double>();
            velocityValues = flowModel1D.OutputVelocity.GetValues<double>();
            waterLevelObservationPointValues =
                flowModel1D.OutputFunctions.First(oc => oc.Name == paramWaterLevelObservation.Name).GetValues<double>();
            velocityObservationPointValues =
                flowModel1D.OutputFunctions.First(oc => oc.Name == paramVelocityObservation.Name).GetValues<double>();

            //Last TimeStep

            //waterlevel

            Assert.AreEqual(2.7699851, waterLevelValues[144], 0.001);
            Assert.AreEqual(2.7699851, waterLevelValues[145], 0.001);
            Assert.AreEqual(2.6913069, waterLevelValues[146], 0.001);
            Assert.AreEqual(1.1576315, waterLevelValues[147], 0.001);
            Assert.AreEqual(0.6796524, waterLevelValues[148], 0.001);
            Assert.AreEqual(-0.5, waterLevelValues[149], 0.001);

            //velocity 

            Assert.AreEqual(0.1785544, velocityValues[120], 0.001);
            Assert.AreEqual(0.2678317, velocityValues[121], 0.001);
            Assert.AreEqual(3.3258302, velocityValues[122], 0.001);
            Assert.AreEqual(1.0650292, velocityValues[123], 0.001);
            Assert.AreEqual(1.8459033, velocityValues[124], 0.001);

            //waterlevel (op)

            Assert.AreEqual(2.7699851, waterLevelObservationPointValues[96], 0.001); //diff a little bit from linear
            Assert.AreEqual(2.7699851, waterLevelObservationPointValues[97], 0.001); //diff a little bit from linear
            Assert.AreEqual(2.6913069, waterLevelObservationPointValues[98], 0.001);
            Assert.AreEqual(1.1576315, waterLevelObservationPointValues[99], 0.001);

            //velocity(op)

            Assert.AreEqual(0.2678317, velocityObservationPointValues[96], 0.001);
            Assert.AreEqual(0.2678317, velocityObservationPointValues[97], 0.001);
            Assert.AreEqual(3.3258302, velocityObservationPointValues[98], 0.001);
            Assert.AreEqual(3.3258302, velocityObservationPointValues[99], 0.001);

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void OutputSettingsPropertyChangedTest_Tools7980()
        {
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var waterLevelParameter = model.OutputSettings.EngineParameters.First(ep => ep.Name == "Water level");

                var coverage = model.OutputFunctions.First(oc => oc.Name == "Water level");
                Assert.AreEqual("Water level", coverage.Components[0].Name);
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level (Average)"));

                // Changing of water level parameter (a parameter that supports multiple aggregation options):
                waterLevelParameter.AggregationOptions = AggregationOptions.Average;
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level"));

                coverage = model.OutputFunctions.First(oc => oc.Name == "Water level (Average)");
                Assert.AreEqual("Water level (Average)", coverage.Components[0].Name);

                waterLevelParameter.AggregationOptions = AggregationOptions.None;
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level"));
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level (Average)"));

                // Run model, now names should not change:
                model.Initialize();
                model.Execute();

                // Adding new output does have name as expected:
                waterLevelParameter.AggregationOptions = AggregationOptions.Current;
                coverage = model.OutputFunctions.First(oc => oc.Name == "Water level");
                Assert.AreEqual("Water level", coverage.Components[0].Name);
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level (Average)"));

                // Changing output settings changes name and clears output
                waterLevelParameter.AggregationOptions = AggregationOptions.Average;
                coverage = model.OutputFunctions.First(oc => oc.Name == "Water level (Average)");
                Assert.AreEqual("Water level (Average)", coverage.Components[0].Name);
                Assert.IsFalse(model.OutputFunctions.Any(oc => oc.Name == "Water level"));
            }
        }

        [Test]
        public void TestGetMetaDataRequirementsIsImplementedForAllSupportedVersions()
        {
            var flowModel = new WaterFlowModel1D();
            var allSupportedVersions = TypeUtils.GetStaticField<int[]>(typeof(WaterFlowModel1D), "SupportedMetaDataVersions");
            foreach (var version in allSupportedVersions)
            {
                Assert.DoesNotThrow(() => TypeUtils.CallPrivateMethod(flowModel, "GetMetaDataRequirements", version));
            }
        }

        [Test]
        public void UseReverseRoughnessUpdatesReverseRoughnessUseNormal()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode();
            var node2 = new HydroNode();
            var branch1 = new Channel(node1, node2)
                {
                    Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(100, 0)})
                };
            network.Nodes.AddRange(new[]{node1, node2});
            network.Branches.Add(branch1);

            var flowModel = new WaterFlowModel1D
                {
                    Network = network,
                    UseReverseRoughness = true,
                    UseReverseRoughnessInCalculation = true
                };

            Assert.IsTrue(flowModel.RoughnessSections.OfType<ReverseRoughnessSection>()
                                   .All(rs => rs.UseNormalRoughness == false));

            flowModel.UseReverseRoughnessInCalculation = false;

            Assert.IsTrue(flowModel.RoughnessSections.OfType<ReverseRoughnessSection>()
                                   .All(rs => rs.UseNormalRoughness == true));
        }

        [Test]
        public void GivenNetworkWithRdNewCoordinateSystem_WhenCoordinateSystemIsSetToNull_ThenGeodeticLengthsOfBranchesAreUpdated()
        {
            using (var flowModel = new WaterFlowModel1D())
            {
                var network = new HydroNetwork();
                var node1 = new HydroNode();
                var node2 = new HydroNode();
                var branch1 = new Channel(node1, node2)
                {
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
                };
                network.Nodes.AddRange(new[] { node1, node2 });
                flowModel.Network = network;
                network.Branches.Add(branch1);
                Assert.That(branch1.GeodeticLength, Is.NaN);
                Assert.That(branch1.Length, Is.EqualTo(100).Within(0.1));
                if (Map.CoordinateSystemFactory == null)
                    Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
                var rdNewCoordinateSystem = 28992;
                flowModel.Network.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(rdNewCoordinateSystem);
                Assert.That(branch1.GeodeticLength, Is.Not.NaN);
                Assert.That(branch1.Length, Is.EqualTo(branch1.GeodeticLength).Within(0.1));
                flowModel.Network.CoordinateSystem = null;
                Assert.That(branch1.GeodeticLength, Is.NaN);
                Assert.That(branch1.Length, Is.EqualTo(100).Within(0.1));
            }
        }

    }
}