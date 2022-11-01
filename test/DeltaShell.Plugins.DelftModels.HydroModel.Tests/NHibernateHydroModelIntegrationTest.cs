using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Core;
using DeltaShell.Dimr.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.ImportExport.GWSW;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DHYDRO.Common.IO.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateHydroModelIntegrationTest : NHibernateIntegrationTestBase
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new FlowFMApplicationPlugin());
        }

        [Test]
        public void SaveLoadHydroModelWithSeveralSubActivities()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            int expectedActivitiesCount = hydroModel.Activities.Count;
            int expectedRegionsCount = hydroModel.Region.AllRegions.Count();

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);

            Assert.AreEqual(expectedActivitiesCount, retrievedHydroModel.Activities.Count);
            Assert.AreEqual(expectedRegionsCount, retrievedHydroModel.Region.AllRegions.Count());
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflow()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);
            Assert.AreEqual(2, retrievedHydroModel.Workflows.IndexOf(retrievedHydroModel.CurrentWorkflow));
        }

        [Test]
        public void SaveHydroModelDoesNotCausePropertyChangeOnWorkflowTools9667()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(hydroModel);

            ProjectRepository.Create(path);

            try
            {
                var workFlowBefore = hydroModel.CurrentWorkflow;
                ProjectRepository.SaveOrUpdate(project);
                var workFlowAfter = hydroModel.CurrentWorkflow;

                Assert.AreSame(workFlowBefore, workFlowAfter);
            }
            finally
            {
                ProjectRepository.Close();
            }
        }

        [Test]
        public void GivenAProjectWithAnIntegratedModelWithHydroLinks_FromVersion202204_MigratesAndLoadsCorrectly()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (DeltaShellApplication app = GetConfiguredApplication())
            {
                string file = temp.CopyTestDataFileAndDirectoryToTempDirectory(Path.Combine("BackwardCompatibility", "ProjectWithHydrolinksCreatedWith2022.04.dsproj"));

                // Call
                app.OpenProject(file);
                
                // Assert
                HydroModel integratedModel = app.Project.GetAllItemsRecursive().OfType<HydroModel>().FirstOrDefault();
                Assert.That(integratedModel, Is.Not.Null);
                
                IEventedList<HydroLink> links = integratedModel.Region.Links;
                Assert.That(links.Count, Is.EqualTo(2));
                Assert.That(links[0].Source, Is.TypeOf<Catchment>());
                Assert.That(links[0].Target, Is.TypeOf<LateralSource>());
                Assert.That(links[1].Source, Is.TypeOf<Catchment>());
                Assert.That(links[1].Target, Is.TypeOf<LateralSource>());
            }
        }

        private DeltaShellApplication GetConfiguredApplication()
        {
            var app = new DeltaShellApplication();
            AddPluginsToApplication(app);
            return app;
        }

        private static void AddPluginsToApplication(DeltaShellApplication app)
        {
            // DeltaShell plugins
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());

            // D-HYDRO plugins
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());

            app.Run();
        }
    }
}