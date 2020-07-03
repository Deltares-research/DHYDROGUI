using System;
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
using DeltaShell.Core;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffModelDataSaveLoadTest
    {
        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void SaveAndLoadGreenHouseData()
        {
            Project project;
            IApplication application;
            var model = GetRRModel(out project, out application);

            var name = "MyFirstCatchment";
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.GreenHouse };

            model.Basin.Catchments.Add(firstCatchment);

            var expected = 555.0;

            var greenhouseData = (GreenhouseData)model.GetCatchmentModelData(firstCatchment);

            greenhouseData.TotalAreaUnit = RainfallRunoffEnums.AreaUnit.km2;
            greenhouseData.RoofStorageUnit = RainfallRunoffEnums.StorageUnit.mm;

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(greenhouseData, new[]
                    {
                        nameof(greenhouseData.TotalAreaUnit),
                        nameof(greenhouseData.RoofStorageUnit)
                    });

            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from2500to3000] = expected;
            var tempFolder = FileUtils.CreateTempDirectory();
            string path = Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".dsproj");

            application.SaveProjectAs(path);
            application.CloseProject();
            application.Dispose();

            using (var app = CreateRunningDeltaShellApplication())
            {
                app.OpenProject(path);
                var retrievedProject = app.Project;
                var retrievedModel = retrievedProject.RootFolder.GetAllModelsRecursive().OfType<RainfallRunoffModel>().FirstOrDefault();
                Assert.NotNull(retrievedModel);
                var retrievedGreenhouse = retrievedModel.GetAllModelData().FirstOrDefault(data => Equals(data.Catchment.CatchmentType, CatchmentType.GreenHouse)) as GreenhouseData;
                Assert.NotNull(retrievedGreenhouse);

                var catchment = retrievedModel.Basin.Catchments.FirstOrDefault();
                Assert.NotNull(catchment);
                Assert.AreEqual(catchment, retrievedGreenhouse.Catchment);
                ReflectionTestHelper.AssertPublicPropertiesAreEqual(greenhouseData, retrievedGreenhouse);
                Assert.AreEqual(greenhouseData.TotalAreaUnit, retrievedGreenhouse.TotalAreaUnit);
                Assert.AreEqual(greenhouseData.RoofStorageUnit, retrievedGreenhouse.RoofStorageUnit);
                Assert.AreEqual(expected,
                                greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from2500to3000]);
            }
        }
        /*
        [Test]
        public void SaveAndLoadHbvData()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            var model = GetRRModel(out project, out hybridProjectRepository);

            var name = "MyFirstCatchment";
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.Hbv };

            model.Basin.Catchments.Add(firstCatchment);

            var hbvData = (HbvData)model.GetCatchmentModelData(firstCatchment);

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(hbvData);

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = retrievedProject.RootFolder.DataItems.First().Value as RainfallRunoffModel;
                var retrievedHbvData = (HbvData)retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());

                ReflectionTestHelper.AssertPublicPropertiesAreEqual(hbvData, retrievedHbvData);
            }
        }
        [Test]
        public void SaveAndLoadMeteorogicalDataPerStation()
        {
            string path = "ps.dsproj";

            using (projectRepository = factory.CreateNew())
            {
                var project = new Project();
                var model = new RainfallRunoffModel { Name = "rr" };
                project.RootFolder.Add(model);
                model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
                model.MeteoStations.Add("Station A");
                model.MeteoStations.Add("Station B");

                model.Precipitation.Data[new DateTime(2001, 2, 3), "Station A"] = 12.0;

                projectRepository.SaveAs(project, path);
            }

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel)retrievedProject.RootFolder.Models.First();

                Assert.AreEqual(2, retrievedModel.MeteoStations.Count);
                Assert.AreEqual("Station A", retrievedModel.MeteoStations[0]);

                Assert.AreEqual(MeteoDataDistributionType.PerStation,
                    retrievedModel.Precipitation.DataDistributionType);

                Assert.AreEqual(12, retrievedModel.Precipitation.Data[new DateTime(2001, 2, 3), "Station A"]);
                Assert.AreEqual(2, retrievedModel.Precipitation.Data.Arguments[1].Values.Count);
                Assert.AreEqual("Station A", retrievedModel.Precipitation.Data.Arguments[1].Values[0]);

                Assert.AreEqual(MeteoDataDistributionType.PerStation,
                    retrievedModel.Evaporation.DataDistributionType);
                Assert.AreEqual(2, retrievedModel.Evaporation.Data.Arguments[1].Values.Count);
            }
        }
        [Test]
        public void SaveAndLoadUnpavedSimple()
        {
            var name = "MyFirstCatchment";
            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.Unpaved };

            model.Basin.Catchments.Add(firstCatchment);

            var unpavedData = (UnpavedData)model.GetCatchmentModelData(firstCatchment);
            unpavedData.UseDifferentAreaForGroundWaterCalculations = true;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel)retrievedProject.RootFolder.DataItems.First().Value;

                var retrievedUnpavedData =
                    (UnpavedData)retrievedModel.GetCatchmentModelData(model.Basin.Catchments.First());
                Assert.IsNotNull(retrievedUnpavedData);
                Assert.IsTrue(retrievedUnpavedData.UseDifferentAreaForGroundWaterCalculations);
            }
        }

        [Test]
        public void SaveAndLoadUnpavedData()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            var model = GetRRModel(out project, out hybridProjectRepository);

            var name = "MyFirstCatchment";
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.Unpaved };

            model.Basin.Catchments.Add(firstCatchment);

            var unpavedData = (UnpavedData)model.GetCatchmentModelData(firstCatchment);

            var expected = 555.0;

            var drainageFormula = new ErnstDrainageFormula();
            unpavedData.InfiltrationCapacityUnit = RainfallRunoffEnums.RainfallCapacityUnit.mm_day;
            unpavedData.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
            unpavedData.SeepageSource = UnpavedEnums.SeepageSourceType.H0Series;
            unpavedData.LandStorageUnit = RainfallRunoffEnums.StorageUnit.m3;
            unpavedData.SoilType = UnpavedEnums.SoilType.clay_minimum;
            unpavedData.SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_10;

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(unpavedData, new[]
                {
                    nameof(unpavedData.InfiltrationCapacityUnit),
                    nameof(unpavedData.InitialGroundWaterLevelSource),
                    nameof(unpavedData.SeepageSource),
                    nameof(unpavedData.LandStorageUnit),
                    nameof(unpavedData.SoilType),
                    nameof(unpavedData.DrainageFormula),
                    nameof(unpavedData.InitialGroundWaterLevelSeries),
                    nameof(unpavedData.SeepageSeries),
                    nameof(unpavedData.SeepageH0Series),
                });
            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(drainageFormula);

            unpavedData.DrainageFormula = drainageFormula;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass] = expected;
            var seepageSeries = new TimeSeries();
            seepageSeries.Components.Add(new Variable<double>("Value"));
            seepageSeries[new DateTime((int)expected)] = expected;
            seepageSeries[new DateTime((int)expected * 2)] = expected * 2;
            unpavedData.SeepageSeries = seepageSeries;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = retrievedProject.RootFolder.DataItems.First().Value as RainfallRunoffModel;
                var retrievedUnpaved =
                    (UnpavedData)retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
                var retrievedSeries = retrievedUnpaved.SeepageSeries;
                var retrievedDrainageFormula = retrievedUnpaved.DrainageFormula as ErnstDrainageFormula;

                ReflectionTestHelper.AssertPublicPropertiesAreEqual(unpavedData, retrievedUnpaved);
                Assert.AreEqual(unpavedData.InfiltrationCapacityUnit, retrievedUnpaved.InfiltrationCapacityUnit);
                Assert.AreEqual(unpavedData.InitialGroundWaterLevelSource,
                                retrievedUnpaved.InitialGroundWaterLevelSource);
                Assert.AreEqual(unpavedData.SeepageSource, retrievedUnpaved.SeepageSource);
                Assert.AreEqual(unpavedData.LandStorageUnit, retrievedUnpaved.LandStorageUnit);
                Assert.AreEqual(unpavedData.SoilType, retrievedUnpaved.SoilType);
                Assert.AreEqual(unpavedData.SoilTypeCapsim, retrievedUnpaved.SoilTypeCapsim);
                Assert.AreEqual(expected, unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass]);
                Assert.AreEqual(seepageSeries.Components.First().Values, retrievedSeries.Components.First().Values);

                ReflectionTestHelper.AssertPublicPropertiesAreEqual(drainageFormula, retrievedDrainageFormula);
            }
        }

        [Test]
        public void SaveAndLoadSacramentoData()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            var model = GetRRModel(out project, out hybridProjectRepository);

            var name = "MyFirstCatchment";
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.Sacramento };

            model.Basin.Catchments.Add(firstCatchment);

            var sacramentoData = (SacramentoData)model.GetCatchmentModelData(firstCatchment);

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(sacramentoData);

            var rng = new Random();
            for (var i = 0; i < 36; ++i)
            {
                sacramentoData.HydrographValues[i] = rng.NextDouble();
            }

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = retrievedProject.RootFolder.DataItems.First().Value as RainfallRunoffModel;
                var retrievedSacramentoData = (SacramentoData)retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());

                ReflectionTestHelper.AssertPublicPropertiesAreEqual(sacramentoData, retrievedSacramentoData);
                for (int i = 0; i < 36; i++)
                {
                    Assert.AreEqual(sacramentoData.HydrographValues[i], retrievedSacramentoData.HydrographValues[i]);
                }
            }
        }

        [Test]
        public void SaveAndLoadRunoffBoundaryData()
        {
            var path = "bd.dsproj";
            using (projectRepository = factory.CreateNew())
            {
                var project = new Project();
                var model = new RainfallRunoffModel { Name = "rr" };
                project.RootFolder.Add(model);

                var boundary = new RunoffBoundary { Name = "Boundary1" };
                model.Basin.Boundaries.Add(boundary);

                var boundaryData = model.BoundaryData.First(bd => bd.Boundary == boundary);
                boundaryData.Series.Data[new DateTime(2005, 1, 1)] = 15.0;
                boundaryData.Series.Value = 11.0;

                projectRepository.SaveAs(project, path);
                projectRepository.Close();
            }

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel)retrievedProject.RootFolder.Models.First();
                var retrievedBoundary = retrievedModel.Basin.Boundaries.First();
                var retrievedBoundaryData = retrievedModel.BoundaryData.First();
                Assert.AreSame(retrievedBoundary, retrievedBoundaryData.Boundary);
                Assert.AreEqual(15.0, retrievedBoundaryData.Series.Data.Components[0].Values[0]);
                Assert.AreEqual(11.0, retrievedBoundaryData.Series.Value);
            }
        }


        [Test]
        public void SaveAndLoadPavedData()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            var model = GetRRModel(out project, out hybridProjectRepository);

            var name = "MyFirstCatchment";
            var firstCatchment = new Catchment { Name = name, CatchmentType = CatchmentType.Paved };

            model.Basin.Catchments.Add(firstCatchment);

            var pavedData = (PavedData)model.GetCatchmentModelData(firstCatchment);

            var expected = 555.0;

            pavedData.DryWeatherFlowOptions = PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesVariableDWF;
            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            pavedData.SewerPumpCapacityUnit = PavedEnums.SewerPumpCapacityUnit.mm_hr;
            pavedData.SewerType = PavedEnums.SewerType.ImprovedSeparateSystem;
            pavedData.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;
            pavedData.StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
            pavedData.WaterUseUnit = PavedEnums.WaterUseUnit.l_day;

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(pavedData, new[]
                    {
                        nameof(pavedData.DryWeatherFlowOptions),
                        nameof(pavedData.DryWeatherFlowSewerPumpDischarge),
                        nameof(pavedData.MixedAndOrRainfallSewerPumpDischarge),
                        nameof(pavedData.SewerPumpCapacityUnit),
                        nameof(pavedData.SewerType),
                        nameof(pavedData.SpillingDefinition),
                        nameof(pavedData.StorageUnit),
                        nameof(pavedData.WaterUseUnit)
                    });

            var sewerPumpVariableCapacitySeries = new TimeSeries();
            sewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value1"));
            sewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value2"));
            sewerPumpVariableCapacitySeries[new DateTime((int)expected)] = new[] { expected, expected * 2 };
            sewerPumpVariableCapacitySeries[new DateTime((int)expected * 2)] = new[] { expected * 3, expected * 4 };
            pavedData.MixedSewerPumpVariableCapacitySeries = sewerPumpVariableCapacitySeries;

            var mixedSewerPumpVariableCapacitySeries = new TimeSeries();
            mixedSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value3"));
            mixedSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>("Value4"));
            mixedSewerPumpVariableCapacitySeries[new DateTime((int)expected)] = new[] { expected, expected * 2 };
            mixedSewerPumpVariableCapacitySeries[new DateTime((int)expected * 2)] = new[] { expected * 3, expected * 4 };
            pavedData.DwfSewerPumpVariableCapacitySeries = mixedSewerPumpVariableCapacitySeries;

            var variableWaterUseFunction = new Function();
            variableWaterUseFunction.Arguments.Add(new Variable<int>("Argument"));
            variableWaterUseFunction.Components.Add(new Variable<double>("Component"));
            variableWaterUseFunction[0] = 5.6;
            variableWaterUseFunction[1] = 11.1;
            pavedData.VariableWaterUseFunction = variableWaterUseFunction;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            projectRepository = factory.CreateNew();
            projectRepository.Open(path);
            var retrievedProject = projectRepository.GetProject();
            var retrievedModel = (RainfallRunoffModel)retrievedProject.RootFolder.DataItems.First().Value;
            var retrievedPaved = (PavedData)retrievedModel.GetCatchmentModelData(retrievedModel.Basin.Catchments.First());
            var retrievedMixedSeries = retrievedPaved.MixedSewerPumpVariableCapacitySeries;
            var retrievedDwfSeries = retrievedPaved.DwfSewerPumpVariableCapacitySeries;
            var retrievedFunction = retrievedPaved.VariableWaterUseFunction;

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(pavedData, retrievedPaved);
            Assert.AreEqual(pavedData.DryWeatherFlowOptions, retrievedPaved.DryWeatherFlowOptions);
            Assert.AreEqual(pavedData.DryWeatherFlowSewerPumpDischarge, retrievedPaved.DryWeatherFlowSewerPumpDischarge);
            Assert.AreEqual(pavedData.MixedAndOrRainfallSewerPumpDischarge, retrievedPaved.MixedAndOrRainfallSewerPumpDischarge);
            Assert.AreEqual(pavedData.SewerPumpCapacityUnit, retrievedPaved.SewerPumpCapacityUnit);
            Assert.AreEqual(pavedData.SewerType, retrievedPaved.SewerType);
            Assert.AreEqual(pavedData.SpillingDefinition, retrievedPaved.SpillingDefinition);
            Assert.AreEqual(pavedData.StorageUnit, retrievedPaved.StorageUnit);
            Assert.AreEqual(pavedData.WaterUseUnit, retrievedPaved.WaterUseUnit);
            Assert.AreEqual(mixedSewerPumpVariableCapacitySeries.Components.First().Values, retrievedMixedSeries.Components.First().Values);
            Assert.AreEqual(mixedSewerPumpVariableCapacitySeries.Components.Last().Values, retrievedMixedSeries.Components.Last().Values);
            Assert.AreEqual(sewerPumpVariableCapacitySeries.Components.First().Values, retrievedDwfSeries.Components.First().Values);
            Assert.AreEqual(sewerPumpVariableCapacitySeries.Components.Last().Values, retrievedDwfSeries.Components.Last().Values);
            Assert.AreEqual(variableWaterUseFunction.Components.First().Values, retrievedFunction.Components.First().Values);
        }
        [Test]
        public void SaveLoadModel()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);
            model.AreaUnit = RainfallRunoffEnums.AreaUnit.ha;
            model.OutputTimeStep = new TimeSpan(0, 0, 1, 0);
            model.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).AggregationOptions
                = AggregationOptions.Current;
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 3, 1);
            model.CapSim = true;
            model.CapSimInitOption = RainfallRunoffEnums.CapsimInitOptions.AtMoistureContentpF2;
            model.CapSimCropAreaOption = RainfallRunoffEnums.CapsimCropAreaOptions.AveragedDataPerUnpavedArea;

            model.UseSaveStateTimeRange = true;
            model.SaveStateStartTime = model.StartTime;
            model.SaveStateTimeStep = model.TimeStep;
            model.SaveStateStopTime = model.StopTime;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel)retrievedProject.RootFolder.DataItems.First().Value;

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
                Assert.AreEqual(AggregationOptions.Current,
                                retrievedModel.OutputSettings.GetEngineParameter(QuantityType.Rainfall,
                                                                                 ElementSet.UnpavedElmSet).
                                               AggregationOptions);

                Assert.AreEqual(model.UseSaveStateTimeRange, retrievedModel.UseSaveStateTimeRange);
                Assert.AreEqual(model.SaveStateStartTime, retrievedModel.SaveStateStartTime);
                Assert.AreEqual(model.SaveStateTimeStep, retrievedModel.SaveStateTimeStep);
                Assert.AreEqual(model.SaveStateStopTime, retrievedModel.SaveStateStopTime);
            }
        }
        */
        private RainfallRunoffModel GetRRModel(out Project project, out IApplication application)
        {
            var model = new RainfallRunoffModel { Name = "rr" };
            //model.Evaporation[new DateTime(2011, 1, 1)] = 99.0;
            //model.Precipitation[new DateTime(2011, 3, 3)] = 22.0;
            model.StartTime = new DateTime(2011, 2, 2);
            model.Basin = new DrainageBasin { Name = "" };

            application = CreateRunningDeltaShellApplication();
            project = application.Project;

            

            project.RootFolder.Items.Add(model);
            return model;
        }
        private static DeltaShellApplication CreateRunningDeltaShellApplication()
        {
            var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true };
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());
            //app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            //app.Plugins.Add(new ToolboxApplicationPlugin());

            app.Run();

            return app;
        }


    }

}