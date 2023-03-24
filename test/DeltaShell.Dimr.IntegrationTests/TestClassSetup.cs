using BasicModelInterface;
using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Dimr.IntegrationTests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DimrApiDataSet.FeedbackLevel = Level.All;

            XmlConfigurator.Configure();
        }
    }
}