using System.Collections;
using System.IO;
using DelftTools.Shell.Core.Workflow;
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

            string path = Path.Combine(Directory.GetCurrentDirectory(), "a.txt");
            const string content = "test";
            FileUtils.DeleteIfExists(path);
            File.WriteAllText(path, content);

            try
            {
                // call
                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, path);

                // Assert
                AssertCorrectDataItem(dataItems, model, content);
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

            using (var tempDirectory = new TemporaryDirectory())
            {
                string firstFilePath = Path.Combine(tempDirectory.Path, "one.txt");
                const string firstFileContent = "one";
                File.WriteAllText(firstFilePath, firstFileContent);
                string secondFilePath = Path.Combine(tempDirectory.Path, "two.txt");
                const string secondFileContent = "two";
                File.WriteAllText(secondFilePath, secondFileContent);

                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, firstFilePath);

                AssertCorrectDataItem(dataItems, model, firstFileContent);

                // Call
                model.AddTextDocument(WaterQualityModel.GridDataItemMetaData, secondFilePath);

                // Assert
                AssertCorrectDataItem(dataItems, model, secondFileContent);
            }
        }

        [Test]
        public void SetupModelDataFolderStructureTest()
        {
            // setup
            var model = new WaterQualityModel {Name = "My Model"};
            model.SetWorkingDirectoryInModelSettings(() => Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory"));
            string projectDataDir = Path.Combine(Directory.GetCurrentDirectory(), "A");
            FileUtils.DeleteIfExists(projectDataDir);

            // call
            model.SetupModelDataFolderStructure(projectDataDir);

            // assert
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model"), model.ModelDataDirectory);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "output"), model.ModelSettings.OutputDirectory);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "boundary_data_tables"), model.BoundaryDataManager.FolderPath);
            Assert.AreEqual(Path.Combine(projectDataDir, "My_Model", "load_data_tables"), model.LoadsDataManager.FolderPath);

            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "output")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model_output")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "boundary_data_tables")));
            Assert.IsFalse(Directory.Exists(Path.Combine(projectDataDir, "My_Model", "load_data_tables")));
        }

        private static void AssertCorrectDataItem(ICollection dataItems, IModel model, string content)
        {
            Assert.AreEqual(1, dataItems.Count);
            Assert.AreEqual(WaterQualityModel.GridDataItemMetaData.Name, model.DataItems[0].Name);
            var document = (TextDocument) model.DataItems[0].Value;
            Assert.That(document.Content, Is.EqualTo(content));
        }
    }
}