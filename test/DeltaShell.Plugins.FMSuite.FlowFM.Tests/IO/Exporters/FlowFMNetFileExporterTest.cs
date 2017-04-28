using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class FlowFMNetFileExporterTest
    {
        [Test]
        public void CanExport()
        {
            var fmModel = new WaterFlowFMModel();

            var exporter = new FlowFMNetFileExporter
                {
                    GetModelForGrid = g => fmModel
                };

            Assert.IsTrue(exporter.CanExportFor(fmModel.Bathymetry));
            Assert.IsFalse(exporter.CanExportFor(fmModel.InitialWaterLevel));
        }

        [TestCase("simplebox_hex7_map.nc", "mesh2d_node_z")] // UGrid
        [TestCase("boundcond_test_map.nc", "NetNode_z")] // Non-UGrid
        public void TestExportNetFileWritesZValues(string netFile, string zValueVariableName)
        {
            const string testDir = "TestExport";
            if (Directory.Exists(testDir)) Directory.Delete(testDir, true);

            // get running DeltaShell application
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                // create FM Model
                var fmModel = new WaterFlowFMModel();
                app.Project.RootFolder.Add(fmModel);

                app.SaveProjectAs(Path.Combine(testDir, "TestExport.dsproj")); // save to initialize file repository..
                fmModel.ExportTo(Path.Combine(testDir, "TestModel.mdu"));

                var importer = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                Assert.IsNotNull(importer);
                var path = TestHelper.GetTestFilePath(Path.Combine("output_mapfiles", netFile));

                // import netfile into Unstructured Grid
                importer.ImportItem(path, fmModel.Grid);
                
                var exporter = new FlowFMNetFileExporter{ GetModelForGrid = g => fmModel };
                var outputFilePath = Path.Combine(testDir, "outputNetFile.nc");

                // exporting UnstructuredGrid should be successful
                Assert.IsTrue(exporter.Export(fmModel.Grid, outputFilePath));

                using (var ncFile = new NetCdfFileWrapper(outputFilePath))
                {
                    // exported grid should contain zValue variable
                    Assert.NotNull(ncFile.GetValues1D<double>(zValueVariableName));
                }
            }

        }

    }
}