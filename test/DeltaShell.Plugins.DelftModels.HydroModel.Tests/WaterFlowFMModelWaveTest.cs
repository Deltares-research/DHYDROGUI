using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class WaterFlowFMModelWaveTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void RunCoupledWaveFlowFMTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"waveFlowFM\fm\te0.mdu");
            var localFmPath = TestHelper.CreateLocalCopy(mduPath);
            var fmModel = new WaterFlowFMModel(localFmPath);
            fmModel.StopTime = fmModel.StartTime + new TimeSpan(20*fmModel.TimeStep.Ticks);
            var fmReport = fmModel.Validate();
            Assert.AreEqual(0, fmReport.ErrorCount);

            var mdwPath = TestHelper.GetTestFilePath(@"waveFlowFM\wave\te0.mdw");
            var localWavePath = TestHelper.CreateLocalCopy(mdwPath);
            var waveModel = new WaveModel(localWavePath) {IsCoupledToFlow = true};
            var waveReport = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(1, waveReport.ErrorCount);//if not in an integrated model Coupling error should occur!
            Assert.IsTrue(
                waveReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == Resources.WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file));

            var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.All);
            hydroModel.Activities.Clear();

            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(waveModel);
            waveReport = new WaveModelValidator().Validate(waveModel);
            Assert.AreEqual(0, waveReport.ErrorCount);//if in an integrated model Coupling error should NOT occur!

            hydroModel.StartTime = fmModel.StartTime;
            hydroModel.StopTime = fmModel.StopTime;
            hydroModel.TimeStep = fmModel.TimeStep;

            hydroModel.CurrentWorkflow = new ParallelActivity { Activities = { new ActivityWrapper(waveModel), new ActivityWrapper(fmModel) } };
            
            ActivityRunner.RunActivity(hydroModel);

            Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);
        }
    }
}
