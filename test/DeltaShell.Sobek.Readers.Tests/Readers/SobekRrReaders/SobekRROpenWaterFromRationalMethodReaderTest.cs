using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekRROpenWaterFromRationalMethodReaderTest
    {
        // Lateral discharge on branch:
        // FLBR id '3' sc 0 lt 0 dc lt 6 ir 0.003 ms 'station 1' ii 0.005 ar 600000 flbr
        // or
        // FLBR id 'Intensity from Meteostation' sc 0 lt 0 dc lt 7 ir 0.003 ms 'meteostation' ii 0.002 ar 1000 flbr

        // sc and lt will already be imported by the lateral source importer

        // dc lt 6  = rational method with constant intensity
        // dc lt 7  = with intensity from the rainfall station
        // ir = constant intensity (mm/s)
        // ms = meteo-station
        // ii = seepage/infiltration intensity (mm/s)
        // ar = runoff area (m2)

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLateralSourceWithRatiolMethodAndConstantIntensityFromManual()
        {
            string line =
                @"FLBR id '3' sc 0 lt 0 dc lt 6 ir 0 ms 'station 1' ii 0.005 ar 600000 flbr";

            var sobekOpenWater = new SobekRROpenWaterFromRationalMethodReader().Parse(line).First();
            Assert.AreEqual("3", sobekOpenWater.Id);

            //sc and lt will be read by the lateral source reader

            Assert.AreEqual(0.0, sobekOpenWater.ConstantIntensity);
            Assert.AreEqual("station 1", sobekOpenWater.MeteoStationId);
            Assert.AreEqual(0.005, sobekOpenWater.InfiltrationItensity);
            Assert.AreEqual(600000, sobekOpenWater.Area);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLateralSourceWithRatiolMethodAndMeteoStationFromManual()
        {
            string line =
                @"FLBR id 'Intensity from Meteostation' sc 0 lt 0 dc lt 7 ir 0.003 ms 'meteostation' ii 0.002 ar 1000 flbr";

            var sobekOpenWater = new SobekRROpenWaterFromRationalMethodReader().Parse(line).First();
            Assert.AreEqual("Intensity from Meteostation", sobekOpenWater.Id);

            //sc and lt will be read by the lateral source reader
                
            Assert.AreEqual(0.003, sobekOpenWater.ConstantIntensity);
            Assert.AreEqual("meteostation", sobekOpenWater.MeteoStationId);
            Assert.AreEqual(0.002, sobekOpenWater.InfiltrationItensity);
            Assert.AreEqual(1000, sobekOpenWater.Area);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLaterSourcesWithRationalMethodFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"Tholen.lit\29\Lateral.DAT");
            var lstOpenWater = new SobekRROpenWaterFromRationalMethodReader().Read(path);
            Assert.AreEqual(16, lstOpenWater.Count()); 
        }
    }
}
