using log4net.Config;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests
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