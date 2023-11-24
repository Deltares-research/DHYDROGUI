using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class FMPartitionExporterTest
    {
        [Test]
        public void PartitionStandAloneGrid3Domains()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                FlowFMNetFileImporter importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                object importedNetFileDataItem =
                    importer.ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"));
                var importedNetFile = ((IDataItem) importedNetFileDataItem).Value as ImportedFMNetFile;
                FMGridPartitionExporter exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(importedNetFile, Path.Combine(exportDir, "har_net.nc"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002_net.nc")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionStandAloneGrid3DomainsWithPolFile()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                FlowFMNetFileImporter importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                object importedNetFileDataItem =
                    importer.ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"));
                var importedNetFile = ((IDataItem) importedNetFileDataItem).Value as ImportedFMNetFile;
                FMGridPartitionExporter exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                string polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(importedNetFile, Path.Combine(exportDir, "har_net.nc"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002_net.nc")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionHarlingenGrid3Domains()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                FMGridPartitionExporter exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model.Grid, Path.Combine(exportDir, "har_net.nc"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002_net.nc")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionHarlingenGridWithInvalidSolverShouldNotCrash()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");
                app.Project.RootFolder.Add(model);

                FMGridPartitionExporter exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model.Grid, Path.Combine(exportDir, "har_net.nc"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002_net.nc")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionHarlingenGrid3DomainsWithPolFile()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                FMGridPartitionExporter exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                string polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model.Grid, Path.Combine(exportDir, "har_net.nc"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002_net.nc")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        [Category(TestCategory.Integration)]
        public void PartitionHarlingen3Domains()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                FMModelPartitionExporter exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model, Path.Combine(exportDir, "har.mdu"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0002_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002.mdu")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionHarlingenWithIncorrectSolverShouldNotCrash()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");
                app.Project.RootFolder.Add(model);

                FMModelPartitionExporter exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model, Path.Combine(exportDir, "har.mdu"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0002_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002.mdu")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        public void PartitionHarlingen3DomainsWithPolFile()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                FMModelPartitionExporter exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                string polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model, Path.Combine(exportDir, "har.mdu"));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0000_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0001_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "fm_003_0002_net.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0000.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0001.mdu")));
                Assert.IsTrue(File.Exists(Path.Combine(exportDir, "har_0002.mdu")));
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void PartitionExporterShouldNotLoseValues()
        {
            const string relativePath = "partition";
            
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"partitionexporter\SongHau.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                FMModelPartitionExporter exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                
                exporter.NumDomains = 4;
                exporter.IsContiguous = true;
                
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                string exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model, Path.Combine(exportDir, "SongHau.mdu"));
                
                var outputFiles = new[]
                {
                    "SongHau_0000.mdu",
                    "SongHau_0001.mdu",
                    "SongHau_0002.mdu",
                    "SongHau_0003.mdu"
                };
                
                foreach (string file in outputFiles)
                {
                    string path = Path.Combine(exportDir, file);
                    Assert.IsTrue(File.Exists(path));

                    var parser = new IniParser();
                    
                    string ini = File.ReadAllText(path);
                    IniData iniData = parser.Parse(ini);
                    
                    IniSection externalForcing = iniData.GetSection("external forcing");
                    IniSection geometry = iniData.GetSection("geometry");
                    IniSection output = iniData.GetSection("output");

                    string strExtForceFile = externalForcing.GetPropertyValueOrDefault("ExtForceFile");
                    string strExtForceFileNew = externalForcing.GetPropertyValueOrDefault("ExtForceFileNew");
                    string strLandBoundaryFile = geometry.GetPropertyValueOrDefault("LandBoundaryFile");
                    string strObsFile = output.GetPropertyValueOrDefault("ObsFile");

                    Assert.IsNotEmpty(strExtForceFile, $"ExtForceFile not set in {file}");
                    Assert.IsNotEmpty(strExtForceFileNew, $"ExtForceFileNew not set in {file}");
                    Assert.IsNotEmpty(strLandBoundaryFile, $"LandBoundaryFile not set in {file}");
                    Assert.IsNotEmpty(strObsFile, $"ObsFile not set in {file}");

                    string partitionFile = geometry.GetPropertyValueOrDefault("PartitionFile");
                    Assert.Null(partitionFile, $"PartitionFile present in {file} - this is not valid.");
                }
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [TestCase(false)]
        [TestCase(true)]
        public void FMPartitionExporter_ShouldNotChangeModelOutputOutOfSyncStatus(bool outputOutOfSync)
        {
            // Arrange
            using (var tempDirectory = new TemporaryDirectory())
            {
                string exportPartitionDirectory = Path.Combine(tempDirectory.Path, "Partition");
                Directory.CreateDirectory(exportPartitionDirectory);

                string mduDirectoryPathInTemp = tempDirectory.CopyDirectoryToTempDirectory(Path.Combine(TestHelper.GetTestDataDirectory(), "harlingen"));
                string mduFilePathInTemp = Path.Combine(mduDirectoryPathInTemp, "har.mdu");

                string polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                string polygonFileInTemp = tempDirectory.CopyTestDataFileToTempDirectory(polygonFile);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePathInTemp);
                
                CreateRestartOutputFile(tempDirectory.Path);
                model.ConnectOutput(tempDirectory.Path);

                model.OutputOutOfSync = outputOutOfSync;

                var exporter = new FMModelPartitionExporter
                {
                    NumDomains = 4,
                    IsContiguous = true,
                    PolygonFile = polygonFileInTemp
                };

                // Act
                exporter.Export(model, Path.Combine(exportPartitionDirectory, "test.mdu"));

                // Assert
                Assert.AreEqual(outputOutOfSync, model.OutputOutOfSync);
            }
        }

        private static void CreateRestartOutputFile(string tempDirectoryPath)
        {
            string restartFilePath = Path.Combine(tempDirectoryPath, "test_rst.nc");
            const string text = "This is some text in the file.";

            using (FileStream fs = File.Create(restartFilePath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(text);
                fs.Write(info, 0, info.Length);
            }
        }
    }
}