using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Test_Given_MduFileWithRelativeRestartFile_When_LoadFromMdu_Then_ImportRestartFile_IsCalled()
        {
            // 1. Define test data.
            var fmModel = Substitute.ForPartsOf<WaterFlowFMModel>();
            string testPath = TestHelper.GetTestFilePath("MduFileWithRelativeRestart\\simplebox.mdu");
            string expectedFilePathCalled =
                TestHelper.GetTestFilePath("MduFileWithRelativeRestart\\original\\simplebox_20010101_000100_rst.nc");

            // 2. Verify initial expectations.
            Assert.That(File.Exists(testPath));
            Assert.That(File.Exists(expectedFilePathCalled));
            Assert.That(fmModel.UseRestart, Is.False);

            // 3. Define test action.
            TestDelegate testAction = () => fmModel.LoadFromMdu(testPath);
            
            // 3. Verify final expectations.
            Assert.That(testAction, Throws.Nothing);
            Assert.That(fmModel.UseRestart);
            fmModel.Received().ImportRestartFile(Arg.Is(expectedFilePathCalled));
        }
    }
}
