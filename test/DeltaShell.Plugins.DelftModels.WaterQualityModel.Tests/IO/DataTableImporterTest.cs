using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DataTableImporterTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            // call
            var importer = new DataTableImporter();

            // assert
            Assert.IsInstanceOf<IFileImporter>(importer);
            Assert.AreEqual("Data table importer", importer.Name);
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
        public void ImportItem_TargetNotDataTableInstance_ThrowNotSupportedException()
        {
            // setup
            var path =
                TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeBlock.csv"));
            var importer = new DataTableImporter();

            // call
            TestDelegate call = () => importer.ImportItem(path, new object());

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Target of import must be an instance of DataTableManager.", exception.Message);
        }

        [Test]
        public void ImportItem_WithoutTarget_ThrowNotSupportedException()
        {
            // setup
            var path =
                TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeBlock.csv"));
            var importer = new DataTableImporter();

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
            var importer = new DataTableImporter();
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

        [Test]
        public void ImportItem_TargetIsEmptyDataTableManager_ImportDataTablesFromSourceFile()
        {
            // setup
            var path =
                TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeBlock.csv"));
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), TestHelper.GetCurrentMethodName());
            FileUtils.DeleteIfExists(folderPath);
            try
            {
                var target = new DataTableManager {FolderPath = folderPath};
                var importer = new DataTableImporter();

                // call
                var importedItem = (DataTableManager) importer.ImportItem(path, target);

                // assert
                Assert.IsTrue(Directory.Exists(target.FolderPath),
                              "Should create required container folder as it wasn't created yet.");

                Assert.AreSame(target, importedItem);
                var dataTables = importedItem.DataTables.ToArray();
                Assert.AreEqual(1, dataTables.Length);
                var dataTable = dataTables[0];
                Assert.AreEqual("timeBlock", dataTable.Name);
                Assert.IsTrue(dataTable.IsEnabled);

                Assert.AreEqual("timeBlock", dataTable.DataFile.Name);
                Assert.IsTrue(dataTable.DataFile.IsOpen);
                Assert.IsTrue(File.Exists(dataTable.DataFile.Path));
                foreach (var locationName in new[]
                {
                    "locA",
                    "locB",
                    "locC",
                    "locD",
                    "locE"
                })
                {
                    var pattern = string.Format("ITEM{0}'{1}'{0}CONCENTRATIONS{0}", Environment.NewLine, locationName);
                    Assert.IsTrue(new Regex(pattern).IsMatch(dataTable.DataFile.Content),
                                  "Should have an item definition for location: " + locationName);
                }

                var itemRegexPattern =
                    "DATA_ITEM" + Environment.NewLine +
                    "\\S+" + Environment.NewLine +
                    "CONCENTRATIONS" + Environment.NewLine +
                    "INCLUDE \\S+" + Environment.NewLine +
                    "TIME BLOCK DATA" + Environment.NewLine;
                Assert.AreEqual(5, new Regex(itemRegexPattern).Matches(dataTable.DataFile.Content).Count);

                Assert.AreEqual("timeBlock", dataTable.SubstanceUseforFile.Name);
                Assert.IsTrue(dataTable.SubstanceUseforFile.IsOpen);
                Assert.IsTrue(File.Exists(dataTable.SubstanceUseforFile.Path));
                Assert.AreEqual(2, new Regex("USEFOR").Matches(dataTable.SubstanceUseforFile.Content).Count);
                foreach (var substanceName in new[]
                {
                    "SubA",
                    "SubB"
                })
                {
                    var pattern = string.Format("USEFOR '{0}' '{0}'", substanceName);
                    Assert.IsTrue(new Regex(pattern).IsMatch(dataTable.SubstanceUseforFile.Content),
                                  "Should have an item definition for location: " + substanceName);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void FilePathNullWhenInstantiatingDataTableImporter()
        {
            // setup
            var importer = new DataTableImporter();

            // assert
            Assert.IsNull(importer.FilePath); // TODO check the test to perform
        }
    }
}