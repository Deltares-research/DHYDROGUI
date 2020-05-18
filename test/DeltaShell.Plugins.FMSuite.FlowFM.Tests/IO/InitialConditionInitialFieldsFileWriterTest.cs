using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class InitialConditionInitialFieldsFileWriterTest
    {
        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel)]
        [TestCase(InitialConditionQuantity.WaterDepth)]
        public void GivenInitialConditionQuantity_WhenWritingToFile_ThenIsSameAsExpectedFile(
            InitialConditionQuantity globalQuantity)
        {
            var expectedFile = TestHelper.GetTestFilePath($"IO\\initialFields{globalQuantity}_expected.ini");
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                // setup
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    fmModel.ModelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D,
                        $"{(int) globalQuantity}");

                    // call
                    InitialConditionInitialFieldsFileWriter.WriteFile(actualFile, globalQuantity);

                    // assert
                    Assert.That(File.Exists(actualFile), Is.True);
                    FileAssert.AreEqual(actualFile, expectedFile); 
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }
    }
}