using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveModelRunTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void RunBasicWaveModelTest()
        {
            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);

            var waveModel = new WaveModel(localPath);
            Assert.IsNotNull(waveModel);

            var report = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, report.ErrorCount);
            ActivityRunner.RunActivity(waveModel);

            Assert.AreEqual(ActivityStatus.Cleaned, waveModel.Status);
        }
    }
}