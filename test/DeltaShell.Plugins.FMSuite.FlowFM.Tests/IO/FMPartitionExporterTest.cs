using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                var importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                var importedNetFileDataItem =
                    importer.ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"));
                var importedNetFile = ((IDataItem) importedNetFileDataItem).Value as ImportedFMNetFile;
                var exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                var importedNetFileDataItem =
                    importer.ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"));
                var importedNetFile = ((IDataItem)importedNetFileDataItem).Value as ImportedFMNetFile;
                var exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                var polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMGridPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                var polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 3;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                var polygonFile = TestHelper.GetTestFilePath(@"har_part\ThreeDomains.pol");
                exporter.PolygonFile = polygonFile;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
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

        [Test, Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        public void PartitionExporterShouldNotLoseValues()
        {
            const string relativePath = "partition";
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("partition.dsproj"); // save to initialize file repository..

                var mduPath = TestHelper.GetTestFilePath(@"partitionexporter\SongHau.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                var exporter = app.FileExporters.OfType<FMModelPartitionExporter>().FirstOrDefault();
                Assert.IsNotNull(exporter);
                exporter.NumDomains = 4;
                exporter.IsContiguous = true;
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }
                var exportDir = Path.GetFullPath(relativePath);
                exporter.Export(model, Path.Combine(exportDir, "SongHau.mdu"));
                var outputFiles = new[] {"SongHau_0000.mdu", "SongHau_0001.mdu", "SongHau_0002.mdu", "SongHau_0003.mdu"};
                foreach (var file in outputFiles)
                {
                    var path = Path.Combine(exportDir, file);
                    Assert.IsTrue(File.Exists(path));
                    var reader = new IniReader(path);
                    reader.SetCommentDelimiters(new[] { '#' });
                    var document = new IniDocument(reader);
                    var externalForcing = document.Sections["external forcing"];
                    var geometry = document.Sections["geometry"];
                    var output = document.Sections["output"];

                    var strExtForceFile = externalForcing.GetValue("ExtForceFile");
                    var strExtForceFileNew = externalForcing.GetValue("ExtForceFileNew");
                    var strLandBoundaryFile = geometry.GetValue("LandBoundaryFile");
                    var strObsFile = output.GetValue("ObsFile");

                    Assert.IsNotEmpty(strExtForceFile, string.Format("ExtForceFile not set in {0}", file));
                    Assert.IsNotEmpty(strExtForceFileNew, string.Format("ExtForceFileNew not set in {0}", file));
                    Assert.IsNotEmpty(strLandBoundaryFile, string.Format("LandBoundaryFile not set in {0}", file));
                    Assert.IsNotEmpty(strObsFile, string.Format("ObsFile not set in {0}", file));

                    var partitionFile = geometry.GetValue("PartitionFile");
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
