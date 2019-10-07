using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    class DataTableBoundaryImporterTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            var importer = new DataTableBoundaryImporter();

            Assert.IsInstanceOf<IFileImporter>(importer);
            Assert.AreEqual("Data table boundary importer", importer.Name);
            Assert.AreEqual("WAQ data tables", importer.Category);
            Assert.IsNull(importer.Image);
            var supportedTypes = importer.SupportedItemTypes.ToArray();
            Assert.AreEqual(1, supportedTypes.Length);
            CollectionAssert.Contains(supportedTypes, typeof(DataTableManager));
            Assert.IsFalse(importer.CanImportOnRootLevel);
            Assert.AreEqual("WAQ data table (*.csv)|*.csv", importer.FileFilter);
            Assert.IsNull(importer.TargetDataDirectory);
            Assert.IsFalse(importer.ShouldCancel);
            Assert.IsNull(importer.ProgressChanged);
            Assert.IsFalse(importer.OpenViewAfterImport);
        }

        [Test]
        public void ImportItem_TargetIsEmptyDataTableManager_ImportDataTablesFromSourceFile()
        {
            // setup
            var path = TestHelper.GetTestFilePath(Path.Combine("IO", "csv", "loads_multisubs.csv"));

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), TestHelper.GetCurrentMethodName());
            FileUtils.DeleteIfExists(folderPath);

            try
            {
                var target = new DataTableManager {FolderPath = folderPath};
                var importer = new DataTableBoundaryImporter();

                // call
                var importedItem = (DataTableManager) importer.ImportItem(path, target);

                // assert
                Assert.IsTrue(Directory.Exists(target.FolderPath),
                              "Should create required container folder as it wasn't created yet.");

                Assert.AreSame(target, importedItem);
                var dataTables = importedItem.DataTables.ToArray();
                Assert.AreEqual(1, dataTables.Length);
            }
            finally
            {
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void ImportItem_WithoutTarget_ThrowNotSupportedException()
        {
            // setup
            var path =
                TestHelper.GetTestFilePath(Path.Combine("IO","csv", "loads_multisubs.csv"));
            var importer = new DataTableBoundaryImporter();

            // call
            TestDelegate call = () => importer.ImportItem(path);

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Target of import must be an instance of DataTableManager.", exception.Message);
        }

        [Test]
        public void ImportItem_FileDoesNotExist_ThrowArgumentException()
        {
            // setup
            var importer = new DataTableBoundaryImporter();
            var target = new DataTableManager();
            target.FolderPath = "test";

            // call
            TestDelegate call = () => importer.ImportItem("Clearly not a valid file path", target);

            // assert
            var exception = Assert.Throws<ArgumentException>(call);
            var expectedMessage = "Not a valid file-path (Clearly not a valid file path) specified." +
                                  Environment.NewLine +
                                  "Parameter name: path";
            Assert.AreEqual(expectedMessage, exception.Message);
        }
    }
}