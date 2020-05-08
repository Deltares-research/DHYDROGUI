using System.Windows.Threading;
using NUnit.Framework;

namespace DeltaShell.Plugins.NGHS.IntegrationTests
{
    /// <summary>
    /// Assembly Fixture to ensure shutdown of backgrounds threads used by
    /// Windows Form tests.
    /// </summary>
    [SetUpFixture]
    public class TestClassSetup
    {
        [TearDown]
        public void TearDownWPFGuiAndWorkerThread()
        {
            // Ensure shut down of background thread to ensure no COM erros are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}