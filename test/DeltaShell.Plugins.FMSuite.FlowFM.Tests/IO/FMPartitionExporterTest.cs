using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class FMPartitionExporterTest
    {
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new FlowFMApplicationPlugin()
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
        [Test]
        public void PartitionStandAloneGrid3Domains()
        {
            const string relativePath = "partition";
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
            using (var app = CreateApplication())
            {
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
    }
}
