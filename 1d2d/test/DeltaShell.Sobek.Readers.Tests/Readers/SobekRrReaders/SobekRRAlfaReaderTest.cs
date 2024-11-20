using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRAlfaReaderTest
    {
        //ALFA id 'alfa_1'   nm 'set1 alfa factors'   af  5.0 0.9 0.7 0.6 0.3 0.03 lv 0. 1.0 2.0  alfa


        // id   =          alfa-factors identification 
        // nm  =          name 
        // af   =          alfa factors (say a1 to a6) for Hellinga-de Zeeuw formula (1/day). 
        //   a1 = alfa factor surface runoff 
        //   a2 = alfa factor drainage to open water, top soil layer
        //   a3 = alfa factor drainage to open water, second layer
        //   a4 = alfa factor drainage to open water, third layer
        //  a5 = alfa factor drainage to open water, last layer
        //  a6 = alfa factor infiltration 

        //lv = three levels below surface (say lv1, lv2, lv3), separating the zones with various alfa-factors (or Ernst resistance values) for drainage.
        //a2 is used between surface level and lv1 m below the surface.
        //a3 is used between lv1 and lv2 m below the surface.
        //a4 is used between lv2 and lv3 m below the surface
        //a5 is used below lv3 m below surface.

        [Test]
        public void ReadAlfaLineFromManual()
        {
            string line =
                @"ALFA id 'Alfa_1'   nm 'set1 Alfa factors'   af  5.0 0.9 0.7 0.6 0.3 0.2 lv 0.1 1.0 2.0  alfa";

            var alfaData = new SobekRRAlfaReader().Parse(line).First();
            Assert.AreEqual("Alfa_1", alfaData.Id);
            Assert.AreEqual("set1 Alfa factors", alfaData.Name);
            Assert.AreEqual(5.0, alfaData.FactorSurface);
            Assert.AreEqual(0.9, alfaData.FactorTopSoil);
            Assert.AreEqual(0.7, alfaData.FactorSecondLayer);
            Assert.AreEqual(0.6, alfaData.FactorThirdLayer);
            Assert.AreEqual(0.3, alfaData.FactorLastLayer);
            Assert.AreEqual(0.2, alfaData.FactorInfiltration);
            Assert.AreEqual(0.1, alfaData.Level1);
            Assert.AreEqual(1.0, alfaData.Level2);
            Assert.AreEqual(2.0, alfaData.Level3);
        }

        [Test]
        public void ReadAlfaWithScientificNotation()
        {
            string line =
                @"ALFA id 'Alfa_1'   nm 'set1 Alfa factors'   af  5.0 0.9 0.7 0.6 3.0e-04 0.2 lv 0.1 1.0 2.0  alfa";

            var alfaData = new SobekRRAlfaReader().Parse(line).First();
            Assert.AreEqual(0.0003, alfaData.FactorLastLayer, float.Epsilon);
        }

        [Test]
        public void ReadAlfaFileLandelijkSobekModel()
        {
            var path = TestHelper.GetTestDataDirectory() + @"..\DeltaShell.Plugins.ImportExport.Sobek.Tests\LSM1_0.lit\12\Unpaved.Alf";
            var lstErnst = new SobekRRAlfaReader().Read(path);

            Assert.IsFalse(lstErnst.Any(alfa => alfa.Id == null));
        }
    }
}