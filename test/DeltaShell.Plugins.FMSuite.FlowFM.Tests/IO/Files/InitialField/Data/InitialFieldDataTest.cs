using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
{
    [TestFixture]
    public class InitialFieldDataTest
    {
        [Test]
        public void Constructor_SetsDefaultProperties()
        {
            // Act
            var initialField = new InitialFieldData();

            // Assert
            Assert.That(initialField.Quantity, Is.EqualTo(InitialFieldQuantity.None));
            Assert.That(initialField.DataFile, Is.Null);
            Assert.That(initialField.DataFileType, Is.EqualTo(InitialFieldDataFileType.None));
            Assert.That(initialField.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.None));
            Assert.That(initialField.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialField.AveragingType, Is.EqualTo(InitialFieldAveragingType.Mean));
            Assert.That(initialField.AveragingRelSize, Is.EqualTo(1.01));
            Assert.That(initialField.AveragingNumMin, Is.EqualTo(1));
            Assert.That(initialField.AveragingPercentile, Is.EqualTo(0.0));
            Assert.That(initialField.ExtrapolationMethod, Is.False);
            Assert.That(initialField.Value, Is.NaN);
            Assert.That(initialField.SpatialOperationName, Is.Null);
            Assert.That(initialField.SpatialOperationQuantity, Is.Null);
        }
    }
}