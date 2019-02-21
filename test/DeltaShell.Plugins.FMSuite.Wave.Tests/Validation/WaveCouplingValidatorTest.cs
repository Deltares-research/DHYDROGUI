using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveCouplingValidatorTest
    {
        [Test]
        public void GivenWaveModelCoupledToFlowAndReferenceTimePrecedingStartTime_WhenValidatingCoupling_ThenValidationErrorIsReturned()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                StartTime = DateTime.Now
            };
            waveModel.ModelDefinition.ModelReferenceDateTime = waveModel.StartTime.AddDays(-1); // Model reference time precedes model start time

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var errorMessages = validationReport.AllErrors.Select(issue => issue.Message).ToArray();
            Assert.Contains(Resources.WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time, errorMessages);
        }
    }
}