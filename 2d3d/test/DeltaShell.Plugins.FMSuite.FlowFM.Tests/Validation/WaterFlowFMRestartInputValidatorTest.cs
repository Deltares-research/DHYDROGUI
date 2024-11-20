using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMRestartInputValidatorTest
    {
        [Test]
        public void Validate_ModelNull_ThrowsException()
        {
            // Call
            void Call() => WaterFlowFMRestartInputValidator.Validate(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IfModelDoesNotUseRestart_ReturnsEmptyValidationReport()
        {
            // Setup
            var restartModel = Substitute.For<IRestartModel<WaterFlowFMRestartFile>>();
            restartModel.UseRestart.Returns(false);

            // Call
            ValidationReport validationReport = WaterFlowFMRestartInputValidator.Validate(restartModel);

            // Assert
            const string expectedCategory = "Input restart state";
            Assert.That(validationReport.Category, Is.EqualTo(expectedCategory));
            Assert.That(validationReport.IsEmpty);
        }

        [Test]
        public void RestartInputFileDoesNotExist_AddValidationIssueToReport()
        {
            // Setup
            var restartModel = Substitute.For<IRestartModel<WaterFlowFMRestartFile>>();
            restartModel.UseRestart.Returns(true);

            var restartFile = new WaterFlowFMRestartFile("thisPathDoesNotExist");
            restartModel.RestartInput.Returns(restartFile);

            // Call
            ValidationReport validationReport = WaterFlowFMRestartInputValidator.Validate(restartModel);

            // Assert
            const string expectedCategory = "Input restart state";
            Assert.That(validationReport.Category, Is.EqualTo(expectedCategory));
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));

            const string expectedError = "Input restart file does not exist; cannot restart.";
            ValidationIssue error = validationReport.AllErrors.First();
            Assert.That(error.Message, Is.EqualTo(expectedError));
        }

        [Test]
        public void Validate_ValidModel_ReturnsEmptyValidationReport()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                var restartModel = Substitute.For<IRestartModel<WaterFlowFMRestartFile>>();
                restartModel.UseRestart.Returns(true);

                string restartFilepath = tempDirectory.CreateFile("randomName_rst.nc");
                var restartFile = new WaterFlowFMRestartFile(restartFilepath);
                restartModel.RestartInput.Returns(restartFile);

                // Call
                ValidationReport validationReport = WaterFlowFMRestartInputValidator.Validate(restartModel);

                // Assert
                const string expectedCategory = "Input restart state";
                Assert.That(validationReport.Category, Is.EqualTo(expectedCategory));
                Assert.That(validationReport.IsEmpty);
            }
        }
    }
}