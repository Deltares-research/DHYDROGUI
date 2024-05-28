using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Plugins;
using DelftTools.Shell.Core.Settings;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateRainfallRunoffTest
    {
        #region SetUp

        private IProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;
        private ISettingsManager settingsManager;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            factory = new NHibernateProjectRepositoryFactory();
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());

            settingsManager = Substitute.For<ISettingsManager>();
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

            _hybridProjectRepository = new HybridProjectRepository(factory.CreateNew(), settingsManager, Substitute.For<IProjectFileBasedItemRepository>(), Substitute.For<IPluginsManager>());
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
