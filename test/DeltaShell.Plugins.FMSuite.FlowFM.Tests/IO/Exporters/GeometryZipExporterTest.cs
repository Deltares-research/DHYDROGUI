using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class GeometryZipExporterTest
    {
        private GeometryZipExporter exporter;

        [SetUp]
        public void Setup()
        {
            exporter = new GeometryZipExporter();
        }

        [Test]
        public void TestWriteZValuesToNetFile()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"harlingen\FilesUsingOldFormat\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention(netFilePath, out convention);
                Assert.AreEqual(convention, GridApiDataSet.DataSetConventions.CONV_OTHER);
            }

            UnstructuredGrid grid = NetFileImporter.ImportGrid(netFilePath);

            IEnumerable<double> currentZValues = grid.Vertices.Select(v => v.Z);
            double[] newZValues = currentZValues.Select(z =>
            {
                z = 123.456;
                return z;
            }).ToArray();

            NetFile.WriteZValues(netFilePath, newZValues);

            UnstructuredGrid adjustedGrid = NetFileImporter.ImportGrid(netFilePath);
            IEnumerable<double> zValues = adjustedGrid.Vertices.Select(v => v.Z);
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        public void TestWriteZValuesAtNodesToNetFile_UGrid()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention(netFilePath, out convention);
                Assert.AreEqual(convention, GridApiDataSet.DataSetConventions.CONV_UGRID);
            }

            // get original grid
            UnstructuredGrid grid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(netFilePath))
            {
                grid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }

            Assert.NotNull(grid);

            // generate new z values
            IEnumerable<double> currentZValues = grid.Vertices.Select(v => v.Z);
            double[] newZValues = currentZValues.Select(z =>
            {
                z = 123.456;
                return z;
            }).ToArray();

            // write new coordinates to netfile
            using (var uGrid = new UGrid(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtNodesForMeshId(1, newZValues);
            }

            // read new grid
            UnstructuredGrid adjustedGrid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(netFilePath))
            {
                adjustedGrid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }

            Assert.NotNull(adjustedGrid);

            // compare z values
            IEnumerable<double> zValues = adjustedGrid.Vertices.Select(v => v.Z);
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        public void TestWriteZValuesAtCellCentersToNetFile_UGrid()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention(netFilePath, out convention);
                Assert.AreEqual(convention, GridApiDataSet.DataSetConventions.CONV_UGRID);
            }

            // get original grid
            UnstructuredGrid grid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(netFilePath))
            {
                grid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }

            Assert.NotNull(grid);

            // generate new z values
            double[] newZValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();

            // write new coordinates to netfile
            using (var uGrid = new UGrid(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtFacesForMeshId(1, newZValues);
            }

            using (var ncFile = new NetCdfFileWrapper(netFilePath))
            {
                // exported grid should contain zValue variable
                IList<double> zValues = ncFile.GetValues1D<double>("mesh2d_face_z");
                Assert.NotNull(zValues);
                Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
            }
        }

        [Test]
        public void GivenAGeometryZipExporter_WhenNamePropertyIsCalled_ThenNameIsReturned()
        {
            const string expectedVal = "Net-geometry exporter";
            Assert.That(exporter.Name, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAGeometryZipExporter_WhenCategoryIsCalled_ThenTheNameOfTheCategoryIsReturned()
        {
            const string expectedVal = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAGeometryZipExporter_WhenSourceTypesIsCalled_ThenAnEnumerableContainingTheSourceTypesIsReturned()
        {
            IEnumerable<Type> obtainedVals = exporter.SourceTypes();
            Assert.That(obtainedVals.Count(), Is.EqualTo(2));
            Assert.That(obtainedVals.Contains(typeof(UnstructuredGrid)));
            Assert.That(obtainedVals.Contains(typeof(UnstructuredGridCoverage)));
        }

        [Test]
        public void GivenAGeometryZipExporter_WhenFileFilterPropertyIsCalled_ThenFileFilterIsReturned()
        {
            const string expectedVal = "Zip file|*.zip";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAGeometryZipExporterAndFmModel_WhenCanExportFor_ThenReturnsTrueOnlyForUnstructuredGridCoverageBathymetryOfModel()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            exporter.GetModelForGrid = g => fmModel;

            // When, Then
            Assert.IsTrue(exporter.CanExportFor(fmModel.SpatialData.Bathymetry),
                          "GeometryZipExporter should be able to export the bathymetry (UnstructuredGridCoverage) of the model.");
            Assert.IsFalse(exporter.CanExportFor(fmModel.SpatialData.InitialWaterLevel),
                           "GeometryZipExporter should not be able to export anything other than the bathymetry of the model.");
        }

        [Test]
        public void GivenAGeometryZipExporterAndFmModel_WhenCanExportFor_ThenReturnsTrueForAnUnstructuredGrid()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            exporter.GetModelForGrid = g => fmModel;

            // When, Then
            Assert.IsTrue(exporter.CanExportFor(new UnstructuredGrid()),
                          "GeometryZipExporter should be able to export an UnstructuredGrid.");
        }

        [Test]
        public void GivenAGeometryZipExporter_WhenExportIsCalledWithNull_ThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, Arg<string>.Is.Anything), Is.False);
        }

        [Test]
        public void GivenAGeometryZipExporterAndAnUnstructuredEmptyGrid_WhenExportIsCalled_ThenFalseIsReturned()
        {
            var mocks = new MockRepository();
            var unstructuredGridMock = mocks.DynamicMock<UnstructuredGrid>();
            unstructuredGridMock.Expect(n => n.IsEmpty).Return(true).Repeat.Any();

            mocks.ReplayAll();
            Assert.That(exporter.Export(unstructuredGridMock, Arg<string>.Is.Anything), Is.False);

            string expectedLogMessage = Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(unstructuredGridMock, Arg<string>.Is.Anything), expectedLogMessage);

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAGeometryZipExporterAndAValidEmptyModelWithAnEmptyGrid_WhenExportIsCalled_ThenFalseIsReturnedAndAWarningIsLogged()
        {
            var mocks = new MockRepository();
            var gridMock = mocks.DynamicMock<UnstructuredGrid>();
            gridMock.Expect(n => n.IsEmpty).Return(true).Repeat.Any();

            mocks.ReplayAll();

            Assert.That(exporter.Export(gridMock, Arg<string>.Is.Anything), Is.False);
            string expectedLogMessage = Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(gridMock, Arg<string>.Is.Anything), expectedLogMessage);

            mocks.VerifyAll();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        public void GivenAGeometryZipExporterAndAnUnstructuredGrid_WhenExportIsCalled_ThenAZipContainingTheGeometryFilesIsCreated()
        {
            // Given
            // Create temporary folder
            string tempFolderPath = FileUtils.CreateTempDirectory();
            string inputPath = Path.Combine(tempFolderPath, "input");
            string outputFolderPath = Path.Combine(tempFolderPath, "output");
            FileUtils.CreateDirectoryIfNotExists(inputPath, true);
            FileUtils.CreateDirectoryIfNotExists(outputFolderPath, true);

            try
            {
                string testBaseFolder = TestHelper.GetTestFilePath(@"ReloadGrid");
                string mduFilePath = Path.Combine(testBaseFolder, "mdufile_projected_assigned.mdu");
                string destFilePath = Path.Combine(inputPath, Path.GetFileName(mduFilePath));
                File.Copy(mduFilePath, destFilePath, true);
                mduFilePath = destFilePath;

                var ncFileName = "netfile_projected_assigned.nc";
                var ncGeomFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}geom.nc";
                string ncFilePath = Path.Combine(testBaseFolder, ncFileName);
                destFilePath = Path.Combine(inputPath, Path.GetFileName(ncFilePath));
                File.Copy(ncFilePath, destFilePath, true);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                // Construct Path
                string outputFilePath = Path.Combine(outputFolderPath, "output.zip");

                // configure exporter
                exporter.GetModelForGrid = x => model;

                // When | Then
                Assert.That(exporter.Export(model.Grid, outputFilePath), Is.True);

                // Assert output exists
                int nFilesInOutput = Directory.GetFiles(outputFolderPath).Length;
                Assert.That(nFilesInOutput, Is.EqualTo(1));
                Assert.That(File.Exists(outputFilePath), Is.True);

                // Assert output is correct
                IList<string> filesInExportedZip = ZipFileUtils.GetFilePathsInZip(outputFilePath);
                Assert.That(filesInExportedZip.Count, Is.EqualTo(2));
                Assert.That(filesInExportedZip.Contains(ncFileName));
                Assert.That(filesInExportedZip.Contains(ncGeomFileName));

                // Assert that exported file is equal to input file.
                ZipFileUtils.Extract(outputFilePath, outputFolderPath);

                string inputFileChecksum = FileUtils.GetChecksum(ncFilePath);
                Assert.That(FileUtils.VerifyChecksum(Path.Combine(outputFolderPath, ncFileName), inputFileChecksum));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolderPath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        public void GivenAGeometryZipExporterAndAnUnstructuredGridAndFilesFromAnOldExport_WhenExportIsCalled_ThenAZipContainingTheGeometryFilesIsCreatedWithTheNamesmodified()
        {
            // Given
            // Create temporary folder
            string tempFolderPath = FileUtils.CreateTempDirectory();
            string inputPath = Path.Combine(tempFolderPath, "input");
            string outputFolderPath = Path.Combine(tempFolderPath, "output");
            FileUtils.CreateDirectoryIfNotExists(inputPath, true);
            FileUtils.CreateDirectoryIfNotExists(outputFolderPath, true);

            try
            {
                // Copy test data
                string testBaseFolder = TestHelper.GetTestFilePath(@"ReloadGrid");
                string mduFilePath = Path.Combine(testBaseFolder, "mdufile_projected_assigned.mdu");
                string destFilePath = Path.Combine(inputPath, Path.GetFileName(mduFilePath));
                File.Copy(mduFilePath, destFilePath, true);
                mduFilePath = destFilePath;

                var ncFileName = "netfile_projected_assigned.nc";
                var ncGeomFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}geom.nc";
                string ncFilePath = Path.Combine(testBaseFolder, ncFileName);
                destFilePath = Path.Combine(inputPath, Path.GetFileName(ncFilePath));
                File.Copy(ncFilePath, destFilePath, true);

                // add garbage output files
                using (StreamWriter tempWriter = File.CreateText(Path.Combine(outputFolderPath, ncFileName)))
                {
                    tempWriter.Write("this is definitely a test.");
                    tempWriter.Flush();
                }

                using (StreamWriter tempWriter = File.CreateText(Path.Combine(outputFolderPath, $"{Path.GetFileNameWithoutExtension(ncFileName)}(2).nc")))
                {
                    tempWriter.Write("Also this.");
                    tempWriter.Flush();
                }

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                // Construct Path
                string outputFilePath = Path.Combine(outputFolderPath, "output.zip");

                // configure exporter
                exporter.GetModelForGrid = x => model;

                // When | Then
                Assert.That(exporter.Export(model.Grid, outputFilePath), Is.True);

                // Assert output exists
                int nFilesInOutput = Directory.GetFiles(outputFolderPath).Length;
                Assert.That(nFilesInOutput, Is.EqualTo(3));
                Assert.That(File.Exists(outputFilePath), Is.True);

                // Assert output is correct
                var outputNcFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}(3).nc";

                IList<string> filesInExportedZip = ZipFileUtils.GetFilePathsInZip(outputFilePath);
                Assert.That(filesInExportedZip.Count, Is.EqualTo(2));
                Assert.That(filesInExportedZip.Contains(outputNcFileName));
                Assert.That(filesInExportedZip.Contains(ncGeomFileName));

                // Assert that exported file is equal to input file.
                ZipFileUtils.Extract(outputFilePath, outputFolderPath);

                string outputUnzippedFilePath = Path.Combine(outputFolderPath, outputNcFileName);
                string inputFileChecksum = FileUtils.GetChecksum(ncFilePath);
                Assert.That(FileUtils.VerifyChecksum(outputUnzippedFilePath, inputFileChecksum));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolderPath);
            }
        }

        [Test]
        public void GivenAGeometryZipExporterAndAnUnstructuredGridWithoutAModel_WhenExportIsCalled_ThenAnExceptionIsReturned()
        {
            var mocks = new MockRepository();
            var gridMock = mocks.DynamicMock<UnstructuredGrid>();
            gridMock.Expect(n => n.IsEmpty).Return(false).Repeat.Any();

            mocks.ReplayAll();

            Assert.That(() => exporter.Export(gridMock, Arg<string>.Is.Anything), Throws.InstanceOf<NotImplementedException>());
        }
    }
}