using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRWasteWaterTreatmentPlantReaderTest
    {

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWasteWaterTreatmentPlantLineFromTholen()
        {
            string line = @"WWTP id 'ZRW5' tb 0  wwtp";

            var wwtpData = new SobekRRWasteWaterTreatmentPlantReader().Parse(line).First();
            Assert.AreEqual("ZRW5", wwtpData.Id);
            Assert.AreEqual(null, wwtpData.TableId);
        }

        [Test]
        public void ReadWasteWaterTreatmentPlantWithTagInId()
        {
            string line = @"WWTP id 'WWTP or west-wwtp' tb 0  wwtp";

            var wwtpData = new SobekRRWasteWaterTreatmentPlantReader().Parse(line).First();
            Assert.AreEqual("WWTP or west-wwtp", wwtpData.Id);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWWTPFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\WWTP.3B");
            var lstWWTP = new SobekRRWasteWaterTreatmentPlantReader().Read(path);
            Assert.AreEqual(2, lstWWTP.Count());
        }

        
    }
}
