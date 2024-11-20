using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class FlowFMNetFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var importer = new FlowFMNetFileImporter();

            // Assert
            Assert.That(importer, Is.InstanceOf<IFileImporter>());
            Assert.That(importer.Name, Is.EqualTo("Unstructured Grid (UGRID)"));
            Assert.That(importer.Category, Is.EqualTo(Resources.FMImporters_Category_D_Flow_FM_2D_3D));
            Assert.That(importer.Description, Is.Empty);

            Assert.That(importer.SupportedItemTypes, Is.EquivalentTo(new[]
            {
                typeof(UnstructuredGrid)
            }));

            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.FileFilter, Is.EqualTo($"Net file|*{FileConstants.NetCdfFileExtension}"));
            Assert.That(importer.OpenViewAfterImport, Is.True);
        }

        private static IEnumerable<TestCaseData> GetCanImportOnData()
        {
            yield return new TestCaseData(null, null, true);
            yield return new TestCaseData(new object(), null, false);
            yield return new TestCaseData(new UnstructuredGrid(), null, false);

            WaterFlowFMModel ReturnsNull(UnstructuredGrid grid) => null;
            yield return new TestCaseData(new UnstructuredGrid(),
                                          (Func<UnstructuredGrid, IWaterFlowFMModel>) ReturnsNull,
                                          false);

            var model = Substitute.For<IWaterFlowFMModel>();
            IWaterFlowFMModel ReturnsModel(UnstructuredGrid grid) => model;
            yield return new TestCaseData(new UnstructuredGrid(),
                                          (Func<UnstructuredGrid, IWaterFlowFMModel>) ReturnsModel,
                                          true);

        }
        
        [Test]
        [TestCaseSource(nameof(GetCanImportOnData))]
        public void CanImportOn_ExpectedResults(object targetObject,
                                                Func<UnstructuredGrid, IWaterFlowFMModel> getModelForGridFunc,
                                                bool expectedResult)
        {
            // Setup
            var importer = new FlowFMNetFileImporter {GetModelForGrid = getModelForGridFunc};

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetImportItemPathDoesNotExistData()
        {
            yield return new TestCaseData(null, new object());
            yield return new TestCaseData("some/non-existent/path.nc", null);
        }

        [Test]
        [TestCaseSource(nameof(GetImportItemPathDoesNotExistData))]
        public void ImportItem_PathDoesNotExist_ThrowsFileNotFoundException(string path, object target)
        {
            // Setup
            var importer = new FlowFMNetFileImporter();

            // Call | Assert
            void Call() => importer.ImportItem(path, target);
            Assert.That(Call, Throws.InstanceOf<FileNotFoundException>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_GridTarget_ExpectedResults()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string gridFileLocation = tempDir.CreateDirectory("net");

                UnstructuredGrid oldGrid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

                var model = Substitute.For<IWaterFlowFMModel>();
                model.NetFilePath.Returns(Path.Combine(gridFileLocation, "file.nc"));

                var bathymetry = new UnstructuredGridCellCoverage(oldGrid, false);

                var spatialData = Substitute.For<ISpatialData>();
                spatialData.Bathymetry = bathymetry;

                model.SpatialData.Returns(spatialData);

                model.Grid = oldGrid;
                model.CoordinateSystem = null;

                var modelDefinition = new WaterFlowFMModelDefinition();
                model.ModelDefinition.Returns(modelDefinition);

                const string relativeSourceGridPath = "grid_generation/existing_grid.nc";
                string sourcePath = TestHelper.GetTestFilePath(relativeSourceGridPath);
                string path = tempDir.CopyTestDataFileToTempDirectory(sourcePath);

                var importer = new FlowFMNetFileImporter { GetModelForGrid = _ => model };

                // Call
                var result = importer.ImportItem(path, model.Grid) as UnstructuredGrid;

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(model.Grid, Is.SameAs(result));

                model.Received(1).ReloadGrid(false, true);

                Assert.That(model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value,
                            Is.EqualTo(Path.GetFileName(path)));

                // Clean up
                importer.GetModelForGrid = null;
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_TargetObjectNull_ReturnsDataItem()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string relativeSourceGridPath = "grid_generation/existing_grid.nc";
                string sourcePath = TestHelper.GetTestFilePath(relativeSourceGridPath);
                string path = tempDir.CopyTestDataFileToTempDirectory(sourcePath);

                var importer = new FlowFMNetFileImporter();

                // Call
                object result = importer.ImportItem(path, null);

                // Assert
                var dataItem = result as DataItem;
                Assert.That(dataItem, Is.Not.Null);
                Assert.That(dataItem.Name, Is.EqualTo(Path.GetFileName(path)));

                var importedNetFile = dataItem.Value as ImportedFMNetFile;
                Assert.That(importedNetFile, Is.Not.Null);
                Assert.That(importedNetFile.Path, Is.EqualTo(path));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_CannotImportOn_ReturnsNull()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string relativeSourceGridPath = "grid_generation/existing_grid.nc";
                string sourcePath = TestHelper.GetTestFilePath(relativeSourceGridPath);
                string path = tempDir.CopyTestDataFileToTempDirectory(sourcePath);

                var importer = new FlowFMNetFileImporter();

                // Call
                object result = importer.ImportItem(path, new object());

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportItem_ForANewGrid_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                // Arrange
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                const string text = "This is some text in the file.";

                using (FileStream fs = File.Create(restartFilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(text);
                    fs.Write(info, 0, info.Length);
                }
                
                model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
                model.ConnectOutput(tempDirectory.Path);

                // Act
                new FlowFMNetFileImporter().ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"), model);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }
    }
}

        
    
