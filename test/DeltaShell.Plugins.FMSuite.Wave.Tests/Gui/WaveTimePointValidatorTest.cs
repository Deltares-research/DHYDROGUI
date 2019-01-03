using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveTimePointValidatorTest
    {
        [Test]
        public void IfStartTimePrecedesTheReferenceTimeAValidationErrorIsGiven()
        {
            var waveModel = new WaveModel();
            var timePoint = new DateTime(2000, 01, 01);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime>()
            {
                timePoint
            };

            waveModel.TimePointData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.NotNull(waveModel.TimePointData);
            Assert.That(waveModel.TimePointData.TimePoints, Is.Not.Empty);
            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Contains("Model Start time precedes Reference Time"));
        }

        [Test]
        public void IfStartTimeComesAfterTheReferenceValidationErrorIsNotGiven()
        {
            var waveModel = new WaveModel();
            var timePoint = waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime>()
            {
                timePoint
            };

            waveModel.TimePointData.InputFields.Arguments[0].AddValues(timePoints);

            Assert.NotNull(waveModel.TimePointData);
            Assert.That(waveModel.TimePointData.TimePoints, Is.Not.Empty);

            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void IfAWaveModelIsImportedAndTheStartTimePrecedesTheReferenceTimeAValidationErrorIsGiven()
        {
            var mdwFilePath = TestHelper.GetTestFilePath("waveTimePointValidator\\timePointPrecedesReferenceTime\\waves_bad.mdw");
            Assert.That(mdwFilePath, Is.Not.Null);

            var waveModel = new WaveModel(mdwFilePath);
            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Contains("Model Start time precedes Reference Time"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void IfAWaveModelIsImportedAndTheStartTimeComesAfterTheReferenceTimeAValidationErrorIsNotGiven()
        {
            var mdwFilePath = TestHelper.GetTestFilePath("waveTimePointValidator\\timePointComesAfterReferenceTime\\waves_good.mdw");
            Assert.That(mdwFilePath, Is.Not.Null);

            var waveModel = new WaveModel(mdwFilePath);
            var validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
            Assert.That(validationReport.AllErrors, Is.Empty);
        }
    }
}
