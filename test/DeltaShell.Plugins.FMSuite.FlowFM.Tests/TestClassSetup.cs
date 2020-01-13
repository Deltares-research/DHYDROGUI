using System;
using System.Windows.Threading;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    /// <summary>
    /// Assembly Fixture to ensure shutdown of backgrounds threads used by
    /// Windows Form tests.
    /// </summary>
    [SetUpFixture]
    public class TestClassSetup
    {
        [SetUp]
        public void Setup()
        {
            if (!UriParser.IsKnownScheme("pack"))
                new System.Windows.Application();
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [TearDown]
        public void TearDownWPFGuiAndWorkerThread()
        {
            // Ensure shut down of background thread to ensure no COM erros are thrown.
            // This should be done after all test fixtures have run.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}
