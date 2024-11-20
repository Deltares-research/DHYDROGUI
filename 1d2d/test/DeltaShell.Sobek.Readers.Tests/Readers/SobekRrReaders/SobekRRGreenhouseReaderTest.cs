using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRGreenhouseReaderTest
    {
            //id   =          node identification
            //na   =          number or areas (default=10) 
            //ar   =          area (in m2) as a table with areas for all greenhouse classes (na  values)
            //as   =          greenhouse area connected to silo storage (m2)
            //sl    =          surface level in m NAP 
            //sd   =          storage definition on roofs
            //si    =          silo definition
            //ms  =          identification of the meteostation
            //is    =          initial salt concentration (mg/l)

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGreenhouseLineFromManual()
        {
            // This is indeed in the manual, but this floating point format ('6.' => 6) will not be recognised in SOBEK3. 
            //string line = @"GRHS id '1'  na 10 ar  1000. 0. 0. 3000.  0. 0. 0. 0. 0. 0. sl 1.0 as 0.1 sd 'roofstor_1mm' si 'silo_typ1' ms 'meteostat1'  is 50.0 grhs";
            
            string line = @"GRHS id '1'  na 10 ar  1000 0 0 3000.0  0 0 0 0 0 0 sl 1.0 as 0.1 sd 'roofstor_1mm' si 'silo_typ1' ms 'meteostat1'  is 50.0 grhs";

            var greenhouseData = new SobekRRGreenhouseReader().Parse(line).First();

            Assert.AreEqual("1", greenhouseData.Id);
            Assert.AreEqual(new[] {1000.0, 0.0, 0.0, 3000.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,}, greenhouseData.Areas);
            Assert.AreEqual(1.0, greenhouseData.SurfaceLevel);
            Assert.AreEqual(0.1, greenhouseData.SiloArea);
            Assert.AreEqual("roofstor_1mm", greenhouseData.StorageOnRoofsId);
            Assert.AreEqual("silo_typ1", greenhouseData.SiloId);
            Assert.AreEqual("meteostat1", greenhouseData.MeteoStationId);
            Assert.AreEqual(50.0, greenhouseData.InitialSaltConcentration);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGreenhouseLineFromCase16()
        {
            string line =
                @"GRHS id 'GRH01' na 10 ar 2500 3750 2250 1150 1050 1000 1000 1250 1350 1605.8 sl -0.3 as 0 sd '2' si '1' ms 'De Bilt' aaf 1 is 0 grhs";

            var greenhouseData = new SobekRRGreenhouseReader().Parse(line).First();

            Assert.AreEqual("GRH01", greenhouseData.Id);
            Assert.AreEqual(-0.3, greenhouseData.SurfaceLevel);
            Assert.AreEqual(0, greenhouseData.SiloArea);
            Assert.AreEqual("2", greenhouseData.StorageOnRoofsId);
            Assert.AreEqual("1", greenhouseData.SiloId);
            Assert.AreEqual("De Bilt", greenhouseData.MeteoStationId);
            Assert.AreEqual(new[] { 2500, 3750, 2250, 1150, 1050, 1000, 1000, 1250, 1350, 1605.8 }, greenhouseData.Areas);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGreenhouseLineFrom212()
        {
            string line =
                @"GRHS id '2' na 10  ar 10000 20000 30000 40000 50000 60000 70000 80000 90000 100000 sl 1.5 as 0 si '-1' sd '1' ms 'Station1' aaf 0.9 is  0 grhs";

            var greenhouseData = new SobekRRGreenhouseReader().Parse(line).First();
            Assert.AreEqual("2", greenhouseData.Id);
            Assert.AreEqual(new[] { 10000.0, 20000.0, 30000.0, 40000.0, 50000.0, 60000.0, 70000.0, 80000.0, 90000.0, 100000.0 }, greenhouseData.Areas);
            Assert.AreEqual(1.5, greenhouseData.SurfaceLevel);
            Assert.AreEqual(0, greenhouseData.SiloArea);
            Assert.AreEqual("-1", greenhouseData.SiloId);
            Assert.AreEqual("1", greenhouseData.StorageOnRoofsId);
            Assert.AreEqual("Station1", greenhouseData.MeteoStationId);
            Assert.AreEqual(0.9, greenhouseData.AreaAjustmentFactor);
            Assert.AreEqual(0.0, greenhouseData.InitialSaltConcentration);


        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGreenhouseFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Greenhouse\Greenhse.3B");
            var lstGreenhouse = new SobekRRGreenhouseReader().Read(path);
            Assert.AreEqual(3, lstGreenhouse.Count()); 
        }
    }
}
