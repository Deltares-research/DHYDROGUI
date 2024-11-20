using System;
using DelftTools.Shell.Core.Workflow;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrErrorCodeExceptionTests
    {
        [Test]
        [TestCase(ActivityStatus.Executing, 1)]
        [TestCase(ActivityStatus.Initializing, 5)]
        public void Constructor_PropertiesShouldBeSetCorrectly(ActivityStatus status, int errorCode)
        {
            var dimrErrorCodeException = new DimrErrorCodeException(status, errorCode);

            Assert.IsInstanceOf<Exception>(dimrErrorCodeException);
            Assert.AreEqual($"During {status.ToString().ToLower()} the model something went wrong. Error code {errorCode} has been detected. Please inspect your diagnostic files.", dimrErrorCodeException.Message);
        }
    }
}