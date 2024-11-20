using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            XmlConfigurator.Configure();
        }
    }
}