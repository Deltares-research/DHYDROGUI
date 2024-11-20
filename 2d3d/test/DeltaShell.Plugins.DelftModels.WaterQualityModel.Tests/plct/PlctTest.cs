using System.IO;
using DelftTools.TestUtils;
using Deltares.Infrastructure.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.plct
{
    [TestFixture]
    public class PlctTest
    {
        private string plctDir;
        private string plctBinDir;
        private string plctEcoIniFile;
        private string plctExeFile;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string pluginsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "plugins");

            plctDir = Path.Combine(pluginsDir, "DeltaShell.Plugins.DelftModels.WaterQualityModel", "plct");
            plctBinDir = Path.Combine(plctDir, "bin");
            plctEcoIniFile = Path.Combine(plctDir, "plct_eco.ini");
            plctExeFile = Path.Combine(plctBinDir, "plct.exe");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void PlctDeploymentTest()
        {
            Assert.That(plctDir, Does.Exist);
            Assert.That(plctBinDir, Does.Exist);
            Assert.That(plctEcoIniFile, Does.Exist);
            Assert.That(plctExeFile, Does.Exist);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void PlctEcoIniFileTest()
        {
            IniData iniData = ParsePlctEcoIniFile(plctEcoIniFile);

            IniSection generalSection = iniData.FindSection("General");
            IniSection systemSection = iniData.FindSection("System");

            Assert.That(generalSection, Is.Not.Null);
            Assert.That(generalSection.GetPropertyValue("configuration"), Is.EqualTo("ECO"));
            Assert.That(generalSection.GetPropertyValue("application_type"), Is.EqualTo("Delft3D"));

            Assert.That(systemSection, Is.Not.Null);
            Assert.That(systemSection.GetPropertyValue("nefis_data_file"), Is.Not.Null.And.Not.Empty);
            Assert.That(systemSection.GetPropertyValue("nefis_definition_file"), Is.Not.Null.And.Not.Empty);
            Assert.That(systemSection.GetPropertyValue("algaedb"), Is.Not.Null.And.Not.Empty);
            Assert.That(systemSection.GetPropertyValue("defaulthome"), Is.Not.Null.And.Not.Empty);

            Assert.That(Path.Combine(plctBinDir, systemSection.GetPropertyValue("nefis_data_file")), Does.Exist);
            Assert.That(Path.Combine(plctBinDir, systemSection.GetPropertyValue("nefis_definition_file")), Does.Exist);
            Assert.That(Path.Combine(plctBinDir, systemSection.GetPropertyValue("algaedb")), Does.Exist);
            Assert.That(Path.Combine(plctBinDir, systemSection.GetPropertyValue("defaulthome")), Does.Exist);
        }

        private static IniData ParsePlctEcoIniFile(string path)
        {
            string ini = File.ReadAllText(path);
            
            return new IniParser().Parse(ini);
        }
    }
}