using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaveModelRunTest
    {
        [Test]
        public void RunBasicWaveModelTest()
        {
            string path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            string localPath = TestHelper.CreateLocalCopy(path);

            var waveModel = new WaveModel(localPath);
            Assert.IsNotNull(waveModel);

            ValidationReport report = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, report.ErrorCount);
            ActivityRunner.RunActivity(waveModel);

            Assert.AreEqual(ActivityStatus.Cleaned, waveModel.Status);
            Assert.IsFalse(waveModel.OutputIsEmpty);
        }

        [Test]
        public void RunBasicWaveModelWithTimePointsTest()
        {
            string path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            string localPath = TestHelper.CreateLocalCopy(path);

            var waveModel = new WaveModel(localPath);
            Assert.IsNotNull(waveModel);

            ValidationReport report = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, report.ErrorCount);
            waveModel.TimeFrameData.TimeVaryingData[waveModel.ModelDefinition.ModelReferenceDateTime] = new[]
            {
                0,
                0,
                0,
                0,
                0
            };
            waveModel.TimeFrameData.TimeVaryingData[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(12)] = new[]
            {
                1,
                1,
                1,
                1,
                1
            };
            waveModel.TimeFrameData.TimeVaryingData[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(24)] = new[]
            {
                2,
                2,
                2,
                2,
                2
            };
            waveModel.TimeFrameData.TimeVaryingData[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(36)] = new[]
            {
                3,
                3,
                3,
                3,
                3
            };
            ActivityRunner.RunActivity(waveModel);

            Assert.AreEqual(ActivityStatus.Cleaned, waveModel.Status);

            // TODO: Add new output logic here, should be done as part of the epic D3DFMIQ-2272.
            // WavmFileFunctionStore fileFunctionStore = waveModel.WavmFunctionStores.FirstOrDefault();
            // Assert.IsNotNull(fileFunctionStore);
            // IFunction function = fileFunctionStore.Functions.FirstOrDefault();
            // Assert.IsNotNull(function);
            // Assert.That(function.Arguments[0].Values.Count, Is.EqualTo(4));
        }
    }
}