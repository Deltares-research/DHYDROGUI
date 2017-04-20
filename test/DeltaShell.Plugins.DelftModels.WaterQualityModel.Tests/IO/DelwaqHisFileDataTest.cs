using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    // TODO: Move this class to another namespace so that it can be shared by WFDExplorer and WaterQualityModel1D
    [TestFixture]
    public class DelwaqHisFileDataTest
    {
        [Test]
        public void CreateDelwaqHisFileDataTest()
        {
            var hisFileData = new DelwaqHisFileData("Observation point");
            var timeStep1 = new DateTime(2010, 12, 1, 1, 0, 0);
            var timeStep2 = new DateTime(2010, 12, 1, 2, 0, 0);

            hisFileData.OutputVariables = new [] { "Substance 1", "Output parameter 1" };
            hisFileData.AddValueForTimeStep(timeStep1, 10.0);
            hisFileData.AddValueForTimeStep(timeStep1, 10.5);
            hisFileData.AddValueForTimeStep(timeStep2, 5.5);

            Assert.AreEqual("Observation point", hisFileData.ObservationVariable);
            Assert.AreEqual(2, hisFileData.OutputVariables.Count());
            Assert.AreEqual("Substance 1", hisFileData.OutputVariables.ElementAt(0));
            Assert.AreEqual("Output parameter 1", hisFileData.OutputVariables.ElementAt(1));
            Assert.AreEqual(2, hisFileData.TimeSteps.Count());
            Assert.AreEqual(timeStep1, hisFileData.TimeSteps.ElementAt(0));
            Assert.AreEqual(timeStep2, hisFileData.TimeSteps.ElementAt(1));

            var timeStep1Values = hisFileData.GetValuesForTimeStep(timeStep1);
            var timeStep2Values = hisFileData.GetValuesForTimeStep(timeStep2);
            Assert.AreEqual(2, timeStep1Values.Count);
            Assert.AreEqual(10.0, timeStep1Values[0]);
            Assert.AreEqual(10.5, timeStep1Values[1]);
            Assert.AreEqual(1, timeStep2Values.Count);
            Assert.AreEqual(5.5, timeStep2Values[0]);
        }
    }
}
