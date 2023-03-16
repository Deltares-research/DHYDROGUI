using BasicModelInterface;
using DeltaShell.Dimr;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
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