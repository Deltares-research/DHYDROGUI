using System;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    [TestFixture]
    public class LateralDTOTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string id = "some_id";
            const string name = "some_name";
            const LateralForcingType type = LateralForcingType.Discharge;
            const LateralLocationType locationType = LateralLocationType.TwoD;
            const int numCoordinates = 3;
            var xCoordinates = new[] { 1.23, 3.45, 4.56 };
            var yCoordinates = new[] { 5.67, 6.78, 7.89 };
            var discharge = new Steerable();

            // Call
            var lateralDTO = new LateralDTO(id, name, type, locationType,
                                            numCoordinates, xCoordinates, yCoordinates, discharge);

            // Assert
            Assert.That(lateralDTO.Id, Is.EqualTo(id));
            Assert.That(lateralDTO.Name, Is.EqualTo(name));
            Assert.That(lateralDTO.Type, Is.EqualTo(type));
            Assert.That(lateralDTO.LocationType, Is.EqualTo(locationType));
            Assert.That(lateralDTO.NumCoordinates, Is.EqualTo(numCoordinates));
            Assert.That(lateralDTO.XCoordinates, Is.EquivalentTo(xCoordinates));
            Assert.That(lateralDTO.YCoordinates, Is.EquivalentTo(yCoordinates));
            Assert.That(lateralDTO.Discharge, Is.SameAs(discharge));
            Assert.That(lateralDTO.LineNumber, Is.Zero);
        }

        [Test]
        public void SetLineNumber_ValueIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            LateralDTO lateralDTO = CreateLateralDto();

            // Assert
            Assert.That(() => lateralDTO.LineNumber = -1, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void SetLineNumber_CanSetZeroOrPositiveValue(int lineNumber)
        {
            // Setup
            LateralDTO lateralDTO = CreateLateralDto();

            // Call
            lateralDTO.LineNumber = lineNumber;

            // Assert
            Assert.That(lateralDTO.LineNumber, Is.EqualTo(lineNumber));
        }

        private LateralDTO CreateLateralDto()
        {
            var xCoordinates = new[] { 1.23, 3.45, 4.56 };
            var yCoordinates = new[] { 5.67, 6.78, 7.89 };
            var discharge = new Steerable();

            return new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                  3, xCoordinates, yCoordinates, discharge);
        }
    }
}