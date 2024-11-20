using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
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