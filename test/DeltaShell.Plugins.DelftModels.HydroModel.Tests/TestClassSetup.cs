using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [SetUpFixture, Apartment(ApartmentState.STA)]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void RetrieveApplicationOnceInOrderToCorrectlyInstantiateResourceDictionaries()
        {
            // Ensure calls to ...
            //
            //   new Uri("pack://application:,,,/<path>");
            //
            // ... don't result in exceptions like ...
            //
            //   Invalid URI: Invalid port specified
            //
            // ... due to the fact that the application is not fully initialized yet.
            var application = Application.Current;
        }

        [OneTimeTearDown]
        public void TearDownWpfGuiAndWorkerThread()
        {
            // Ensure shut down of background thread to ensure no COM errors are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}