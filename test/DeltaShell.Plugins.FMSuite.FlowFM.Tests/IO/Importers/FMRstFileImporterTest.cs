using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    [Category(TestCategory.Slow)]
    public class FMRstFileImporterTest
    {
        [TestFixtureSetUp]
        public void SetMapCoordinateSystemFactory()
        {
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        //test if restart file is copied
        [Test]
        public void FMRstFileImporterCopiedRestartFile()
        {
            using (var gui = new DeltaShellGui())
            {
                gui.Plugins.Add(new FlowFMGuiPlugin());
                
                var app = gui.Application;
                app.Plugins.Add(new FlowFMApplicationPlugin());
                
                gui.Run();
            
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                
                var waterFlowFmModel = new WaterFlowFMModel(TestHelper.CreateLocalCopy(mduPath));
                app.Project.RootFolder.Add(waterFlowFmModel);
                
                var importHandler = new GuiImportHandler(gui);
                
                var restartImportHandlersRst = importHandler.GetImporters(waterFlowFmModel.RestartInput).OfType<FMRstFileImporter>().FirstOrDefault();
                Assert.IsNotNull(restartImportHandlersRst);
                const string harRstNc = "har_20080119_120000_rst.nc";
                const string harlingenDfmOutputHarHarRstNc = @"harlingen/DFM_OUTPUT_har/" +harRstNc;
                var directoryName = Path.GetDirectoryName(waterFlowFmModel.MduFilePath);
                Assert.IsNotNullOrEmpty(directoryName);
                
                restartImportHandlersRst.ImportItem(TestHelper.GetTestFilePath(harlingenDfmOutputHarHarRstNc), waterFlowFmModel.RestartInput);
                //Assert.IsTrue(File.Exists(Path.Combine(directoryName, @"../state_*.zip")));
                Assert.IsTrue(Directory.EnumerateFiles(Path.Combine(directoryName, @"../"), "state_*.zip").Any());
                string[] files = Directory.GetFiles(Path.Combine(directoryName, @"../"), "state_*.zip", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    //file exist
                    foreach (var zipfile in files)
                    {
                        ZipFileUtils.Extract(zipfile, Path.Combine(directoryName, @"../"));
                    }
                }
                Assert.IsTrue(Directory.EnumerateFiles(Path.Combine(directoryName, @"../"), harRstNc).Any());
            }
        }
    }
}