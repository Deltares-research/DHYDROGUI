using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{

    [TestFixture]
    public class GeometryZipExporterTest
    {
        private GeometryZipExporter exporter;

        [SetUp]
        public void Setup()
        {
            this.exporter = new GeometryZipExporter();
        }

        // TODO: Why are these here and shouldn't they be data access?
        [Test]
        public void TestWriteZValuesToNetFile()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\FilesUsingOldFormat\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);
            using (var ugridFile = new UGridFile(netFilePath))
                Assert.IsFalse(ugridFile.IsUGridFile());
            var grid = NetFileImporter.ImportGrid(netFilePath);

            var currentZValues = grid.Vertices.Select(v => v.Z);
            var newZValues = currentZValues.Select(z => { z = 123.456; return z; }).ToArray();

            NetFile.WriteZValues(netFilePath, newZValues);

            var adjustedGrid = NetFileImporter.ImportGrid(netFilePath);
            var zValues = adjustedGrid.Vertices.Select(v => v.Z);
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        public void TestWriteZValuesAtNodesToNetFile_UGrid()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);
            
            // get original grid
            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(netFilePath))
            {
                Assert.IsTrue(ugridFile.IsUGridFile());

                ugridFile.SetUnstructuredGrid(grid);
                Assert.IsFalse(grid.IsEmpty);


                // generate new z values
                double[] currentZValues;
                using (var uGridFile = new UGridFile(netFilePath))
                {
                    currentZValues = uGridFile.ReadZValues(BedLevelLocation.NodesMeanLev);
                }

                var newZValues = currentZValues.Select(z =>
                {
                    z = 123.456;
                    return z;
                }).ToArray();

                // write new coordinates to netfile
                ugridFile.WriteZValues(BedLevelLocation.NodesMaxLev, newZValues);

                // read new grid
                var adjustedGrid = new UnstructuredGrid();
                ugridFile.SetUnstructuredGrid(adjustedGrid);

                Assert.IsFalse(adjustedGrid.IsEmpty);

                // compare z values
                var zValues = ugridFile.ReadZValues(BedLevelLocation.NodesMeanLev);
                Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
            }
        }

        [Test]
        public void TestWriteZValuesAtCellCentersToNetFile_UGrid()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            // get original grid
            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(netFilePath))
            {
                Assert.IsTrue(ugridFile.IsUGridFile());

                ugridFile.SetUnstructuredGrid(grid);
                
                Assert.IsFalse(grid.IsEmpty);

                // generate new z values
                var newZValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();

                // write new coordinates to netfile
                ugridFile.WriteZValues(BedLevelLocation.Faces, newZValues);
            }

            using (var ncFile = new NetCdfFileWrapper(netFilePath))
            {
                // exported grid should contain zValue variable
                var zValues = ncFile.GetValues1D<double>("mesh2d_face_z");
                Assert.NotNull(zValues);
                Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
            }
        }

        [Test]
        public void GivenAGeometryZipExporterWhenNamePropertyIsCalledThenNameIsReturned()
        {
            const string expectedVal = "Net-geometry exporter";
            Assert.That(exporter.Name, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAGeometryZipExporterWhenCategoryIsCalledThenTheNameOfTheCategoryIsReturned()
        {
            const string expectedVal = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAGeometryZipExporterWhenSourceTypesIsCalledThenAnEnumerableContainingTheSourceTypesIsReturned()
        {
            var obtainedVals = exporter.SourceTypes();
            Assert.That(obtainedVals.Count(), Is.EqualTo(2));
            Assert.That(obtainedVals.Contains(typeof(UnstructuredGrid)));
            Assert.That(obtainedVals.Contains(typeof(UnstructuredGridCoverage)));
        }


        [Test]
        public void GivenAGeometryZipExporterWhenFileFilterPropertyIsCalledThenFileFilterIsReturned()
        {
            const string expectedVal = "Zip file|*.zip";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedVal));
        }


        [Test]
        public void GivenAGeometryZipExporterWhenCanExportForIsCalledWithNullThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(null), Is.True);
        }


        [Test]
        public void GivenAGeometryZipExporterWhenExportIsCalledWithNullAndAnyPathThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, Arg<string>.Is.Anything), Is.False);
        }


        [Test]
        public void GivenAGeometryZipExporterAndAnUnstructuredEmptyGridWhenExportIsCalledWithThisGridAndAnyPathThenFalseIsReturned()
        {
            var mocks = new MockRepository();
            var unstructuredGridMock = mocks.DynamicMock<UnstructuredGrid>();
            unstructuredGridMock.Expect(n => n.IsEmpty).Return(true).Repeat.Any();

            mocks.ReplayAll();
            Assert.That(exporter.Export(unstructuredGridMock, Arg<string>.Is.Anything), Is.False);

            var expectedLogMessage = Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(unstructuredGridMock, Arg<string>.Is.Anything), expectedLogMessage);

            mocks.VerifyAll();
        }


        [Test]
        public void GivenAGeometryZipExporterAndAValidEmptyModelWithAnEmptyGridWhenExportIsCalledWithTheEmptyGridAndAnyPathThenFalseIsReturnedAndAWarningIsLogged()
        {
            var mocks = new MockRepository();
            var gridMock = mocks.DynamicMock<UnstructuredGrid>();
            gridMock.Expect(n => n.IsEmpty).Return(true).Repeat.Any();

            mocks.ReplayAll();
            
            Assert.That(exporter.Export(gridMock, Arg<string>.Is.Anything), Is.False);
            var expectedLogMessage = Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(gridMock, Arg<string>.Is.Anything), expectedLogMessage);

            mocks.VerifyAll();
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        public void GivenAGeometryZipExporterAndAValidUnstructuredGridAndAValidFilePathAndMduFileWhenExportIsCalledWithThisGridAndPathThenAZipContainingTheGeometryFilesIsCreated()
        {
            // Given
            // Create temporary folder
            var tempFolderPath = FileUtils.CreateTempDirectory();
            var inputPath  = Path.Combine(tempFolderPath, "input");
            var outputFolderPath = Path.Combine(tempFolderPath, "output");
            FileUtils.CreateDirectoryIfNotExists(inputPath,  deleteIfExists:true);
            FileUtils.CreateDirectoryIfNotExists(outputFolderPath, deleteIfExists:true);

            try
            {
                var testBaseFolder = TestHelper.GetTestFilePath(@"ReloadGrid");
                var mduFilePath = Path.Combine(testBaseFolder, "mdufile_projected_assigned.mdu");
                var destFilePath = Path.Combine(inputPath, Path.GetFileName(mduFilePath));
                File.Copy(mduFilePath, destFilePath, true);
                mduFilePath = destFilePath;

                var ncFileName = "netfile_projected_assigned.nc";
                var ncGeomFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}geom.nc";
                var ncFilePath = Path.Combine(testBaseFolder, ncFileName);
                destFilePath = Path.Combine(inputPath, Path.GetFileName(ncFilePath));
                File.Copy(ncFilePath, destFilePath, true);
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
                var model = new WaterFlowFMModel(mduFilePath);

                // Construct Path
                var outputFilePath = Path.Combine(outputFolderPath, "output.zip");

                // configure exporter
                exporter.GetModelForGrid = x => model;

                // When | Then
                Assert.That(exporter.Export(model.Grid, outputFilePath), Is.True);

                // Assert output exists
                var nFilesInOutput = Directory.GetFiles(outputFolderPath).Length;
                Assert.That(nFilesInOutput, Is.EqualTo(1));
                Assert.That(File.Exists(outputFilePath), Is.True);

                // Assert output is correct
                var filesInExportedZip = ZipFileUtils.GetFilePathsInZip(outputFilePath);
                Assert.That(filesInExportedZip.Count, Is.EqualTo(2));
                Assert.That(filesInExportedZip.Contains(ncFileName));
                Assert.That(filesInExportedZip.Contains(ncGeomFileName));

                // Assert that exported file is equal to input file.
                ZipFileUtils.Extract(outputFilePath, outputFolderPath);

                var inputFileChecksum = FileUtils.GetChecksum(ncFilePath);
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
        public void GivenAGeometryZipExporterAndAValidUnstructuredGridAndAValidFilePathAndMduFileAndFilesAlreadyExistInExportWhenExportIsCalledWithThisGridAndPathThenAZipContainingTheGeometryFilesIsCreatedWithTheNamesmodified()
        {
            // Given
            // Create temporary folder
            var tempFolderPath = FileUtils.CreateTempDirectory();
            var inputPath  = Path.Combine(tempFolderPath, "input");
            var outputFolderPath = Path.Combine(tempFolderPath, "output");
            FileUtils.CreateDirectoryIfNotExists(inputPath,  deleteIfExists:true);
            FileUtils.CreateDirectoryIfNotExists(outputFolderPath, deleteIfExists:true);

            try
            {
                // Copy test data
                var testBaseFolder = TestHelper.GetTestFilePath(@"ReloadGrid");
                var mduFilePath = Path.Combine(testBaseFolder, "mdufile_projected_assigned.mdu");
                var destFilePath = Path.Combine(inputPath, Path.GetFileName(mduFilePath));
                File.Copy(mduFilePath, destFilePath, true);
                mduFilePath = destFilePath;

                var ncFileName = "netfile_projected_assigned.nc";
                var ncGeomFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}geom.nc";
                var ncFilePath = Path.Combine(testBaseFolder, ncFileName);
                destFilePath = Path.Combine(inputPath, Path.GetFileName(ncFilePath));
                File.Copy(ncFilePath, destFilePath, true);

                // add garbage output files
                using (var tempWriter = File.CreateText(Path.Combine(outputFolderPath, ncFileName)))
                {
                    tempWriter.Write("this is definitely a test.");
                    tempWriter.Flush();
                }

                using (var tempWriter = File.CreateText(Path.Combine(outputFolderPath, $"{Path.GetFileNameWithoutExtension(ncFileName)}(2).nc")))
                {
                    tempWriter.Write("Also this.");
                    tempWriter.Flush();
                }
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
                var model = new WaterFlowFMModel(mduFilePath);

                // Construct Path
                var outputFilePath = Path.Combine(outputFolderPath, "output.zip");

                // configure exporter
                exporter.GetModelForGrid = x => model;

                // When | Then
                Assert.That(exporter.Export(model.Grid, outputFilePath), Is.True);

                // Assert output exists
                var nFilesInOutput = Directory.GetFiles(outputFolderPath).Length;
                Assert.That(nFilesInOutput, Is.EqualTo(3));
                Assert.That(File.Exists(outputFilePath), Is.True);

                // Assert output is correct
                var outputNcFileName = $"{Path.GetFileNameWithoutExtension(ncFileName)}(3).nc";

                var filesInExportedZip = ZipFileUtils.GetFilePathsInZip(outputFilePath);
                Assert.That(filesInExportedZip.Count, Is.EqualTo(2));
                Assert.That(filesInExportedZip.Contains(outputNcFileName));
                Assert.That(filesInExportedZip.Contains(ncGeomFileName));

                // Assert that exported file is equal to input file.
                ZipFileUtils.Extract(outputFilePath, outputFolderPath);

                var outputUnzippedFilePath = Path.Combine(outputFolderPath, outputNcFileName);
                var inputFileChecksum = FileUtils.GetChecksum(ncFilePath);
                Assert.That(FileUtils.VerifyChecksum(outputUnzippedFilePath, inputFileChecksum));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolderPath);
            }
        }


        [Test]
        public void GivenAGeometryZipExporterAndAValidUnstructuredGridAndAValidFilePathWithoutGridToModelSetWhenExportIsCalledWithThisGridAndPathThenFalseIsReturned()
        {
            var mocks = new MockRepository();
            var gridMock = mocks.DynamicMock<UnstructuredGrid>();
            gridMock.Expect(n => n.IsEmpty).Return(false).Repeat.Any();

            mocks.ReplayAll();

            Assert.Throws<NotImplementedException>(() =>
            {
                exporter.Export(gridMock, Arg<string>.Is.Anything);
            });
        }

    }
}
