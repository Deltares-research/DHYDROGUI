﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffModelDataSaveLoadTest
    {
        private const string ORIGINAL_CATCHMENT_NAME = "MyFirstCatchment";

        [Test]
        public void SaveAndLoadGreenHouseData()
        {
            string path = null;
            try
            {
                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    var firstCatchment = new Catchment
                        {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.GreenHouse};

                    GreenhouseData greenhouseData;
                    var expected = 555.0;

                    using (var model = GetRRModel())
                    {
                        model.Basin.Catchments.Add(firstCatchment);
                        greenhouseData = (GreenhouseData) model.GetCatchmentModelData(firstCatchment);
                        greenhouseData.TotalAreaUnit = RainfallRunoffEnums.AreaUnit.m2;
                        ReflectionTestHelper.FillRandomValuesForValueTypeProperties(greenhouseData, new[]
                        {
                            nameof(greenhouseData.TotalAreaUnit),
                        });
                        greenhouseData.UseSubsoilStorage = true;
                        greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from2500to3000] =
                            expected;
                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    var retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>()
                        .FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedGreenhouse =
                        retrievedModel.GetAllModelData().FirstOrDefault(data =>
                            Equals(data.Catchment.CatchmentType, CatchmentType.GreenHouse)) as GreenhouseData;
                    Assert.NotNull(retrievedGreenhouse);

                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedGreenhouse.Catchment);

                    // not testing this in this test:
                    catchment.LongName = firstCatchment.LongName;
                    retrievedGreenhouse.MeteoStationName = greenhouseData.MeteoStationName;
                    retrievedGreenhouse.TemperatureStationName = greenhouseData.TemperatureStationName;
                    retrievedGreenhouse.AreaAdjustmentFactor = greenhouseData.AreaAdjustmentFactor;

                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(greenhouseData, retrievedGreenhouse);
                    Assert.AreEqual(greenhouseData.TotalAreaUnit, retrievedGreenhouse.TotalAreaUnit);
                    Assert.AreEqual(expected,
                                    greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from2500to3000]);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }

        [Test]
        public void SaveAndLoadHbvData()
        {
            string path = null;
            try
            {
                using (var app = CreateRunningApplication())
                {
                    IProjectService projectService = app.ProjectService;
                    var firstCatchment = new Catchment
                        {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Hbv};

                    HbvData hbvData;
                    using (var model = GetRRModel())
                    {
                        model.Basin.Catchments.Add(firstCatchment);
                        hbvData = (HbvData) model.GetCatchmentModelData(firstCatchment);
                        ReflectionTestHelper.FillRandomValuesForValueTypeProperties(hbvData);
                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }
                    
                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedHbvData =
                        (HbvData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedHbvData.Catchment);

                    // not testing this in this test:
                    catchment.LongName = firstCatchment.LongName;
                    retrievedHbvData.StationName = hbvData.StationName;
                    retrievedHbvData.TemperatureStationName = hbvData.TemperatureStationName;

                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(hbvData, retrievedHbvData);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }


        [Test]
        public void SaveAndLoadMeteorogicalDataPerStation()
        {
            string path = null;
            try
            {
                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    using (var model = GetRRModel())
                    {
                        projectService.Project.RootFolder.Add(model);
                        model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
                        model.Evaporation.DataDistributionType = MeteoDataDistributionType.PerStation;
                        model.Temperature.DataDistributionType = MeteoDataDistributionType.PerStation;
                        model.ModelData.Add(new HbvData(new Catchment()));// meteo stations are only for HBV
                        model.MeteoStations.Add("Station A");
                        model.MeteoStations.Add("Station B");
                        model.MeteoStations.Add("Station C");

                        model.Precipitation.Data[new DateTime(2001, 2, 3), "Station A"] = 12.0;
                        model.Precipitation.Data[new DateTime(2001, 2, 4), "Station A"] = 2.0;
                        model.Evaporation.Data[new DateTime(2001, 2, 3), "Station B"] = 80.1;
                        model.Evaporation.Data[new DateTime(2001, 2, 4), "Station B"] = 80.2;
                        model.Temperature.Data[new DateTime(2001, 2, 3), "Station B"] = 18.1;
                        model.Temperature.Data[new DateTime(2001, 2, 4), "Station C"] = 27.3;
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>()
                        .FirstOrDefault();
                    Assert.NotNull(retrievedModel);

                    Assert.AreEqual(3, retrievedModel.MeteoStations.Count);
                    Assert.AreEqual("Station A", retrievedModel.MeteoStations[0]);
                    Assert.AreEqual("Station B", retrievedModel.MeteoStations[1]);
                    Assert.AreEqual("Station C", retrievedModel.MeteoStations[2]);

                    Assert.AreEqual(MeteoDataDistributionType.PerStation,
                        retrievedModel.Precipitation.DataDistributionType);

                    Assert.AreEqual(12, retrievedModel.Precipitation.Data[new DateTime(2001, 2, 3), "Station A"]);
                    Assert.AreEqual(2, retrievedModel.Precipitation.Data[new DateTime(2001, 2, 4), "Station A"]);
                    Assert.AreEqual(3, retrievedModel.Precipitation.Data.Arguments[1].Values.Count);
                    Assert.AreEqual("Station A", retrievedModel.Precipitation.Data.Arguments[1].Values[0]);
                    Assert.AreEqual("Station B", retrievedModel.Precipitation.Data.Arguments[1].Values[1]);
                    Assert.AreEqual("Station C", retrievedModel.Precipitation.Data.Arguments[1].Values[2]);

                    Assert.AreEqual(MeteoDataDistributionType.PerStation,
                        retrievedModel.Evaporation.DataDistributionType);
                    Assert.AreEqual(80.1, retrievedModel.Evaporation.Data[new DateTime(2001, 2, 3), "Station B"]);
                    Assert.AreEqual(80.2, retrievedModel.Evaporation.Data[new DateTime(2001, 2, 4), "Station B"]);
                    Assert.AreEqual(3, retrievedModel.Evaporation.Data.Arguments[1].Values.Count);
                    Assert.AreEqual("Station A", retrievedModel.Evaporation.Data.Arguments[1].Values[0]);
                    Assert.AreEqual("Station B", retrievedModel.Evaporation.Data.Arguments[1].Values[1]);
                    Assert.AreEqual("Station C", retrievedModel.Evaporation.Data.Arguments[1].Values[2]);

                    Assert.AreEqual(MeteoDataDistributionType.PerStation,
                        retrievedModel.Temperature.DataDistributionType);
                    Assert.AreEqual(18.1, retrievedModel.Temperature.Data[new DateTime(2001, 2, 3), "Station B"]);
                    Assert.AreEqual(27.3, retrievedModel.Temperature.Data[new DateTime(2001, 2, 4), "Station C"]);
                    Assert.AreEqual(2, retrievedModel.Temperature.Data.Arguments[1].Values.Count);
                    Assert.AreEqual("Station B", retrievedModel.Temperature.Data.Arguments[1].Values[0]);
                    Assert.AreEqual("Station C", retrievedModel.Temperature.Data.Arguments[1].Values[1]);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }

        [Test]
        public void SaveAndLoadUnpavedSimple()
        {
            string path = null;
            try
            {
                var firstCatchment = new Catchment
                    {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Unpaved};

                var model1 = GetRRModel();
                model1.Basin.Catchments.Add(firstCatchment);

                var unpavedData = (UnpavedData) model1.GetCatchmentModelData(firstCatchment);
                unpavedData.CalculationArea = 801;
                unpavedData.TotalAreaForGroundWaterCalculations = 802;
                unpavedData.UseDifferentAreaForGroundWaterCalculations = true;

                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    projectService.Project.RootFolder.Items.Add(model1);
                    path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    
                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedUnpavedData =
                        (UnpavedData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    Assert.IsNotNull(retrievedUnpavedData);
                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedUnpavedData.Catchment);

                    Assert.IsTrue(retrievedUnpavedData.UseDifferentAreaForGroundWaterCalculations);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }
        
        [Test]
        public void SaveAndLoadUnpavedData()
        {
            string path = null;
            try
            {
                var firstCatchment = new Catchment
                    {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Unpaved};

                var model1 = GetRRModel();
                model1.Basin.Catchments.Add(firstCatchment);

                var unpavedData = (UnpavedData) model1.GetCatchmentModelData(firstCatchment);

                var expected = 555.0;

                var drainageFormula = new ErnstDrainageFormula();
                unpavedData.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
                unpavedData.SeepageSource = UnpavedEnums.SeepageSourceType.H0Series;
                unpavedData.SoilType = UnpavedEnums.SoilType.clay_minimum;
                unpavedData.SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_10;

                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(unpavedData, new[]
                {
                    nameof(unpavedData.InitialGroundWaterLevelSource),
                    nameof(unpavedData.SeepageSource),
                    nameof(unpavedData.SoilType),
                    nameof(unpavedData.DrainageFormula),
                    nameof(unpavedData.InitialGroundWaterLevelSeries),
                    nameof(unpavedData.SeepageSeries),
                    nameof(unpavedData.SeepageH0Series),
                });
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(drainageFormula, new[]
                {
                    nameof(drainageFormula.LevelOneEnabled),
                    nameof(drainageFormula.LevelTwoEnabled),
                    nameof(drainageFormula.LevelThreeEnabled),
                });
                drainageFormula.LevelOneEnabled = true;
                drainageFormula.LevelTwoEnabled = true;
                drainageFormula.LevelThreeEnabled = true;
                unpavedData.DrainageFormula = drainageFormula;
                unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass] = expected;
                var seepageSeries = new TimeSeries();
                seepageSeries.Components.Add(new Variable<double>("Value"));
                seepageSeries[new DateTime((int) expected)] = expected;
                seepageSeries[new DateTime((int) expected * 2)] = expected * 2;
                unpavedData.SeepageSeries = seepageSeries;

                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    projectService.Project.RootFolder.Items.Add(model1);
                    path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedUnpaved =
                        (UnpavedData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedUnpaved.Catchment);

                    // not testing this in this test:
                    catchment.LongName = firstCatchment.LongName;
                    retrievedUnpaved.SoilTypeCapsim = unpavedData.SoilTypeCapsim;
                    retrievedUnpaved.InitialGroundWaterLevelSource = unpavedData.InitialGroundWaterLevelSource;
                    retrievedUnpaved.InitialGroundWaterLevelConstant = unpavedData.InitialGroundWaterLevelConstant;
                    retrievedUnpaved.MaximumLandStorage = unpavedData.MaximumLandStorage;
                    retrievedUnpaved.InitialLandStorage = unpavedData.InitialLandStorage;
                    retrievedUnpaved.InfiltrationCapacity = unpavedData.InfiltrationCapacity;
                    retrievedUnpaved.SeepageConstant = unpavedData.SeepageConstant;
                    retrievedUnpaved.MeteoStationName = unpavedData.MeteoStationName;
                    retrievedUnpaved.TemperatureStationName = unpavedData.TemperatureStationName;
                    retrievedUnpaved.AreaAdjustmentFactor = unpavedData.AreaAdjustmentFactor;

                    var retrievedDrainageFormula = retrievedUnpaved.DrainageFormula as ErnstDrainageFormula;
                    Assert.IsNotNull(retrievedDrainageFormula);
                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(unpavedData, retrievedUnpaved);
                    Assert.AreEqual(unpavedData.InitialGroundWaterLevelSource,
                        retrievedUnpaved.InitialGroundWaterLevelSource);
                    Assert.AreEqual(unpavedData.SeepageSource, retrievedUnpaved.SeepageSource);
                    Assert.AreEqual(unpavedData.SoilType, retrievedUnpaved.SoilType);
                    Assert.AreEqual(unpavedData.SoilTypeCapsim, retrievedUnpaved.SoilTypeCapsim);
                    Assert.AreEqual(expected, unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass]);

                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(drainageFormula, retrievedDrainageFormula);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }
        
        [Test]
        public void SaveAndLoadSacramentoData()
        {
            string path = null;
            try
            {
                var firstCatchment = new Catchment
                    {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Sacramento};

                

                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    
                    SacramentoData sacramentoData;
                    var rng = new Random();

                    using (var model = GetRRModel())
                    {
                        model.Basin.Catchments.Add(firstCatchment);

                        sacramentoData = (SacramentoData) model.GetCatchmentModelData(firstCatchment);

                        ReflectionTestHelper.FillRandomValuesForValueTypeProperties(sacramentoData);

                        for (var i = 0; i < 36; ++i)
                        {
                            sacramentoData.HydrographValues[i] = rng.NextDouble();
                        }

                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedSacramentoData =
                        (SacramentoData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedSacramentoData.Catchment);

                    // not testing this in this test:
                    catchment.LongName = firstCatchment.LongName;
                    retrievedSacramentoData.StationName = sacramentoData.StationName;
                    retrievedSacramentoData.TemperatureStationName = sacramentoData.TemperatureStationName;
                    retrievedSacramentoData.AreaAdjustmentFactor = sacramentoData.AreaAdjustmentFactor;

                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(sacramentoData, retrievedSacramentoData);
                    for (int i = 0; i < 36; i++)
                    {
                        Assert.AreEqual(sacramentoData.HydrographValues[i], retrievedSacramentoData.HydrographValues[i],
                            0.00001);
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }
        
        [Test]
        public void SaveAndLoadRunoffBoundaryData()
        {
            string path = null;
            try
            {
                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    var boundary = new RunoffBoundary {Name = "Boundary1"};

                    using (var model = GetRRModel())
                    {
                        model.Basin.Boundaries.Add(boundary);
                        var boundaryData = model.BoundaryData.First(bd => bd.Boundary == boundary);
                        boundaryData.Series.Data[new DateTime(2005, 1, 1)] = 15.0;
                        boundaryData.Series.Value = 11.0;

                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedBoundary = retrievedModel.Basin.Boundaries.FirstOrDefault();
                    Assert.NotNull(retrievedBoundary);
                    var retrievedBoundaryData = retrievedModel.BoundaryData.FirstOrDefault();
                    Assert.NotNull(retrievedBoundaryData);
                    Assert.AreSame(retrievedBoundary, retrievedBoundaryData.Boundary);
                    Assert.AreEqual(15.0, retrievedBoundaryData.Series.Data.Components[0].Values[0]);
                    Assert.AreEqual(11.0, retrievedBoundaryData.Series.Value);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }

        
        [Test]
        public void SaveAndLoadPavedData()
        {
            string path = null;
            try
            {
                var firstCatchment = new Catchment
                    {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Paved};
                var expected = 555.0;
                PavedData pavedData;

                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    TimeSeries sewerPumpVariableCapacitySeries;
                    TimeSeries mixedSewerPumpVariableCapacitySeries;
                    Function variableWaterUseFunction;
                    using (var model = GetRRModel())
                    {
                        model.Basin.Catchments.Add(firstCatchment);

                        pavedData = (PavedData) model.GetCatchmentModelData(firstCatchment);

                        pavedData.DryWeatherFlowOptions =
                            PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesVariableDWF;
                        pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                        pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                        pavedData.SewerType = PavedEnums.SewerType.ImprovedSeparateSystem;
                        pavedData.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;

                        ReflectionTestHelper.FillRandomValuesForValueTypeProperties(pavedData, new[]
                        {
                            nameof(pavedData.DryWeatherFlowOptions),
                            nameof(pavedData.DryWeatherFlowSewerPumpDischarge),
                            nameof(pavedData.MixedAndOrRainfallSewerPumpDischarge),
                            nameof(pavedData.SewerType),
                            nameof(pavedData.SpillingDefinition),
                        });

                        sewerPumpVariableCapacitySeries = new TimeSeries();
                        sewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value1")
                            {InterpolationType = InterpolationType.Linear});
                        sewerPumpVariableCapacitySeries[DateTime.Today] = new[] {expected};
                        sewerPumpVariableCapacitySeries[DateTime.Today + TimeSpan.FromDays(1)] = new[] {expected * 3};
                        pavedData.MixedSewerPumpVariableCapacitySeries = sewerPumpVariableCapacitySeries;

                        mixedSewerPumpVariableCapacitySeries = new TimeSeries();
                        mixedSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value3")
                            {InterpolationType = InterpolationType.Linear});
                        mixedSewerPumpVariableCapacitySeries[DateTime.Today] = new[] {expected};
                        mixedSewerPumpVariableCapacitySeries[DateTime.Today + TimeSpan.FromDays(1)] =
                            new[] {expected * 3};
                        pavedData.DwfSewerPumpVariableCapacitySeries = mixedSewerPumpVariableCapacitySeries;

                        variableWaterUseFunction = new Function();
                        variableWaterUseFunction.Arguments.Add(new Variable<int>("Argument"));
                        variableWaterUseFunction.Components.Add(new Variable<double>("Component"));
                        variableWaterUseFunction[0] = 5.6;
                        variableWaterUseFunction[1] = 11.1;
                        variableWaterUseFunction[2] = 83.3;
                        //must be 100 in total
                        for (int i = 3; i < 24; i++)
                        {
                            variableWaterUseFunction[i] = 0d;
                        }

                        pavedData.VariableWaterUseFunction = variableWaterUseFunction;
                        pavedData.WaterUse = variableWaterUseFunction.GetValues<double>().Sum();
                        pavedData.IsSewerPumpCapacityFixed = false;
                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                        .OfType<RainfallRunoffModel>()
                        .FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedPaved =
                        (PavedData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                    Assert.NotNull(catchment);
                    Assert.AreEqual(catchment, retrievedPaved.Catchment);

                    // not testing this in this test:
                    catchment.LongName = firstCatchment.LongName;
                    retrievedPaved.CapacityMixedAndOrRainfall = pavedData.CapacityMixedAndOrRainfall;
                    retrievedPaved.CapacityDryWeatherFlow = pavedData.CapacityDryWeatherFlow;
                    retrievedPaved.WaterUse = pavedData.WaterUse;
                    retrievedPaved.MeteoStationName = pavedData.MeteoStationName;
                    retrievedPaved.TemperatureStationName = pavedData.TemperatureStationName;
                    retrievedPaved.AreaAdjustmentFactor = pavedData.AreaAdjustmentFactor;

                    var retrievedMixedSeries = retrievedPaved.MixedSewerPumpVariableCapacitySeries;
                    Assert.IsNotNull(retrievedMixedSeries);
                    var retrievedDwfSeries = retrievedPaved.DwfSewerPumpVariableCapacitySeries;
                    Assert.IsNotNull(retrievedDwfSeries);
                    var retrievedFunction = retrievedPaved.VariableWaterUseFunction;
                    Assert.IsNotNull(retrievedFunction);

                    ReflectionTestHelper.AssertPublicPropertiesAreEqual(pavedData, retrievedPaved);
                    Assert.AreEqual(pavedData.DryWeatherFlowOptions, retrievedPaved.DryWeatherFlowOptions);
                    Assert.AreEqual(pavedData.DryWeatherFlowSewerPumpDischarge,
                        retrievedPaved.DryWeatherFlowSewerPumpDischarge);
                    Assert.AreEqual(pavedData.MixedAndOrRainfallSewerPumpDischarge,
                        retrievedPaved.MixedAndOrRainfallSewerPumpDischarge);
                    Assert.AreEqual(pavedData.SewerType, retrievedPaved.SewerType);
                    Assert.AreEqual(pavedData.SpillingDefinition, retrievedPaved.SpillingDefinition);
                    Assert.AreEqual(mixedSewerPumpVariableCapacitySeries.Components.First().Values,
                        retrievedMixedSeries.Components.First().Values);
                    Assert.AreEqual(mixedSewerPumpVariableCapacitySeries.Components.Last().Values,
                        retrievedMixedSeries.Components.Last().Values);
                    Assert.AreEqual(sewerPumpVariableCapacitySeries.Components.First().Values,
                        retrievedDwfSeries.Components.First().Values);
                    Assert.AreEqual(sewerPumpVariableCapacitySeries.Components.Last().Values,
                        retrievedDwfSeries.Components.Last().Values);
                    Assert.AreEqual(variableWaterUseFunction.Components.First().GetValues<double>(),
                        retrievedFunction.Components.First().GetValues<double>());

                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }
        
        [Test]
        public void SaveLoadModel()
        {
            string path = null;
            try
            {
                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    using (var model = GetRRModel())
                    {
                        model.AreaUnit = RainfallRunoffEnums.AreaUnit.ha;
                        model.OutputTimeStep = new TimeSpan(0, 0, 1, 0);
                        model.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).IsEnabled = true;
                        model.StartTime = new DateTime(2000, 1, 1);
                        model.StopTime = new DateTime(2000, 3, 1);
                        model.CapSim = true;
                        model.CapSimInitOption = RainfallRunoffEnums.CapsimInitOptions.AtMoistureContentpF2;
                        model.CapSimCropAreaOption =
                            RainfallRunoffEnums.CapsimCropAreaOptions.AveragedDataPerUnpavedArea;

                        model.UseSaveStateTimeRange = true;
                        model.SaveStateStartTime = model.StartTime;
                        model.SaveStateTimeStep = model.TimeStep;
                        model.SaveStateStopTime = model.StopTime;

                        projectService.Project.RootFolder.Items.Add(model);
                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);

                        Project retrievedProject = projectService.OpenProject(path);
                        var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                            .OfType<RainfallRunoffModel>()
                            .FirstOrDefault();
                        Assert.NotNull(retrievedModel);

                        Assert.AreEqual(model.Basin, retrievedModel.Basin);
                        Assert.AreEqual(model.Precipitation, retrievedModel.Precipitation);
                        Assert.AreEqual(model.Evaporation, retrievedModel.Evaporation);
                        Assert.AreEqual(model.AreaUnit, retrievedModel.AreaUnit);
                        Assert.AreEqual(model.StartTime, retrievedModel.StartTime);
                        Assert.AreEqual(model.StopTime, retrievedModel.StopTime);
                        Assert.AreEqual(model.OutputTimeStep, retrievedModel.OutputTimeStep);
                        Assert.AreEqual(model.CapSim, retrievedModel.CapSim);
                        Assert.AreEqual(model.CapSimInitOption, retrievedModel.CapSimInitOption);
                        Assert.AreEqual(model.CapSimCropAreaOption, retrievedModel.CapSimCropAreaOption);
                        Assert.AreEqual(true,
                            retrievedModel.OutputSettings.GetEngineParameter(QuantityType.Rainfall,
                                ElementSet.UnpavedElmSet).IsEnabled);

                        Assert.AreEqual(model.UseSaveStateTimeRange, retrievedModel.UseSaveStateTimeRange);
                        Assert.AreEqual(model.SaveStateStartTime, retrievedModel.SaveStateStartTime);
                        Assert.AreEqual(model.SaveStateTimeStep, retrievedModel.SaveStateTimeStep);
                        Assert.AreEqual(model.SaveStateStopTime, retrievedModel.SaveStateStopTime);
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }

        [Test]
        public void SaveLoadCatchment()
        {
            var catchment = new Catchment
            {
                Name = "testName",
                LongName = "testName",
                Geometry =
                    new Polygon(
                    new LinearRing(new[]
                                                           {
                                                               new Coordinate(0, 0), new Coordinate(10, 0),
                                                               new Coordinate(10, 10), new Coordinate(0, 0)
                                                           })),
                IsGeometryDerivedFromAreaSize = true,
                CatchmentType = CatchmentType.GreenHouse
            };

            catchment.SetAreaSize(500);

            var basin = new DrainageBasin();
            basin.Catchments.Add(catchment);

            var retrievedBasin = SaveLoadObject(basin, "basin");

            Assert.AreEqual(1, retrievedBasin.Catchments.Count);
            var retrievedCatchment = retrievedBasin.Catchments.First();
            Assert.AreEqual(catchment.Name, retrievedCatchment.Name);
            Assert.AreEqual(retrievedBasin, retrievedCatchment.Basin);
            Assert.AreEqual(catchment.LongName, retrievedCatchment.LongName);
            Assert.AreEqual(catchment.Geometry.Coordinates.Length, retrievedCatchment.Geometry.Coordinates.Length);
            for (int i = 0; i < catchment.Geometry.Coordinates.Length; i++)
            {
                Assert.AreEqual(catchment.Geometry.Coordinates[i].X, retrievedCatchment.Geometry.Coordinates[i].X, 1 );
                Assert.AreEqual(catchment.Geometry.Coordinates[i].Y, retrievedCatchment.Geometry.Coordinates[i].Y, 1 );
            }
            
            Assert.AreEqual(catchment.GeometryArea, retrievedCatchment.GeometryArea, 0.001);
            Assert.AreEqual(catchment.CatchmentType, retrievedCatchment.CatchmentType);
        }

        [Test]
        public void SaveLoadDefaultGeometryCatchment()
        {
            // add catchment
            var catchment = new Catchment
            {
                IsGeometryDerivedFromAreaSize = true,
                CatchmentType = CatchmentType.GreenHouse
            };
            catchment.SetAreaSize(500);


            var basin = new DrainageBasin();
            basin.Catchments.Add(catchment);

            var retrievedBasin = SaveLoadObject(basin, "basin");

            Assert.AreEqual(1, retrievedBasin.Catchments.Count);
            var retrievedCatchment = retrievedBasin.Catchments.First();

            var oldArea = retrievedCatchment.Geometry.Area;

            retrievedCatchment.SetAreaSize(retrievedCatchment.GeometryArea * 2);

            Assert.AreNotEqual(oldArea, retrievedCatchment.Geometry.Area); //check if change in area, changes geometry (when IsGeometryDerivedFromAreaSize)
        }
        
        [Test]
        public void SaveLoadWasteWaterTreatmentPlant()
        {
            var wwtp = new WasteWaterTreatmentPlant { Name = "testName", Description = "testDescr", Geometry = new Point(55, 33) };

            var basin = new DrainageBasin();
            basin.WasteWaterTreatmentPlants.Add(wwtp);

            var retrievedBasin = SaveLoadObject(basin, "basin");

            Assert.AreEqual(1, retrievedBasin.WasteWaterTreatmentPlants.Count);
            var retrievedWwtp = retrievedBasin.WasteWaterTreatmentPlants.First();
            Assert.AreEqual(wwtp.Geometry, retrievedWwtp.Geometry);
            Assert.AreEqual(wwtp.Name, retrievedWwtp.Name);
            Assert.AreEqual(basin, retrievedWwtp.Basin);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)] 
        [Category(TestCategory.Integration)] 
        public void SaveAndLoad_UnpavedDataExtended()
        {
            string path = null;
            var useLocalBoundaryData = true;
            try
            {
                var firstCatchment = new Catchment
                    {Name = ORIGINAL_CATCHMENT_NAME, CatchmentType = CatchmentType.Unpaved};

                var rrModel = GetRRModel();
                rrModel.Basin.Catchments.Add(firstCatchment);

                var unpavedData = (UnpavedData) rrModel.GetCatchmentModelData(firstCatchment);

                unpavedData.BoundarySettings.UseLocalBoundaryData = useLocalBoundaryData;

                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    projectService.Project.RootFolder.Items.Add(rrModel);
                    path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);

                    Project retrievedProject = projectService.OpenProject(path);
                    var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                                                         .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                    var retrievedUnpavedData =
                        (UnpavedData) retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                    Assert.IsNotNull(retrievedUnpavedData);

                    Assert.AreEqual(useLocalBoundaryData,retrievedUnpavedData.BoundarySettings.UseLocalBoundaryData);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)] 
        [Category(TestCategory.Integration)] 
        public void GivenRRModelWithStartTimeAfterStopTime_WhenSaveAndLoad_ThenNoExceptionIsThrown()
        {
            string path = null;
            try
            {
                RainfallRunoffModel rrModel = GetRRModel();
                rrModel.StopTime = new DateTime(2024, 5, 8, 10, 0, 0);
                rrModel.StartTime = new DateTime(2024, 5, 8, 12, 0, 0); // Start time after stop time
                rrModel.OutputTimeStep = TimeSpan.FromHours(1); 

                using (IApplication application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    projectService.Project.RootFolder.Items.Add(rrModel);
                    Assert.DoesNotThrow(() => path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService));

                    Project retrievedProject = projectService.OpenProject(path);
                    RainfallRunoffModel retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive()
                                                                         .OfType<RainfallRunoffModel>().FirstOrDefault();
                    Assert.NotNull(retrievedModel);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(path));
            }
        }



        private RainfallRunoffModel GetRRModel()
        {
            var model = new RainfallRunoffModel { Name = "rr" };
            model.StartTime = new DateTime(2011, 2, 2);
            model.Basin = new DrainageBasin { Name = "" };
            return model;
        }

        private string SaveAndCloseProjectWithThisRrModel(string methodName, IProjectService projectService)
        {
            var tempFolder = FileUtils.CreateTempDirectory();

            var path = Path.Combine(tempFolder, methodName + ".dsproj");
            projectService.SaveProjectAs(path);
            List<RainfallRunoffModel> rrModels = projectService.Project.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>().ToList();

            rrModels.ForEach(rrModel => projectService.Project.RootFolder.Items.Remove(rrModel));
            projectService.CloseProject();
            ;
            return path;
        }

        private static IApplication CreateRunningApplication()
        {
            var app = new DHYDROApplicationBuilder().WithRainfallRunoff().Build();
            app.Run();
            app.ProjectService.CreateProject();
            return app;
        }

        /// <summary>
        /// Saves and retrieves an object by wrapping it in a dataitem in the rootfolder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="path">Project path used to save / load</param>
        /// <returns></returns>
        protected T SaveLoadObject<T>(T o, string onThis) where T : class
        {
            string path = string.Empty;

            try
            {
                using (var application = CreateRunningApplication())
                {
                    IProjectService projectService = application.ProjectService;
                    using (var model = new RainfallRunoffModel())
                    {

                        switch (onThis)
                        {
                            case "catchments":
                                var catchment = o as Catchment;
                                if (catchment != null)
                                {
                                    projectService.Project.RootFolder.Add(model);
                                    model.Basin.Catchments.Add(catchment);
                                }

                                break;
                            case "basin":
                                var basin = o as DrainageBasin;
                                if (basin != null)
                                {
                                    projectService.Project.RootFolder.Add(model);
                                    model.Basin = basin;
                                }

                                break;
                            case "wwtp":
                                var wwtp = o as WasteWaterTreatmentPlant;
                                if (wwtp != null)
                                {
                                    projectService.Project.RootFolder.Add(model);
                                    model.Basin.WasteWaterTreatmentPlants.Add(wwtp);
                                }

                                break;
                            default:
                                projectService.Project.RootFolder.Add(o);
                                break;
                        }

                        path = SaveAndCloseProjectWithThisRrModel(TestHelper.GetCurrentMethodName(), projectService);
                    }

                    Project retrievedProject = projectService.OpenProject(path);
                    RainfallRunoffModel retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>()
                                                                         .FirstOrDefault();
                    Assert.IsNotNull(retrievedModel);
                    switch (onThis)
                    {
                        case "catchments":
                            var catchment = o as Catchment;
                            if (catchment != null)
                            {
                                return retrievedModel.Basin.Catchments.FirstOrDefault() as T;
                            }

                            break;
                        case "basin":
                            var basin = o as DrainageBasin;
                            if (basin != null)
                            {
                                return retrievedModel.Basin as T;
                            }

                            break;
                        case "wwtp":
                            var wwtp = o as WasteWaterTreatmentPlant;
                            if (wwtp != null)
                            {
                                return retrievedModel.Basin.WasteWaterTreatmentPlants.FirstOrDefault() as T;
                            }

                            break;
                    }

                    return (T)((DataItem)retrievedProject.RootFolder.Items[0]).Value;
                }
            }
            finally
            {
                //FileUtils.DeleteIfExists(path);
            }

        }
    }

}