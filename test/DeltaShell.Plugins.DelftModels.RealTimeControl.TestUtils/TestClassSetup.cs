using BasicModelInterface;
using DeltaShell.Dimr;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DimrApiDataSet.FeedbackLevel = Level.All;
        }
    }
}