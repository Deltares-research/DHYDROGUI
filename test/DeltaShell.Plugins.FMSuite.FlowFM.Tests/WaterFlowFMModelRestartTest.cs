using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelRestartTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void WaterFlowFMModelRestartCoverageTest()
        {
            var model = LoadBendProfModelWithWriteRestart();
            ActivityRunner.RunActivity(model);
            Assert.AreEqual(1, model.GetRestartOutputStates().Count());

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

            var restartOutputStates = model.GetRestartOutputStates().ToList();
            Assert.IsNotEmpty(restartOutputStates);

            model.RestartInput = (FileBasedRestartState)restartOutputStates.First().Clone();
            Assert.NotNull(model.RestartInput);
        }

        private static WaterFlowFMModel.WaterFlowFMModel LoadBendProfModelWithWriteRestart()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel.WaterFlowFMModel(mduPath)
            {
                WriteRestart = true,
                OutputTimeStep = new TimeSpan(0, 0, 15)
            };

            model.StopTime = model.StartTime.AddMinutes(2); //6 sec timestep
            model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = model.TimeStep;
            model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = model.TimeStep;

            return model;
        }
    }
}