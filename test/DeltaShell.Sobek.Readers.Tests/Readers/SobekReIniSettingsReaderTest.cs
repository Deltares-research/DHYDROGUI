using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekReIniSettingsReaderTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void Test()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\PARSEN.INI");

            var getSobekReIniSettings = SobekReIniSettingsReader.GetSobekReIniSettings(path);

            Assert.IsTrue(getSobekReIniSettings.Salt);
        }
    }
}