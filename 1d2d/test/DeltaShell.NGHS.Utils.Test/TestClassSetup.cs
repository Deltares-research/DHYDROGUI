using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test
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