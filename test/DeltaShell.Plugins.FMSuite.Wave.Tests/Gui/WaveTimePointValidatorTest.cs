using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveTimePointValidatorTest
    {
        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndNotCoupledToFlow_WhenValidating_ThenValidationErrorIsGiven()
        {
            var waveModel = new WaveModel {IsCoupledToFlow = false};

            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("No time points defined"));
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndCoupledToFlow_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            var waveModel = new WaveModel { IsCoupledToFlow = true };

            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsGiven()
        {
            var waveModel = new WaveModel();
            var timePoint = new DateTime(2000, 01, 01);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> {timePoint};
            var timePointData = waveModel.TimePointData;
            timePointData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.That(timePointData.TimePoints, Is.Not.Empty);

            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("Model start time precedes reference time"));
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            var waveModel = new WaveModel();
            var timePoint = waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> {timePoint};
            var timePointsData = waveModel.TimePointData;
            timePointsData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.That(timePointsData.TimePoints, Is.Not.Empty);

            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }
    }
}
