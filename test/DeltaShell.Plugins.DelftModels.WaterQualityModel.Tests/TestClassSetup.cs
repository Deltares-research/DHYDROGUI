using System.Windows.Threading;
using BasicModelInterface;
using DeltaShell.Dimr;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    /// <summary>
    /// Assembly Fixture to ensure shutdown of backgrounds threads used by
    /// Windows Form tests.
    /// </summary>
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DimrApiDataSet.FeedbackLevel = Level.All;
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