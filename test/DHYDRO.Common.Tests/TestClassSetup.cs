using log4net.Config;
using NUnit.Framework;

namespace DHYDRO.Common.Tests
{
    /// <summary>
    /// Assembly fixture to enable logging and to ensure shutdown of backgrounds threads used by Windows Form tests.
    /// </summary>
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