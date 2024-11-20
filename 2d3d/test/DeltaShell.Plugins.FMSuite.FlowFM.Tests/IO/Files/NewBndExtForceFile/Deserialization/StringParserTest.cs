using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class StringParserTest
    {
        [Test]
        [TestCase("1.23", 1.23)]
        [TestCase("1.2300000e+000", 1.23)]
        [TestCase("0.1230000e+001", 1.23)]
        public void TryParseToDouble_ValidCases(string value, double expResult)
        {
            // Setup
            bool success = value.TryParseToDouble(out double result);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("one")]
        public void TryParseToDouble_InvalidCases(string value)
        {
            // Setup
            bool success = value.TryParseToDouble(out double _);

            // Assert
            Assert.That(success, Is.False);
        }
    }
}