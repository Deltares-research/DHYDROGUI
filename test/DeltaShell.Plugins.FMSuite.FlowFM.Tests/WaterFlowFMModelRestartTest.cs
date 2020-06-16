using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
            WaterFlowFMModel model = LoadBendProfModelWithWriteRestart();
            ActivityRunner.RunActivity(model);
            Assert.AreEqual(1, model.GetRestartOutputStates().Count());

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

            List<FileBasedRestartState> restartOutputStates = model.GetRestartOutputStates().ToList();
            Assert.IsNotEmpty(restartOutputStates);

            model.RestartInput = (FileBasedRestartState) restartOutputStates.First().Clone();
            Assert.NotNull(model.RestartInput);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("bendprof_20080905_040000_rst.nc", 4)]
        [TestCase("bendprof_20080905_120000_rst.nc", 12)]
        [TestCase("bendprof_20080905_220000_rst.nc", 22)]
        public void GivenRestartFile_WhenImported_ThenCorrectDataSetOnModel(string restartFile, int expectedHour)
        {
            // Given
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath("dummy.mdu"));

            // When
            model.ImportRestartFile(TestHelper.GetTestFilePath(Path.Combine(nameof(WaterFlowFMModelRestartTest), restartFile)));

            // Then
            Assert.That(model.RestartInput.SimulationTime, Is.EqualTo(new DateTime(2008, 9, 5, expectedHour,0,0)));
        }

        private static WaterFlowFMModel LoadBendProfModelWithWriteRestart()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            model.WriteRestart = true;
            model.OutputTimeStep = new TimeSpan(0, 0, 15);

            model.StopTime = model.StartTime.AddMinutes(2); //6 sec timestep
            model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = model.TimeStep;
            model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = model.TimeStep;

            return model;
        }
    }
}