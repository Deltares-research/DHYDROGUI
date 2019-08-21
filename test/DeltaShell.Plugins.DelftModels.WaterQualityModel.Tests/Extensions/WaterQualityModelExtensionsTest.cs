using System.IO;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
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
    }
}