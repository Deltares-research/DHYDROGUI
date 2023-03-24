using System.Windows.Threading;
using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            XmlConfigurator.Configure();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            // Ensure shut down of background thread to ensure no COM erros are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}