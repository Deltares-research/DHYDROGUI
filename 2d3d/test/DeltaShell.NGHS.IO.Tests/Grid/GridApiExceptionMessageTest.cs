using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class GridApiExceptionMessageTest
    {
        [Test]
        public void Format_WithErrorCode_AndMessage_ReturnsCorrectFormattedString()
        {
            // Setup
            const int errorCode = 123;
            const string message = "This is a message";

            // Call
            string result = GridApiExceptionMessage.Format(errorCode, message);

            // Assert
            Assert.That(result, Is.EqualTo("GridApi returned error code 123: This is a message"));
        }

        [Test]
        public void Format_WithErrorCode_ReturnsCorrectFormattedString()
        {
            // Setup
            const int errorCode = 123;

            // Call
            string result = GridApiExceptionMessage.Format(errorCode);

            // Assert
            Assert.That(result, Is.EqualTo("GridApi returned error code 123"));
        }
    }
}