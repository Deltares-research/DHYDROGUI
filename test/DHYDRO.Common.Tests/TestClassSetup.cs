using System.IO;
using log4net.Config;
using NUnit.Framework;

namespace DHYDRO.Common.Tests
{
    [SetUpFixture]
    public class TestClassSetup
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string configPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "log4net.config");
            XmlConfigurator.Configure(new FileInfo(configPath));
        }
    }
}