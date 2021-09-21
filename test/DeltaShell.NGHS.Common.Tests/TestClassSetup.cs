using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.Common.Tests
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

            // Set static Map.CoordinateSystemFactory so coordinate transformation can be done
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
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