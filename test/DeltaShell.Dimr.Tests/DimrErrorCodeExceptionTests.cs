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

            Assert.AreEqual(dimrErrorCodeException.Message,
                            $"During {status.ToString().ToLower()} the model run something went wrong. Error code {errorCode} sent by the computational core.");
        }
    }
}