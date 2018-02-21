using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;
using NetTopologySuite.Extensions.Coverages;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    /// <summary>
    /// Tests related to salt handling in waterflowmodel 1d.
    /// </summary>
    [TestFixture]    
    public class WaterFlowModel1DSaltTest
    {
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

        [TestCase(true)]
        [TestCase(false)]
        public void EnableDisableEnableSaltChangesModelCoverages(bool useKuijperVanRijnPrismatic)
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.DispersionFormulationType = useKuijperVanRijnPrismatic
                    ? DispersionFormulationType.KuijperVanRijnPrismatic
                    : DispersionFormulationType.Constant; 

                Assert.IsNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNull(waterFlowModel1D.DispersionCoverage);

                // enable salt
                waterFlowModel1D.UseSalt = true;

                Assert.IsNotNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNotNull(waterFlowModel1D.DispersionCoverage);
                Assert.AreEqual(useKuijperVanRijnPrismatic, waterFlowModel1D.DispersionF3Coverage != null);
                Assert.AreEqual(useKuijperVanRijnPrismatic, waterFlowModel1D.DispersionF4Coverage != null);

                // disable salt
                waterFlowModel1D.UseSalt = false;

                //all should be 'normal' again
                Assert.IsNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNull(waterFlowModel1D.DispersionCoverage);
                Assert.IsNull(waterFlowModel1D.DispersionF3Coverage);
                Assert.IsNull(waterFlowModel1D.DispersionF4Coverage);

                // enable salt
                waterFlowModel1D.UseSalt = true;

                Assert.IsNotNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNotNull(waterFlowModel1D.DispersionCoverage);
                Assert.AreEqual(useKuijperVanRijnPrismatic, waterFlowModel1D.DispersionF3Coverage != null);
                Assert.AreEqual(useKuijperVanRijnPrismatic, waterFlowModel1D.DispersionF4Coverage != null);
            }
        }

        [Test]
        public void SwitchBetweenDispersionFormulationsTypes()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                Assert.IsNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNull(waterFlowModel1D.DispersionCoverage);

                // Enable salt: default to dispersion formulation constant. 
                waterFlowModel1D.UseSalt = true;

                Assert.IsNotNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNotNull(waterFlowModel1D.DispersionCoverage);
                Assert.IsNull(waterFlowModel1D.DispersionF3Coverage);
                Assert.IsNull(waterFlowModel1D.DispersionF4Coverage);

                Assert.That(waterFlowModel1D.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));

                // Set to ThatcherHarleman
                waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;
                Assert.IsNotNull(waterFlowModel1D.DispersionF3Coverage);
                Assert.IsNotNull(waterFlowModel1D.DispersionF4Coverage);

                // Disable and enable salt does not change existing formulation
                waterFlowModel1D.UseSalt = false;
                waterFlowModel1D.UseSalt = true;
                Assert.That(waterFlowModel1D.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.KuijperVanRijnPrismatic));

                // Set to Constant
                waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.Constant;
                Assert.That(waterFlowModel1D.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
                Assert.IsNull(waterFlowModel1D.DispersionF3Coverage);
                Assert.IsNull(waterFlowModel1D.DispersionF4Coverage);

                // Disable salt should remove some coverages, too. 
                waterFlowModel1D.UseSalt = false;
                Assert.IsNull(waterFlowModel1D.InitialSaltConcentration);
                Assert.IsNull(waterFlowModel1D.DispersionCoverage);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void EnableSaltChangesBoundaryConditions()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                //should have two BC's without 'salt'
                Assert.AreEqual(2, waterFlowModel1D.BoundaryConditions.Count);
                Assert.IsTrue(waterFlowModel1D.BoundaryConditions.All(bc => !bc.UseSalt));

                //enable salt
                waterFlowModel1D.UseSalt = true;

                //assert the BC's got updates
                Assert.IsTrue(waterFlowModel1D.BoundaryConditions.All(bc => bc.UseSalt));

                //come back
                waterFlowModel1D.UseSalt = false;

                Assert.IsTrue(waterFlowModel1D.BoundaryConditions.All(bc => !bc.UseSalt));
            }
        }

        [Test]
        public void GetSetUseSaltInCalculation()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                //default
                Assert.IsFalse(waterFlowModel1D.UseSaltInCalculation);

                waterFlowModel1D.UseSaltInCalculation = true;

                Assert.IsTrue(waterFlowModel1D.UseSaltInCalculation);
            }
        }

        [Test]
        public void UseSaltParametersAffectEachOther()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                waterFlowModel1D.UseSalt = true;
                waterFlowModel1D.UseSaltInCalculation = true;
                //action! turn of all the salt
                waterFlowModel1D.UseSalt = false;

                //checkt the calculation is also false
                Assert.IsFalse(waterFlowModel1D.UseSaltInCalculation);

                //turning on saltincalculation turn on salt
                waterFlowModel1D.UseSaltInCalculation = true;

                Assert.IsTrue(waterFlowModel1D.UseSalt);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void EnableSaltChangesLaterals()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            using (var waterFlowModel1D = new WaterFlowModel1D { Network = network })
            {
                network.Branches[0].BranchFeatures.Add(new LateralSource());

                //enable salt
                waterFlowModel1D.UseSalt = true;

                //assert the laterals s got updates
                Assert.IsTrue(waterFlowModel1D.LateralSourceData.All(bc => bc.UseSalt));

                //come back
                waterFlowModel1D.UseSalt = false;

                Assert.IsTrue(waterFlowModel1D.LateralSourceData.All(bc => !bc.UseSalt));
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("WIP")]
        public void RunSimpleSaltModelCheckOutputIsNotZero()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            using (var model = new WaterFlowModel1D
                                    {
                                        UseSaltInCalculation = true,
                                        StartTime = startTime,
                                        StopTime = stopTime
                                    })
            {
                //a node network with single branch and 2 nodes
                var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
                model.Network = network;

                //add a single crossection halfway the branch
                CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);

                //generate discretization 
                HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                          0.5, false, false, true, 200);

                model.InitialSaltConcentration.DefaultValue = 0.0;
                model.DispersionCoverage.DefaultValue = 2.0;

                //TODO: make it sinusoidal like the modelapi test
                //one side is inflow of salt water 
                WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
                qBoundary.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                qBoundary.SaltConcentrationTimeSeries[startTime] = 0.0;
                qBoundary.SaltConcentrationTimeSeries[stopTime] = 10.0;
                qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                qBoundary.Flow = 2.0;

                //the other side is constant Sal and H 
                WaterFlowModel1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
                hBoundary.SaltConditionType = SaltBoundaryConditionType.Constant;
                hBoundary.SaltConcentrationConstant = 1.0;
                hBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                hBoundary.WaterLevel = 3.0;

                // set output coverages on
                model.OutputSettings.LocationSaltConcentration = AggregationOptions.Current;
                model.OutputSettings.EngineParameters.First(
                    p => p.Name == WaterFlowModelParameterNames.BranchSaltDispersion).AggregationOptions =
                    AggregationOptions.Current;
                model.OutputSettings.EngineParameters.First(p => p.Name == WaterFlowModelParameterNames.LocationDensity)
                    .AggregationOptions = AggregationOptions.Current;


                model.Initialize();
                WaterFlowModel1DTestHelper.RunInitializedModel(model);

                var outputSaltConcentration =
                    (INetworkCoverage)
                    model.OutputFunctions.First(o => o.Name == WaterFlowModelParameterNames.LocationSaltConcentration);
                var location = outputSaltConcentration.Locations.Values[4];
                var time = outputSaltConcentration.Time.Values[10];

                //Assert output is not zero
                Assert.AreNotEqual(0.0, (double) outputSaltConcentration[time, location]);
                var outputCoverageDispersion =
                    (INetworkCoverage)
                    model.OutputFunctions.First(oc => oc.Name == WaterFlowModelParameterNames.BranchSaltDispersion);
                Assert.AreNotEqual(0.0, (double) outputCoverageDispersion[time, location]);
                var outputCoverageDensity =
                    (INetworkCoverage)
                    model.OutputFunctions.First(oc => oc.Name == WaterFlowModelParameterNames.LocationDensity);
                Assert.AreNotEqual(0.0, (double) outputCoverageDensity[time, location]);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestDispersionParameter()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            using (var model = new WaterFlowModel1D
                                    {
                                        UseSaltInCalculation = true,
                                        StartTime = startTime,
                                        StopTime = stopTime,
                                    })
            {
                //a node network with single branch and 2 nodes
                var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
                model.Network = network;

                //add a single crossection halfway the branch
                CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);
                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                //generate discretization 
                HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                          0.5, false, false, true, 200);

                model.InitialSaltConcentration.DefaultValue = 0.0;

                //TODO: make it sinusoidal like the modelapi test
                //one side is inflow of salt water 
                WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
                qBoundary.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                qBoundary.SaltConcentrationTimeSeries[startTime] = 0.0;
                qBoundary.SaltConcentrationTimeSeries[stopTime] = 10.0;
                qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                qBoundary.Flow = 2.0;

                //the other side is constant Sal and H 
                WaterFlowModel1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
                hBoundary.SaltConditionType = SaltBoundaryConditionType.Constant;
                hBoundary.SaltConcentrationConstant = 1.0;
                hBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                hBoundary.WaterLevel = 3.0;

                //////////////////////////////////////////////////////////////////////////////////
                model.DispersionCoverage.DefaultValue = 0.0;

                // we also want salt output 
                model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
                model.OutputSettings.LocationSaltConcentration = AggregationOptions.Current;

                WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(model);

                model.Initialize();
                WaterFlowModel1DTestHelper.RunInitializedModel(model);

                var outputSaltConcentration =
                    (INetworkCoverage)
                    model.OutputFunctions.First(
                        o => o.Name.StartsWith(WaterFlowModelParameterNames.LocationSaltConcentration));

                var location = outputSaltConcentration.Locations.Values[4];
                var time = outputSaltConcentration.Time.Values[10];

                var valueDispersion0 = (double) outputSaltConcentration[time, location];

                model.DispersionCoverage.DefaultValue = 100000.0;
                model.Initialize();
                WaterFlowModel1DTestHelper.RunInitializedModel(model);

                var valueDispersion100000 = (double) outputSaltConcentration[time, location];

                Assert.AreNotEqual(valueDispersion0, valueDispersion100000);
            }

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)] //Local OK, maybe something to do with salt values in parameters.xml/sobeksim.ini
        public void TestTimeLagParameter()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            using (var model = new WaterFlowModel1D
                                    {
                                        UseSaltInCalculation = true,
                                        StartTime = startTime,
                                        StopTime = stopTime
                                    })
            {
                //a node network with single branch and 2 nodes
                var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
                model.Network = network;

                //add a single crossection halfway the branch
                CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);

                //generate discretization 
                HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                          0.5, false, false, true, 200);

                model.InitialSaltConcentration.DefaultValue = 0.0;
                model.DispersionCoverage.DefaultValue = 100000.0;
                model.OutputSettings.LocationSaltConcentration = AggregationOptions.Current;
                model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

                //one side is inflow of salt water 
                WaterFlowModel1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
                qBoundary.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                qBoundary.SaltConcentrationTimeSeries[startTime] = 0.0;
                qBoundary.SaltConcentrationTimeSeries[stopTime] = 10.0;
                qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                qBoundary.Flow = 2.0;

                //the other side is constant Sal and H 
                WaterFlowModel1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
                hBoundary.SaltConditionType = SaltBoundaryConditionType.Constant;
                hBoundary.SaltConcentrationConstant = 0.0;
                hBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                hBoundary.WaterLevel = 3.0;

                //////////////////////////////////////////////////////////////////////////////////

                //ThatcherHarlemannCoefficient result 0
                qBoundary.ThatcherHarlemannCoefficient = 0;

                model.Initialize();
                WaterFlowModel1DTestHelper.RunInitializedModel(model);

                var outputSaltConcentration =
                    (INetworkCoverage)
                    model.OutputFunctions.First(
                        o => o.Name.StartsWith(WaterFlowModelParameterNames.LocationSaltConcentration));

                var location = outputSaltConcentration.Locations.Values[4];
                var time = outputSaltConcentration.Time.Values[10];

                var valuesDispersionTimeLag0 = outputSaltConcentration.GetValues<double>().ToArray();

                //ThatcherHarlemannCoefficient result 200
                qBoundary.ThatcherHarlemannCoefficient = 200;

                model.Initialize();
                WaterFlowModel1DTestHelper.RunInitializedModel(model);

                var valuesDispersionTimeLag200 = outputSaltConcentration.GetValues<double>().ToArray();

                //ThatcherHarlemannCoefficient result 0 is not ThatcherHarlemannCoefficient result 200

                Assert.AreNotEqual(valuesDispersionTimeLag0, valuesDispersionTimeLag200);
            }
        }

        private class ModelResolution
        {
            public ModelResolution(double gridResolution, TimeSpan timeStep)
            {
                GridResolution = gridResolution;
                TimeStep = timeStep;
            }

            public double GridResolution { get; set; }
            public TimeSpan TimeStep { get; set; }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void VerifySaltConcentrationIsReasonablyIndependentOfGridSizeAndTimeStep()
        {
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                model.StartTime = new DateTime(2000, 1, 1);
                model.StopTime = new DateTime(2000, 1, 1, 5, 0, 0); //5h later
                model.UseSalt = true;
                model.UseSaltInCalculation = true;
                model.InitialSaltConcentration.DefaultValue = 0.2;
                model.DispersionCoverage.DefaultValue = 2.0;
                
                model.OutputSettings.LocationSaltConcentration = AggregationOptions.Current;

                // add a salt boundary
                var qBoundary = model.BoundaryConditions[0];
                qBoundary.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                qBoundary.SaltConcentrationTimeSeries[model.StartTime] = 0.0;
                qBoundary.SaltConcentrationTimeSeries[model.StopTime] = 3.0;
                qBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                qBoundary.Flow = 1.5;

                var modelResolutions = new List<ModelResolution>
                    {
                        new ModelResolution(5, new TimeSpan(0, 0, 30)),
                        new ModelResolution(10, new TimeSpan(0, 0, 30)),
                        new ModelResolution(25, new TimeSpan(0, 0, 30)),
                        new ModelResolution(5, new TimeSpan(0, 2, 30)),
                        new ModelResolution(10, new TimeSpan(0, 2, 30)),
                        new ModelResolution(25, new TimeSpan(0, 2, 30)),
                        new ModelResolution(5, new TimeSpan(0, 5, 0)),
                        new ModelResolution(10, new TimeSpan(0, 5, 0)),
                        new ModelResolution(25, new TimeSpan(0, 5, 0)),
                    };

                var branch1 = model.Network.Branches[0];
                branch1.IsLengthCustom = true;
                branch1.Length = 1000;

                var sampleTime = new DateTime(2000, 1, 1, 2, 30, 0);
                var sampleLocation = new NetworkLocation(branch1, branch1.Length - 1); // end of branch
                model.OutputTimeStep = new TimeSpan(0, 0, 1, 0); //1min

                foreach(var resolution in modelResolutions)
                {
                    // apply time & spatial resolution:
                    SetGridResolution(model, resolution.GridResolution);
                    model.TimeStep = resolution.TimeStep;
                    
                    // run model:
                    ActivityRunner.RunActivity(model);
                    
                    // gather salt concentration
                    var outputSaltConcentration = (INetworkCoverage) model.OutputFunctions.First(
                        o => o.Name == WaterFlowModelParameterNames.LocationSaltConcentration);

                    var saltConc = outputSaltConcentration.Evaluate(sampleTime,
                                                                    sampleLocation);
                    var waterLevel = model.OutputWaterLevel.Evaluate(sampleTime, sampleLocation);

                    Console.WriteLine(
                        "[ΔGrid: {0,2}m, Δt: {1,2}m{2,2}s] Salt Conc.: {3:0.000} ppt (Water level: {4:0.000} m AD)",
                        resolution.GridResolution, resolution.TimeStep.Minutes, resolution.TimeStep.Seconds, saltConc,
                        waterLevel);
                }
            }
        }

        private static void SetGridResolution(WaterFlowModel1D model, double gridPointDistance)
        {
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, true, 10, false,
                                                      0.5, false, false, true, 10); // erase
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, gridPointDistance); // set
        }
    }
}