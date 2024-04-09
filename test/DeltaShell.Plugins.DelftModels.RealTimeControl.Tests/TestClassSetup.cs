using BasicModelInterface;
using DeltaShell.Dimr;
using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            DimrLogging.FeedbackLevel = Level.All;
            
            XmlConfigurator.Configure();
        }
    }
}