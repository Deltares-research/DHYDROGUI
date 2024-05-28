using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
        [TestFixture]
    public class SobekRRInfiltrationReaderTest
    {
  
        //id   =          infiltration identification
        //nm  =          name
        //ic    =          infiltration capacity of the soil, constant. (mm/hour)
        //                  Remark: no variable infiltration capacity implemented yet.

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadInfiltrationLineFromManual()
        {
            string line =
                @"INFC id 'Infcap_5'   nm 'inf_5mm'    ic 5.  infc";

            var InfiltrationData = new SobekRRInfiltrationReader().Parse(line).First();

            Assert.AreEqual("Infcap_5", InfiltrationData.Id);
            Assert.AreEqual("inf_5mm", InfiltrationData.Name);
            Assert.AreEqual(5.0, InfiltrationData.InfiltrationCapacity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadInfiltrationLineFromTholen()
        {
            string line =
                @"INFC id 'INF1' nm 'INF1' ic 99 infc";

            var InfiltrationData = new SobekRRInfiltrationReader().Parse(line).First();

            Assert.AreEqual("INF1", InfiltrationData.Id);
            Assert.AreEqual("INF1", InfiltrationData.Name);
            Assert.AreEqual(99.0, InfiltrationData.InfiltrationCapacity);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadInfiltrationFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Unpaved.inf");
            var lstInfiltration = new SobekRRInfiltrationReader().Read(path);
            Assert.AreEqual(1, lstInfiltration.Count());
        }

    }
}