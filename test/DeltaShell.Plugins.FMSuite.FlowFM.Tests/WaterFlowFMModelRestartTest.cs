using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelRestartTest
    {
        [Test]
        public void WaterFlowFMModelRestartCoverageTest()
        {
            var model = new WaterFlowFMModel();
            Assert.NotNull(model);

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

            model.RestartInput = (FileBasedRestartState)model.GetRestartOutputStates().First().Clone();

            Assert.NotNull(model.RestartInput);
        }
        
    }
}