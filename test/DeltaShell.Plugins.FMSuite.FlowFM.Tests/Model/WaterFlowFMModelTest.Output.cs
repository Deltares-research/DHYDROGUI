using System.IO;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void ClearOutput_WithTextDocumentOutput_ThenOutputIsRemovedFromModel()
        {
            // Setup
            const string textDocumentTag = "myTextDocument";
            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, textDocumentTag));

            // Private field outputIsEmpty is set to false after a successful model run. This field should be false when clearing model output.
            // As we do not focus on model run, we use reflection to set this field and omit the model run.
            TypeUtils.SetField(waterFlowFmModel, "outputIsEmpty", false);

            // Call
            waterFlowFmModel.ClearOutput();

            // Assert
            Assert.That(waterFlowFmModel.GetDataItemByTag(textDocumentTag), Is.Null,
                        "Text Document data item should have been removed at model output clearance.");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ClearOutput_WithFunctionsStoreOutput_ThenStoresAreCleared()
        {
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "input", "FlowFM.mdu");

                var waterFlowFmModel = new WaterFlowFMModel(mduFilePath);
                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;
                string classMapFilePath = waterFlowFmModel.OutputClassMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(hisFilePath), Is.True, "Test pre-condition failure.");
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(mapFilePath), Is.True, "Test pre-condition failure.");
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(classMapFilePath), Is.True, "Test pre-condition failure.");

                // Call
                waterFlowFmModel.ClearOutput();

                // Assert
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null, "His file store should be set to null at model output clearance.");
                Assert.That(File.Exists(hisFilePath), Is.True, "Model output files should not be removed at model output clearance.");
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null, "Output map file store should be set to null at model output clearance.");
                Assert.That(File.Exists(mapFilePath), Is.True, "Model output files should not be removed at model output clearance.");
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Null, "Class map file store should be set to null at model output clearance.");
                Assert.That(File.Exists(classMapFilePath), Is.True, "Model output files should not be removed at model output clearance.");
            }
        }
    }
}