using BasicModelInterface;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
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