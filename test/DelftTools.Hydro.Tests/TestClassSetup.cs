using System.Windows.Threading;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
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