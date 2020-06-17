using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Test_Given_MduFileWithRelativeRestartFile_When_LoadFromMdu_Then_LoadsRestartState()
        {
            // 1. Given
            var fmModel = new WaterFlowFMModel();
            string testPath = TestHelper.GetTestFilePath("MduFileWithRelativeRestart\\simplebox.mdu");

            // 2. When
            Assert.That(File.Exists(testPath));
            TestDelegate testAction = () => fmModel.LoadFromMdu(testPath);
            
            // 3. Then
            Assert.That(testAction, Throws.Nothing);
        }
    }
}
