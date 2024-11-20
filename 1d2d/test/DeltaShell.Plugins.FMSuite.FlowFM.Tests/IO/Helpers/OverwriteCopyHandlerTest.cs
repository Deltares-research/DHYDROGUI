using System;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Helpers.CopyHandlers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class OverwriteCopyHandlerTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var copyHandler = new OverwriteCopyHandler();

            // Assert
            Assert.That(copyHandler, Is.InstanceOf(typeof(ICopyHandler)));
        }

        [Test]
        public void Copy_ThrowsException_RethrowsAsFileCopyException()
        {
            // Setup
            var copyHandler = new OverwriteCopyHandler();

            // Call
            void Call() => copyHandler.Copy(null, null);

            // Assert
            var exception = Assert.Throws<FileCopyException>(Call, "Expected an exception to be thrown.");
            Assert.That(exception.Message, Is.EqualTo(exception.InnerException.Message),
                        "Expected the message to be equal to the inner message.");
            Assert.That(exception.InnerException, Is.InstanceOf<ArgumentNullException>());
        }
    }
}