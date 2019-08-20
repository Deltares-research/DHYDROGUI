using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Extensions
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterQualityModelExtensionsTest
    {
        [Test]
        public void AddTextDocument_WaterQualityModel_OpenTextDocument()
        {
            // setup
            var mocks = new MockRepository();
            var model = mocks.Stub<WaterQualityModel>();
            var dataItems = new EventedList<IDataItem>();
            model.DataItems = dataItems;

            mocks.ReplayAll();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "a.txt");
            const string content = "test";
            FileUtils.DeleteIfExists(path);
            File.WriteAllText(path, content);

            try
            {
                // call
                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, path);

                // assert
                Assert.AreEqual(1, dataItems.Count);
                Assert.AreEqual(WaterQualityModel.GridDataItemMetaData.Name, model.DataItems[0].Name);
                var document = (TextDocumentFromFile)model.DataItems[0].Value;
                Assert.AreEqual(content, document.Content);
                Assert.IsTrue(document.IsOpen);
                Assert.IsTrue(document.ReadOnly);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void AddTextDocument_WaterQualityModelSecondTime_UpdateContents()
        {
            // setup
            var mocks = new MockRepository();
            var model = mocks.Stub<WaterQualityModel>();
            var dataItems = new EventedList<IDataItem>();
            model.DataItems = dataItems;

            mocks.ReplayAll();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "a.txt");
            const string content = "test";
            FileUtils.DeleteIfExists(path);
            File.WriteAllText(path, content);

            try
            {
                const string newContent = "Some other text";
                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, path);

                File.WriteAllText(path, newContent);

                // call
                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, path);

                // assert
                Assert.AreEqual(1, dataItems.Count);
                Assert.AreEqual(WaterQualityModel.GridDataItemMetaData.Name, model.DataItems[0].Name);
                var document = (TextDocumentFromFile)model.DataItems[0].Value;
                Assert.AreEqual(newContent, document.Content);
                Assert.IsTrue(document.IsOpen);
                Assert.IsTrue(document.ReadOnly);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void SetupModelDataFolderStructureTest()
        {
            // setup
            var model = new WaterQualityModel { Name = "My Model" };
            var projectDataDir = Path.Combine(Directory.GetCurrentDirectory(), "A");
            FileUtils.DeleteIfExists(projectDataDir);

            // call
            model.SetupModelDataFolderStructure(projectDataDir);

            // assert
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model"), model.ModelDataDirectory);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "output"), model.ModelSettings.OutputDirectory);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model_output"), model.ModelSettings.WorkDirectory);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "boundary_data_tables"), model.BoundaryDataManager.FolderPath);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "load_data_tables"), model.LoadsDataManager.FolderPath);

            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "output")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model_output")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "boundary_data_tables")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "load_data_tables")));
        }

        [TestCase("deltashell.map")]
        [TestCase("deltashell_map.nc")]
        public void ConnectMapOutput_(string fileName)
        {
            // Set-up
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", fileName);

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = tempDir.CopyTestDataFileToTempDirectory(testFilePath);
                var model = new WaterQualityModel {ModelSettings = {OutputDirectory = Path.GetDirectoryName(filePath)}};

                // Act
                model.ConnectMapOutput();

                // Assert
                Assert.That(model.MapFileFunctionStore.Path, Is.EqualTo(filePath));
            }
        }

        [Test]
        public void ConnectMapOutput_WhenBothMapFilesExist_ThenModelIsConnectedToTheNetCdfFile()
        {
            // Set-up
            string testDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");
            string mapFilePath = Path.Combine(testDirectory, "deltashell.map");
            string mapNetCdfFilePath = Path.Combine(testDirectory, "deltashell_map.nc");

            using (var tempDir = new TemporaryDirectory())
            {
                mapNetCdfFilePath = tempDir.CopyAllTestDataToTempDirectory(mapNetCdfFilePath, mapFilePath)[0];
                var model = new WaterQualityModel {ModelSettings = {OutputDirectory = tempDir.Path}};

                // Act
                model.ConnectMapOutput();

                // Assert
                Assert.That(model.MapFileFunctionStore.Path, Is.EqualTo(mapNetCdfFilePath));
            }
        }

        [Test]
        public void ConnectMapOutput_WithFileWithUnsupportedConvention_ThenFileIsNotConnectedAndWarningIsGiven()
        {
            // Set-up
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "NetCDFConventions",
                                               "CF1.5_UGRID0.9.nc");

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, "deltashell_map.nc");
                File.Copy(testFilePath, filePath);
                var model = new WaterQualityModel {ModelSettings = {OutputDirectory = Path.GetDirectoryName(filePath)}};

                // Action
                void TestAction()
                {
                    model.ConnectMapOutput();
                }

                // Assert
                IEnumerable<string> warningMessages = TestHelper.GetAllRenderedMessages(TestAction, Level.Warn);
                string expectedWarning = string.Format(
                    Resources.WaterQualityModel_File_does_not_meet_supported_UGRID_1_0_or_newer_standard,
                    Path.GetFileName(filePath));
                Assert.That(warningMessages, Contains.Item(expectedWarning));
                Assert.That(model.MapFileFunctionStore.Path, Is.EqualTo(null));
            }
        }
    }
}