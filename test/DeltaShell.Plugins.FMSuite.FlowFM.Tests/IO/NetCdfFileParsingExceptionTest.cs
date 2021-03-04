using System;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class NetCdfFileParsingExceptionTest
    {
        [Test]
        public void Constructor_WithArguments_ExpectedValues()
        {
            // Setup
            const string message = "ExceptionMessage";

            // Call
            var exception = new NetCdfFileParsingException(message);

            // Assert
            Assert.That(exception, Is.InstanceOf<Exception>());
            Assert.That(exception.Message, Is.EqualTo(message));
        }
    }
}