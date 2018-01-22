using System;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    public class RoughnessesFromDsProjTest
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
        public void VariousSpatialRoughnessesFromDsProj()
        {
            const string testDataDirName = "VariousRoughnesses";

            var sourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), @"FileWriters\IntegrationTests", testDataDirName));
            FileUtils.CopyDirectory(sourcePath, testDataDirName, ".svn");
            var relativePathSpatialRoughnessesExpectedFile =
                    TestHelper.GetTestFilePath(
                        @"FileWriters/IntegrationTests/VariousRoughnesses/SpatialRoughnesses_expected.txt");
                
            var dsProjPath = Path.Combine(testDataDirName, "VariousSpatialRoughnesses.dsproj");
            app.OpenProject(dsProjPath);

            var waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().ToList()[0];
            waterFlowModel1D.Initialize();
            string errorMessage;
            var actualDirectory = Path.Combine(waterFlowModel1D.WorkingDirectory, waterFlowModel1D.DirectoryName);
            var relativePathActualSpatialRoughnessesFile = Path.Combine(actualDirectory, "roughness-Main.ini");
            Assert.IsTrue(
                FileComparer.Compare(relativePathSpatialRoughnessesExpectedFile, relativePathActualSpatialRoughnessesFile,
                    out errorMessage, true),
                string.Format("Generated roughness file does not match template!{0}{1}",
                    Environment.NewLine, errorMessage));

            waterFlowModel1D.Finish();

            waterFlowModel1D.Cleanup();

        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void VariousSpatialRoughnessesFromDsProjWriteReadWrite()
        {
            string testDataDirName = "VariousRoughnesses";

            string sourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), @"FileWriters\IntegrationTests", testDataDirName));
            FileUtils.CopyDirectory(sourcePath, testDataDirName, ".svn");
                
            string dsProjPath = Path.Combine(testDataDirName, "VariousSpatialRoughnesses.dsproj");
            app.OpenProject(dsProjPath);

            WaterFlowModel1D waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().ToList()[0];
            string targetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(targetPath);
            var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            WaterFlowModel1DFileWriter.Write(modelFilename, waterFlowModel1D);
            
            var readModel = WaterFlowModel1DFileReader.Read(modelFilename);
            Assert.NotNull(readModel);
            Thread.Sleep(2000);
            Directory.CreateDirectory(targetPath + "_SFB");
            var readModelFilename = Path.Combine(targetPath +"_SFB", ModelFileNames.ModelDefinitionFilename);
            
            WaterFlowModel1DFileWriter.Write(readModelFilename, readModel);
            
            string errorMessage;

            var dsprojFile = Path.Combine(targetPath, "roughness-Main.ini");
            var sfbFile = Path.Combine(targetPath + "_SFB", "roughness-Main.ini");
            Assert.IsTrue(
                FileComparer.Compare(dsprojFile, sfbFile, out errorMessage, true), string.Format("Generated roughness file does not match sfb generated!{0}{1}", Environment.NewLine, errorMessage));

            

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
            
            app.Run();
            //app.ProjectRepositoryFactory.SpeedUpConfigurationCreationUsingCaching = true;
            //app.ProjectRepositoryFactory.ConfigurationCacheDirectory = app.GetUserSettingsDirectoryPath();

            return app;
        }

    }
}