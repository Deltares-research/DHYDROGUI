using System.Windows.Threading;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [TearDown]
        public void TearDownWpfGuiAndWorkerThread()
        {
            // Ensure shut down of background thread to ensure no COM errors are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}