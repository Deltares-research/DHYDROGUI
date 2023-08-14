using System;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class LateralValidatorTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            // Call
            void Call() => lateralValidator.Validate(null, 0);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_ArgNegative_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);
            var lateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None, null, null, null, null);

            // Call
            void Call() => lateralValidator.Validate(lateralDTO, -1);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Validate_MissingId_ReturnsFalseAndReportsError(string id)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO(id, "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'id' must be provided. Line: 3");
        }

        [Test]
        public void Validate_UnsupportedForcingType_ReturnsFalseAndReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Unsupported, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'type' contains an unsupported value. Supported values: discharge. Line: 3");
        }

        [Test]
        public void Validate_UnsupportedLocationType_ReturnsFalseAndReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'locationType' contains an unsupported value. Supported values: 2d. Line: 3");
        }

        [Test]
        public void Validate_IncompleteLocationSpecification_ReturnsFalseAndReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, null, null, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Properties 'numCoordinates', 'xCoordinates' and 'yCoordinates' must be provided. Line: 3");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2)]
        public void Validate_InvalidNumCoordinates_ReturnsFalseAndReportsError(int numCoordinates)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            numCoordinates, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'numCoordinates' must either be 1 (point) or any value greater than 2 (polygon). Line: 3");
        }

        [Test]
        public void Validate_IncorrectXCoordinateCount_ReturnsFalseAndReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("The number of values of property 'xCoordinates' must be equal to the value of property 'numCoordinates'. Line: 3");
        }

        [Test]
        public void Validate_IncorrectYCoordinateCount_ReturnsFalseAndReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("The number of values of property 'yCoordinates' must be equal to the value of property 'numCoordinates'. Line: 3");
        }

        [Test]
        public void Validate_ValidLateralDTO_ReturnsTrue()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralValidator = new LateralValidator(logHandler);

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            var discharge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7
            };
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge);

            // Call
            bool result = lateralValidator.Validate(lateralDTO, 3);

            // Assert
            Assert.That(result, Is.True);
            logHandler.Received(0).ReportError(Arg.Any<string>());
        }
    }
}