using System;
using System.Collections.Generic;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainfallRunoffMeteoValidatorHelperTest
    {
        private static IEnumerable<TestCaseData> HasCorrectNumberValuesData()
        {
            var singleValueTimeArgument = new Variable<DateTime>();
            singleValueTimeArgument.Values.Add(new DateTime(2020, 05, 1));

            var multipleValueTimeArgument = new Variable<DateTime>();

            multipleValueTimeArgument.Values.Add(new DateTime(2020, 05, 1));
            multipleValueTimeArgument.Values.Add(new DateTime(2020, 05, 2));
            multipleValueTimeArgument.Values.Add(new DateTime(2020, 05, 3));

            DateTime time = new DateTime(2022, 05, 2);
            
            yield return new TestCaseData(multipleValueTimeArgument, time, time.AddDays(1), true);
            yield return new TestCaseData(multipleValueTimeArgument, time, time, true);
            yield return new TestCaseData(singleValueTimeArgument, time, time.AddDays(1), false);
            yield return new TestCaseData(singleValueTimeArgument, time, time, true);
        }

        [Test]
        [TestCaseSource(nameof(HasCorrectNumberValuesData))]
        public void HasCorrectNumberValues_ExpectedResult(Variable<DateTime> timeArgument,
                                                          DateTime startTime,
                                                          DateTime stopTime,
                                                          bool expectedResult)
        {
            bool result = timeArgument.HasCorrectNumberValues(startTime, stopTime);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> HasCorrectStartTimeData()
        {
            DateTime time = new DateTime(2022, 05, 2);

            var timeArgument = new Variable<DateTime>();
            timeArgument.Values.Add(time);
            timeArgument.Values.Add(time.AddDays(1));
            timeArgument.Values.Add(time.AddDays(2));

            yield return new TestCaseData(timeArgument, time, true);
            yield return new TestCaseData(timeArgument, time.Subtract(TimeSpan.FromDays(1)), false);
            yield return new TestCaseData(timeArgument, time.Add(TimeSpan.FromDays(2)), true);
        }

        [Test]
        [TestCaseSource(nameof(HasCorrectStartTimeData))]
        public void HasCorrectStartTime_ExpectedResult(Variable<DateTime> timeArgument,
                                                       DateTime startTime,
                                                       bool expectedResult)
        {
            bool result = timeArgument.HasCorrectStartingTime(startTime);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> HasCorrectStopTimeData()
        {
            DateTime time = new DateTime(2022, 05, 2);

            var singleTimeArgument = new Variable<DateTime>();
            singleTimeArgument.Values.Add(time);

            var multipleTimeArgument = new Variable<DateTime>();
            multipleTimeArgument.Values.Add(time);
            multipleTimeArgument.Values.Add(time.AddDays(1));
            multipleTimeArgument.Values.Add(time.AddDays(2));
            multipleTimeArgument.Values.Add(time.AddDays(3));
            multipleTimeArgument.Values.Add(time.AddDays(4));

            yield return new TestCaseData(singleTimeArgument, time, false, true);

            yield return new TestCaseData(multipleTimeArgument, time, false, true);
            yield return new TestCaseData(multipleTimeArgument, time, true, true);
            yield return new TestCaseData(multipleTimeArgument, time.AddDays(5), false, false);
            yield return new TestCaseData(multipleTimeArgument, time.AddDays(5), true, true);
            yield return new TestCaseData(multipleTimeArgument, time.AddDays(6), true, false);
        }

        [Test]
        [TestCaseSource(nameof(HasCorrectStopTimeData))]
        public void HasCorrectStopTime_ExpectedResults(Variable<DateTime> timeArgument,
                                                       DateTime stopTime,
                                                       bool addTimeStep,
                                                       bool expectedResult)
        {
            bool result = timeArgument.HasCorrectStopTime(stopTime, addTimeStep);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> HasCorrectTimeStepData()
        {
            DateTime time = new DateTime(2022, 05, 2);

            var singleTimeArgument = new Variable<DateTime>();
            singleTimeArgument.Values.Add(time);

            var multipleTimeArgument = new Variable<DateTime>();
            multipleTimeArgument.Values.Add(time);
            multipleTimeArgument.Values.Add(time.AddDays(1));
            multipleTimeArgument.Values.Add(time.AddDays(2));
            multipleTimeArgument.Values.Add(time.AddDays(3));
            multipleTimeArgument.Values.Add(time.AddDays(4));

            yield return new TestCaseData(singleTimeArgument, TimeSpan.FromSeconds(1), true);
            yield return new TestCaseData(multipleTimeArgument, TimeSpan.Zero, true);
            yield return new TestCaseData(multipleTimeArgument, TimeSpan.FromDays(2), false);
            yield return new TestCaseData(multipleTimeArgument, TimeSpan.FromHours(5), false);
            yield return new TestCaseData(multipleTimeArgument, TimeSpan.FromHours(1), true);
        }

        [Test]
        [TestCaseSource(nameof(HasCorrectTimeStepData))]
        public void HasCorrectTimeStep_ExpectedResults(Variable<DateTime> timeArgument,
                                                       TimeSpan timeStep,
                                                       bool expectedResult)
        {
            bool result = timeArgument.HasCorrectTimeStep(timeStep);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> GetMeteoEndData()
        {
            DateTime time = new DateTime(2022, 05, 2);

            var multipleTimeArgument = new Variable<DateTime>();
            multipleTimeArgument.Values.Add(time);
            multipleTimeArgument.Values.Add(time.AddDays(1));
            multipleTimeArgument.Values.Add(time.AddDays(2));
            multipleTimeArgument.Values.Add(time.AddDays(3));
            multipleTimeArgument.Values.Add(time.AddDays(4));

            yield return new TestCaseData(multipleTimeArgument, false, time.AddDays(4));
            yield return new TestCaseData(multipleTimeArgument, true, time.AddDays(5));
        }

        [Test]
        [TestCaseSource(nameof(GetMeteoEndData))]
        public void GetMeteoEnd_ExpectedResults(IVariable<DateTime> timeArgument,
                                                bool addTimeStep,
                                                DateTime expectedDateTime)
        {
            DateTime result = timeArgument.GetMeteoEnd(addTimeStep);
            Assert.That(result, Is.EqualTo(expectedDateTime));
        }

        [Test]
        public void GetMeteoTimeStep_ExpectedResults()
        {
            var timeArgument = new Variable<DateTime>();
            DateTime time = new DateTime(2022, 05, 2);

            timeArgument.Values.Add(time);
            timeArgument.Values.Add(time.AddDays(1));
            timeArgument.Values.Add(time.AddDays(2));
            timeArgument.Values.Add(time.AddDays(3));
            timeArgument.Values.Add(time.AddDays(4));

            TimeSpan result = timeArgument.GetMeteoTimeStep();
            Assert.That(result, Is.EqualTo(TimeSpan.FromDays(1)));
        }
    }
}