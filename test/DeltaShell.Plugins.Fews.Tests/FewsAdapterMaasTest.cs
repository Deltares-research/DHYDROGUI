using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    class FewsAdapterMaasTest : FewsAdapterTestBase
    {
        [SetUp]
        public void SetUpFixture()
        {
            XmlConfigurator.Configure();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        [Category(TestCategory.BackwardCompatibility)]
        public void LoadMaasProjectShouldNotCrash()
        {
            string testRunDir = "LoadMaas";
            FileUtils.DeleteIfExists(testRunDir);
            FileUtils.CreateDirectoryIfNotExists(testRunDir);
            var path = TestHelper.GetTestDataPath(typeof(FewsAdapterTest).Assembly, @"Maas\DSModel\j03mid_19728_FEWS_v007.dsproj");
            var pathNew = testRunDir + "/j03mid_19728_FEWS_v007.dsproj";
            FileUtils.CopyFile(path, pathNew);
            FileUtils.CopyDirectory(path + "_data", pathNew + "_data");

            using (var gui = GetRunningGui())
            {
                var app = gui.Application;
                app.OpenProject(pathNew);
            }
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.VerySlow)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.Integration)]
        public void ImportMaasProjectShouldNotCrash()
        {
            string testRunDir = "ImportMaas";
            FileUtils.DeleteIfExists(testRunDir);
            FileUtils.CreateDirectoryIfNotExists(testRunDir);
            var path = TestHelper.GetTestDataPath(typeof(FewsAdapterTest).Assembly, @"Maas\DSModel\j03mid_19728_FEWS_v007.dsproj");
            var pathNew = testRunDir + "/j03mid_19728_FEWS_v007.dsproj";
            FileUtils.CopyFile(path, pathNew);
            FileUtils.CopyDirectory(path + "_data", pathNew + "_data", ".svn");

            using (var app = GetRunningDSApplication())
            {
                app.CreateNewProject();
                app.SaveProjectAs(pathNew);

                var importer = new ProjectImporter { HybridProjectRepository = app.HybridProjectRepository, TargetDataDirectory = "." };
                var items = (IList<IProjectItem>)importer.ImportItem(path);

                Assert.AreEqual(1, items.Count);
            }
        }

        private static DeltaShellGui GetRunningGui()
        {
            var gui = new DeltaShellGui();
            var app = gui.Application;
            app.UserSettings["autosaveWindowLayout"] = false;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());

            gui.Run();

            return gui;
        }

    }
}
