using System.Threading;
using System.Windows.Threading;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [SetUpFixture, Apartment(ApartmentState.STA)]
    public class TestClassSetup
    {
        [OneTimeTearDown]
        public void TearDownWpfGuiAndWorkerThread()
        {
            // Ensure shut down of background thread to ensure no COM errors are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}