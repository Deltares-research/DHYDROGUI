using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class FlowFMNetFileExporterTest
    {
        private FlowFMNetFileExporter exporter;

        [SetUp]
        public void SetUp()
        {
            exporter = new FlowFMNetFileExporter();
        }

        [Test]
        public void CanExport()
        {
            var fmModel = new WaterFlowFMModel();

            exporter.GetModelForGrid = g => fmModel;
            
            Assert.IsTrue(exporter.CanExportFor(fmModel.Bathymetry));
            Assert.IsFalse(exporter.CanExportFor(fmModel.InitialWaterLevel));
        }

        [TestCase("simplebox_hex7_map.nc", "mesh2d_node_z")] // UGrid
        [TestCase("boundcond_test_map.nc", "NetNode_z")] // Non-UGrid
        public void TestExportNetFileWritesZValuesAtNodes(string netFile, string zValueVariableName)
        {
            const string testDir = "TestExport";
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }

            // get running DeltaShell application
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();
                app.CreateNewProject();

                // create FM Model
                var fmModel = new WaterFlowFMModel();
                app.Project.RootFolder.Add(fmModel);

                app.SaveProjectAs(Path.Combine(testDir, "TestExport.dsproj")); // save to initialize file repository..
                fmModel.ExportTo(Path.Combine(testDir, "TestModel.mdu"));

                FlowFMNetFileImporter importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);

                string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
                var zmDfmZipFileName = "zm_dfm_map.zip";
                string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

                TestHelper.PerformActionInTemporaryDirectory(tempDir =>
                {
                    FileUtils.CopyDirectory(testDataFilePath, tempDir);
                    ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                    string mapFilePath = Path.Combine(tempDir, netFile);

                    // import netfile into Unstructured Grid
                    importer.ImportItem(mapFilePath, fmModel.Grid);

                    exporter.GetModelForGrid = g => fmModel;
                    string outputFilePath = Path.Combine(testDir, "outputNetFile.nc");

                    // exporting UnstructuredGrid should be successful
                    Assert.IsTrue(exporter.Export(fmModel.Grid, outputFilePath));

                    using (var ncFile = new NetCdfFileWrapper(outputFilePath))
                    {
                        // exported grid should contain zValue variable
                        Assert.NotNull(ncFile.GetValues1D<double>(zValueVariableName));
                    }
                });
            }
        }

        [Test]
        public void TestExportNetFileWriteZValuesAtCellCenters()
        {
            const string testDir = "TestExport";
            if (Directory.Exists(testDir)) Directory.Delete(testDir, true);

            // get running DeltaShell application
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();
                app.CreateNewProject();

                // create FM Model
                var fmModel = new WaterFlowFMModel();

                // set bed level location to faces
                var cellsValue = ((int)UGridFileHelper.BedLevelLocation.Faces).ToString();
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(cellsValue);
                
                app.Project.RootFolder.Add(fmModel);

                app.SaveProjectAs(Path.Combine(testDir, "TestExport.dsproj")); // save to initialize file repository..
                fmModel.ExportTo(Path.Combine(testDir, "TestModel.mdu"));

                var importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                var path = TestHelper.GetTestFilePath(Path.Combine("ugrid", "Custom_Ugrid.nc"));

                // import netfile into Unstructured Grid
                importer.ImportItem(path, fmModel.Grid);

                exporter.GetModelForGrid = g => fmModel;
                var outputFilePath = Path.Combine(testDir, "outputNetFile.nc");

                // exporting UnstructuredGrid should be successful
                Assert.IsTrue(exporter.Export(fmModel.Grid, outputFilePath));

                using (var ncFile = new NetCdfFileWrapper(outputFilePath))
                {
                    // exported grid should contain zValue variable
                    Assert.NotNull(ncFile.GetValues1D<double>("mesh2d_face_z"));
                }
            }
        }

        [Test]
        public void GivenImportedFMNetFileWhenExportingWithSamePathThenReturnTrue()
        {
            var nonExistingFilePath = "NonExistingFile.nc";
            var netFile = new ImportedFMNetFile(nonExistingFilePath);
            var result = exporter.Export(netFile, nonExistingFilePath);

            Assert.IsTrue(result);
        }

        [Test]
        public void GivenImportedFMNetFileWhenExportingWithDifferentPathThenCreateFileCopyAndReturnTrue()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "simplebox_hex7_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);
                string dummyFilePath = Path.Combine(testDataFilePath, "dummy.nc");

                var netFile = new ImportedFMNetFile(mapFilePath);
                bool result = exporter.Export(netFile, dummyFilePath);
                Assert.IsTrue(result);

                CheckIfFileExistsAndDeleteTheFile(dummyFilePath);
            });
        }

        [Test]
        public void GivenImportedFMNetFileWhenExportingWithUnstructuredGridWithEmptyGridThenReturnFalse()
        {
            var unstructuredGrid = new UnstructuredGrid();
            var result = exporter.Export(unstructuredGrid, "NonExistingFile.nc");

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenWaterFlowFMModelWithNonEmptyGridWhenExportingThenWriteNetFileAndReturnTrue()
        {
            const string testDir = "TestExport";
            if (Directory.Exists(testDir)) Directory.Delete(testDir, true);
            var dummyFilePath = TestHelper.GetTestFilePath(Path.Combine("output_mapfiles", "dummy.nc"));

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();
                app.CreateNewProject();

                // create FM Model
                var fmModel = new WaterFlowFMModel();

                // set bed level location to faces
                var cellsValue = ((int) UGridFileHelper.BedLevelLocation.Faces).ToString();
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(cellsValue);

                app.Project.RootFolder.Add(fmModel);
                
                app.SaveProjectAs(Path.Combine(testDir, "TestExport.dsproj")); // save to initialize file repository..
                fmModel.ExportTo(Path.Combine(testDir, "TestModel.mdu"));

                var importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                var path = TestHelper.GetTestFilePath(Path.Combine("ugrid", "Custom_Ugrid.nc"));

                // import netfile into Unstructured Grid
                importer.ImportItem(path, fmModel.Grid);
                
                var result = exporter.Export(fmModel.Grid, dummyFilePath);
                Assert.IsTrue(result);
            }
            CheckIfFileExistsAndDeleteTheFile(dummyFilePath);
        }

        [Test]
        public void GivenImportedFMNetFileWhenGettingSourceTypesThenReturnDefaultValues()
        {
            var sourceTypes = exporter.SourceTypes().AsList();
            Assert.That(sourceTypes.Count, Is.EqualTo(3));
            Assert.That(sourceTypes.Contains(typeof(UnstructuredGrid)));
            Assert.That(sourceTypes.Contains(typeof(ImportedFMNetFile)));
            Assert.That(sourceTypes.Contains(typeof(UnstructuredGridCoverage)));
        }

        private static void CheckIfFileExistsAndDeleteTheFile(string dummyFilePath)
        {
            // Check if a file copy has been created with the new file path
            Assert.That(File.Exists(dummyFilePath), Is.True);

            // Delete the dummy file again
            File.Delete(dummyFilePath);
            Assert.That(File.Exists(dummyFilePath), Is.False);
        }
    }
}