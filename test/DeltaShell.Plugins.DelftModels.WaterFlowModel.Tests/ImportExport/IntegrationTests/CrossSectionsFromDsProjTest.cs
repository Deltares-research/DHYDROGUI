using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    public class CrossSectionsFromDsProjTest
    {
        private DeltaShellApplication app;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);

            app = GetRunningDSApplication();
        }

        [TearDown]
        public void TearDown()
        {
            app.Dispose();
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void VariousCrossSectionsFromDsProj()
        {
            const string testDataDirName = "VariousCrossSects";
            var sourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), @"FileWriters\IntegrationTests", testDataDirName));
            FileUtils.CopyDirectory(sourcePath, testDataDirName, ".svn");

            var dsProjPath = Path.Combine(testDataDirName, "VariousCrossSects.dsproj");
            app.OpenProject(dsProjPath);

            var waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().ToList()[0];
            waterFlowModel1D.Initialize();
            waterFlowModel1D.Finish();
            waterFlowModel1D.Cleanup();

        }
        internal static DeltaShellApplication GetRunningDSApplication()
        {
            // make sure log4net is initialized
            var app = new DeltaShellApplication
            {
                IsProjectCreatedInTemporaryDirectory = false,
                IsDataAccessSynchronizationDisabled = true,
                ScriptRunner = { SkipDefaultLibraries = true }
            };

            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());

            app.Run();
            //app.ProjectRepositoryFactory.SpeedUpConfigurationCreationUsingCaching = true;
            //app.ProjectRepositoryFactory.ConfigurationCacheDirectory = app.GetUserSettingsDirectoryPath();

            return app;
        }

    }
}