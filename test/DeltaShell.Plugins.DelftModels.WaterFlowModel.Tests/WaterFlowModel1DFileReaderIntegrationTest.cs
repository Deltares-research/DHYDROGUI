using System.IO;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DFileReaderIntegrationTest
    {
        private string tempFolderPath;

        [SetUp]
        public void Setup()
        {
            var testFolder = TestHelper.GetTestDataPath(Assembly.GetExecutingAssembly(), @"Md1dReading");
            tempFolderPath = TestHelper.CreateLocalCopy(testFolder);

        }

        [Test]
        public void TestName()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExport.md1d");

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();

                var importer = new WaterFlowModel1DFileImporter();
                var importActivity = new FileImportActivity(importer, app.Project.RootFolder.Models)
                {
                    ImportedItemOwner = app.Project.RootFolder,
                    Files = new[] { md1dFilePath }
                };

                app.RunActivity(importActivity);

                var waterFlowModel1D = importer.ImportItem(md1dFilePath);
                Assert.IsNotNull(waterFlowModel1D);

            }
        }
    }
}