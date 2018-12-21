using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
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
            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);

            var waveModel = new WaveModel(localPath);
            Assert.IsNotNull(waveModel);

            var report = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, report.ErrorCount);
            ActivityRunner.RunActivity(waveModel);

            Assert.AreEqual(ActivityStatus.Cleaned, waveModel.Status);
        }

        [Test]
        public void RunBasicWaveModelWithTimePointsTest()
        {
            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);

            var waveModel = new WaveModel(localPath);
            Assert.IsNotNull(waveModel);

            var report = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, report.ErrorCount);
            waveModel.TimePointData.InputFields[waveModel.ModelDefinition.ModelReferenceDateTime] = new[] { 0, 0, 0, 0, 0 };
            waveModel.TimePointData.InputFields[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(12)] = new[] { 1, 1, 1, 1, 1 };
            waveModel.TimePointData.InputFields[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(24)] = new[] { 2, 2, 2, 2, 2 };
            waveModel.TimePointData.InputFields[waveModel.ModelDefinition.ModelReferenceDateTime.AddHours(36)] = new[] { 3, 3, 3, 3, 3 };
            ActivityRunner.RunActivity(waveModel);

            Assert.AreEqual(ActivityStatus.Cleaned, waveModel.Status);
            var fileFunctionStore = waveModel.WavmFunctionStores.FirstOrDefault();
            Assert.IsNotNull(fileFunctionStore);
            var function = fileFunctionStore.Functions.FirstOrDefault();
            Assert.IsNotNull(function);
            Assert.That(function.Arguments[0].Values.Count, Is.EqualTo(4));
        }
    }
}