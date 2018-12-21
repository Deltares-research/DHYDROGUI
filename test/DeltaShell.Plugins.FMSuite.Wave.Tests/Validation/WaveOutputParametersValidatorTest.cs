using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveOutputParametersValidatorTest
    {
        [Test]
        public void GivenWaveModelWithoutObservationPointsAndWriteTableTrue_WhenValidatingOutputParameters_ThenErrorValidationIssueIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteTable).Value = true;

            // When
            var validationReport = WaveOutputParametersValidator.Validate(waveModel);

            // Then
            Assert.That(validationReport.Category, Is.EqualTo("Output parameters"));

            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Warning));
            var expectedMessage = Resources.WaveOutputParametersValidator_Validate_Option__Write_Tables__is_selected_but_there_are_no_Observation_Points_in_your_model_;
            Assert.That(validationIssue.Message, Is.EqualTo(expectedMessage));
        }
    }
}