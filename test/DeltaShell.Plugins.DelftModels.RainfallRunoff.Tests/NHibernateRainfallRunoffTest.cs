using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateRainfallRunoffTest
    {
        #region SetUp

        private NHibernateProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.SetLoggingLevel(Level.Off);

            factory = new NHibernateProjectRepositoryFactory();
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());
            }

        [SetUp]
        public void SetUp()
        {
            projectRepository = factory.CreateNew();
        }

        [TearDown]
        public void TearDown()
        {
            projectRepository.Dispose();
            //aggresive collect..needed because GC is so lazy it goes out of mem
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion
        
        [Test(Description = "See RainfallRunoffModelDataSaveLoadTest.SaveAndLoadMeteorogicalDataGlobal")]
        [Category("Quarantine")]
        public void SaveAndLoadGlobalMeteorogicalData()
        {
            var now = new DateTime(2000, 1, 2);
            var value = 123.45;

            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);

            Assert.AreEqual(MeteoDataDistributionType.Global, model.Precipitation.DataDistributionType);
            model.Precipitation.Data[now] = value;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel) retrievedProject.RootFolder.DataItems.First().Value;

                Assert.AreEqual(MeteoDataDistributionType.Global, retrievedModel.Precipitation.DataDistributionType);

                //Assert.IsTrue(retrievedModel.Precipitation.Data is ITimeSeries);

                Assert.IsTrue(((TimeSeries) retrievedModel.Precipitation.Data).Time.Values.Contains(now),
                              "Timestep of precipitation has not been saved/load.");

                Assert.AreEqual(value, retrievedModel.Precipitation.Data[now],
                                "Value of precipitation has not been saved/load.");
            }
        }

        [Test(Description = "See RainfallRunoffModelDataSaveLoadTest.SaveAndLoadMeteorogicalDataPerFeature")]
        [Category("Quarantine")]
        public void SaveAndLoadMeteorogicalDataPerFeature()
        {
            var now = new DateTime(2000, 1, 2);
            var value = 123.45;

            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);

            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

            var catchment = Catchment.CreateDefault();
            model.Basin.Catchments.Add(catchment);

            model.Precipitation.Data[now, catchment] = value;


            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel) retrievedProject.RootFolder.DataItems.First().Value;

                Assert.AreEqual(MeteoDataDistributionType.PerFeature,
                                retrievedModel.Precipitation.DataDistributionType);

                Assert.IsTrue(retrievedModel.Precipitation.Data is IFeatureCoverage);

                Assert.IsTrue(((IFeatureCoverage) retrievedModel.Precipitation.Data).Time.Values.Contains(now),
                              "Timestep of precipitation has not been saved/load.");

                Assert.AreEqual(value, retrievedModel.Precipitation.Data[now, catchment],
                                "Value of precipitation has not been saved/load.");
            }
        }
        
        
        [Test]
        public void SaveLoadModelOutputSettingsEvent()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);
            model.AreaUnit = RainfallRunoffEnums.AreaUnit.ha;
            model.OutputTimeStep = new TimeSpan(0, 0, 1, 0);
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 3, 1);

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel) retrievedProject.RootFolder.DataItems.First().Value;

                var nOutputCoverage = retrievedModel.OutputCoverages.Count();

                Assert.AreEqual(false,
                                retrievedModel.OutputSettings.GetEngineParameter(QuantityType.Rainfall,
                                                                                 ElementSet.UnpavedElmSet)
                                              .IsEnabled);
                Assert.AreEqual(AggregationOptions.Current, retrievedModel.OutputSettings.AggregationOption);

                retrievedModel.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).
                               IsEnabled = true;

                Assert.AreEqual(nOutputCoverage + 1, retrievedModel.OutputCoverages.Count());
            }
        }

        private RainfallRunoffModel GetRRModel(out Project project, out HybridProjectRepository _hybridProjectRepository)
        {
            var model = new RainfallRunoffModel { Name = "rr" };
            //model.Evaporation[new DateTime(2011, 1, 1)] = 99.0;
            //model.Precipitation[new DateTime(2011, 3, 3)] = 22.0;
            model.StartTime = new DateTime(2011, 2, 2);
            model.Basin = new DrainageBasin {Name = ""};

            _hybridProjectRepository = new HybridProjectRepository(factory);
            project = new Project();

            var dataItem = new DataItem(model);

            project.RootFolder.Items.Add(dataItem);
            return model;
        }
        
        [Test]
        public void SaveLoadUnpavedDataExtended()
        {
            Project project;
            HybridProjectRepository hybridProjectRepository;
            RainfallRunoffModel model = GetRRModel(out project, out hybridProjectRepository);

            var catchmentName = "hahahaha";
            var useLocalBoundaryData = true;
            
            model.SaveUnpavedDataExtended.Add(new UnpavedDataExtended(catchmentName,useLocalBoundaryData));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Close(project);
            hybridProjectRepository.Dispose();

            using (projectRepository = factory.CreateNew())
            {
                projectRepository.Open(path);
                var retrievedProject = projectRepository.GetProject();
                var retrievedModel = (RainfallRunoffModel) retrievedProject.RootFolder.DataItems.First().Value;

                Assert.AreEqual(1,retrievedModel.SaveUnpavedDataExtended.Count);
                var unpavedDataExtended = retrievedModel.SaveUnpavedDataExtended.First();
                
                Assert.AreEqual(catchmentName,unpavedDataExtended.CatchmentName);
                Assert.AreEqual(useLocalBoundaryData,unpavedDataExtended.UseLocalBoundaryData);
            }
        }
    }
}
