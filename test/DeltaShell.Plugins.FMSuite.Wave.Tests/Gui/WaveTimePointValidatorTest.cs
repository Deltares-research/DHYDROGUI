using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveTimePointValidatorTest
    {
        private WaveModel waveModel;

        [SetUp]
        public void Initialize()
        {
            waveModel = new WaveModel();
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndNotCoupledToFlow_WhenValidating_ThenValidationErrorIsGiven()
        {
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("No time points defined"));
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndCoupledToFlow_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            waveModel.IsCoupledToFlow = true;

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0), "A validation error(s) is given");
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsGiven()
        {
            var timePoint = new DateTime(2000, 01, 01);
            var timePoints = new List<DateTime> { timePoint };
            ITimeFrameData timePointData = waveModel.TimeFrameData;
            timePointData.TimeVaryingData.Arguments[0].AddValues(timePoints);
            Assert.That(timePointData.TimePoints, Is.Not.Empty);

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("Model start time precedes reference time"));
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            SetupModelWithTimePoints(1);

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        private void SetupModelWithTimePoints(int yearsToAdd)
        {
            DateTime timePoint = waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(yearsToAdd);
            var timePoints = new List<DateTime> { timePoint };
            ITimeFrameData timePointsData = waveModel.TimeFrameData;
            timePointsData.TimeVaryingData.Arguments[0].AddValues(timePoints);

            Assert.That(timePointsData.TimePoints, Is.Not.Empty);
        }
    }
}