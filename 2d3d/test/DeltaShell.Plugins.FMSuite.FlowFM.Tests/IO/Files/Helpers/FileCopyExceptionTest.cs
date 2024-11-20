using System;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class FileCopyExceptionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string message = "message";
            var innerException = new Exception();

            // Call
            var exception = new FileCopyException(message,
                                                  innerException);

            // Assert
            Assert.That(exception.Message, Is.EqualTo(message),
                        "Expected an equal Message:");
            Assert.That(exception.InnerException, Is.SameAs(innerException),
                        "Expected the same InnerException:");
        }
    }
}