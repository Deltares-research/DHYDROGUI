using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DelwaqHisFileDataTest
    {
        [Test]
        public void CreateDelwaqHisFileDataTest()
        {
            var hisFileData = new DelwaqHisFileData("Observation point");
            var timeStep1 = new DateTime(2010, 12, 1, 1, 0, 0);
            var timeStep2 = new DateTime(2010, 12, 1, 2, 0, 0);

            hisFileData.OutputVariables = new[]
            {
                "Substance 1",
                "Output parameter 1"
            };
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

            List<double> timeStep1Values = hisFileData.GetValuesForTimeStep(timeStep1);
            List<double> timeStep2Values = hisFileData.GetValuesForTimeStep(timeStep2);
            Assert.AreEqual(2, timeStep1Values.Count);
            Assert.AreEqual(10.0, timeStep1Values[0]);
            Assert.AreEqual(10.5, timeStep1Values[1]);
            Assert.AreEqual(1, timeStep2Values.Count);
            Assert.AreEqual(5.5, timeStep2Values[0]);
        }

        [Test]
        public void When_GetValuesForKey_No_OutputVariables_Then_Returns_EmptyEnumerable()
        {
            // 1. Set up test data.
            var hisFileData = new DelwaqHisFileData("Observation point");
            const string outputKey = "dummyKey";
            IEnumerable<double> resultValues = Enumerable.Empty<double>();

            // 2. Verify initial conditions
            Assert.That(hisFileData.OutputVariables, Is.Null);

            // 3. Run test
            resultValues = hisFileData.GetValuesForKey(outputKey);

            // 4. Verify final expectations
            Assert.That(resultValues, Is.EqualTo(Enumerable.Empty<double>()));
        }

        [Test]
        public void When_GetValuesForKey_Key_DoesNot_Exist_Then_Returns_EmptyEnumerable()
        {
            // 1. Set up test data.
            var hisFileData = new DelwaqHisFileData("Observation point")
            {
                OutputVariables = new[]
                {
                    "dummyValue"
                }
            };
            const string outputKey = "dummyKey";
            IEnumerable<double> resultValues = Enumerable.Empty<double>();

            // 2. Verify initial conditions
            Assert.That(hisFileData.OutputVariables, Is.Not.Null);

            // 3. Run test
            resultValues = hisFileData.GetValuesForKey(outputKey);

            // 4. Verify final expectations
            Assert.That(resultValues, Is.Empty);
        }

        [Test]
        public void When_GetValuesForKey_Key_Exists_Then_Returns_ListOfValues()
        {
            // 1. Set up test data.
            const string outputKey = "dummyKey";
            var outputValues = new[]
            {
                10.0,
                5.5
            };
            var firstTimeStep = new DateTime(2010, 12, 1, 1, 0, 0);
            var lastTimeStep = new DateTime(2010, 12, 1, 2, 0, 0);

            var hisFileData = new DelwaqHisFileData("Observation point");
            hisFileData.OutputVariables = new[]
            {
                outputKey,
                "Output parameter 1"
            };
            hisFileData.AddValueForTimeStep(firstTimeStep, outputValues[0]);
            hisFileData.AddValueForTimeStep(firstTimeStep, 10.5);
            hisFileData.AddValueForTimeStep(lastTimeStep, outputValues[1]);

            IEnumerable<double> resultValues = Enumerable.Empty<double>();

            // 2. Verify initial conditions
            Assert.That(hisFileData.OutputVariables, Is.Not.Null);
            Assert.That(hisFileData.TimeSteps.Any(), Is.True);

            // 3. Run test
            resultValues = hisFileData.GetValuesForKey(outputKey);

            // 4. Verify final expectations
            Assert.That(resultValues, Is.EqualTo(outputValues));
        }
    }
}