using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using Nini.Ini;
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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"partitionexporter\SongHau.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.LoadMdu(mduFilePath);

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
                    var reader = new IniReader(path);
                    reader.SetCommentDelimiters(new[]
                    {
                        '#'
                    });
                    var document = new IniDocument(reader);
                    IniSection externalForcing = document.Sections["external forcing"];
                    IniSection geometry = document.Sections["geometry"];
                    IniSection output = document.Sections["output"];

                    string strExtForceFile = externalForcing.GetValue("ExtForceFile");
                    string strExtForceFileNew = externalForcing.GetValue("ExtForceFileNew");
                    string strLandBoundaryFile = geometry.GetValue("LandBoundaryFile");
                    string strObsFile = output.GetValue("ObsFile");

                    Assert.IsNotEmpty(strExtForceFile, string.Format("ExtForceFile not set in {0}", file));
                    Assert.IsNotEmpty(strExtForceFileNew, string.Format("ExtForceFileNew not set in {0}", file));
                    Assert.IsNotEmpty(strLandBoundaryFile, string.Format("LandBoundaryFile not set in {0}", file));
                    Assert.IsNotEmpty(strObsFile, string.Format("ObsFile not set in {0}", file));

                    string partitionFile = geometry.GetValue("PartitionFile");
                    Assert.Null(partitionFile, string.Format("PartitionFile present in {0} - this is not valid.", file));
                }
            }

            if (Directory.Exists(relativePath))
            {
                Directory.Delete(relativePath, true);
            }
        }
    }
}