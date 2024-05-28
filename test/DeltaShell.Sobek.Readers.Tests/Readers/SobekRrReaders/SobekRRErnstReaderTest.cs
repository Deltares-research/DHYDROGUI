using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRErnstReaderTest
    {
        //     ERNS id 'Ernst_1'   nm 'Ernst definition set1'   cvi 300 cvo 30 30 30 cvs 1 lv 0. 1.0 2.0  erns


        //id   =          alfa-factors identification 
        //nm  =          name 
        //cvi  =          Resistance value (in days) for infiltration from open water into unpaved area
        //cvo =         Resistance value (in days) for drainage from unpaved area to open water, for 3 layers 
        //cvs =          Resistance value (in days) for surface runoff
        //lv    =          three levels below surface (say lv1, lv2, lv3), separating the zones with various alfa-factors (or Ernst resistance values) for drainage.
        //                  a2 is used between surface level and lv1 m below the surface.
        //                  a3 is used between lv1 and lv2 m below the surface.
        //                  a4 is used between lv2 and lv3 m below the surface
        //                  a5 is used below lv3 m below surface.

        [Test]
        public void ReadErnstLineFromManual()
        {
            string line =
                @"ERNS id 'Ernst_1'   nm 'Ernst definition set1'   cvi 300 cvo 50 40 30 cvs 1 lv 0.1 1.0 2.0  erns";

            var ernstData = new SobekRRErnstReader().Parse(line).First();
            Assert.AreEqual("Ernst_1", ernstData.Id);
            Assert.AreEqual("Ernst definition set1", ernstData.Name);

            Assert.AreEqual(300.0, ernstData.ResistanceInfiltration);

            Assert.AreEqual(50.0, ernstData.ResistanceLayer1);
            Assert.AreEqual(40.0, ernstData.ResistanceLayer2);
            Assert.AreEqual(30.0, ernstData.ResistanceLayer3);

            Assert.AreEqual(1.0, ernstData.ResistanceSurface);

            Assert.AreEqual(0.1, ernstData.Level1);
            Assert.AreEqual(1.0, ernstData.Level2);
            Assert.AreEqual(2.0, ernstData.Level3);
        }

        [Test]
        public void ReadErnstLineFromTholen()
        {
            string line =
                @"ERNS id 'Drain1' nm 'Drain1' cvi 400 cvs 0.2 cvo 0 0 7.5 4000 lv 0 0 1.2 erns";

            var ernstData = new SobekRRErnstReader().Parse(line).First();
            Assert.AreEqual("Drain1", ernstData.Id);
            Assert.AreEqual("Drain1", ernstData.Name);

            Assert.AreEqual(400.0, ernstData.ResistanceInfiltration);

            Assert.AreEqual(0.0, ernstData.ResistanceLayer1);
            Assert.AreEqual(0.0, ernstData.ResistanceLayer2);
            Assert.AreEqual(7.5, ernstData.ResistanceLayer3);
            Assert.AreEqual(4000.0, ernstData.ResistanceLayer4);

            Assert.AreEqual(0.2, ernstData.ResistanceSurface);

            Assert.AreEqual(0.0, ernstData.Level1);
            Assert.AreEqual(0.0, ernstData.Level2);
            Assert.AreEqual(1.2, ernstData.Level3);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadErnstAlfaFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Unpaved.Alf");
            var lstErnst = new SobekRRErnstReader().Read(path);
            Assert.AreEqual(205, lstErnst.Count());
        }
        
        [Test]
        public void ReadAlternatingRecords()
        {
            var text =
                "ALFA id 'P06_P01_P01_ad_f005-lovl' nm 'P06_P01_P01_f005-lovl' af 2.5 0 0 0 .7 0.05 lv 0 0 0 alfa" + Environment.NewLine +
                "ERNS id 'P07_Drain1' nm 'P07_Drain1' cvi 164.72 cvo 3.09037 82.35832 500 500 lv 0.1 0.46995 10 cvs 1 erns" + Environment.NewLine +
                "ERNS id 'P07_Drain2' nm 'P07_Drain2' cvi 409.48 cvo 2 4.09481 500 500 lv 0.1 0.29944 10 cvs 1 erns" + Environment.NewLine +
                "ALFA id 'P06_P01_P01_ad_f005-uovl' nm 'P06_P01_P01_f005-uovl' af 2.5 0 0 0 .7 0.05 lv 0 0 0 alfa" + Environment.NewLine +
                "ALFA id 'P06_P01_P01_ad_f004O-bovl' nm 'P06_P01_P01_f004O-bovl' af 2.5 0 0 0 .7 0.05 lv 0 0 0 alfa" + Environment.NewLine +
                "ALFA id 'P06_P01_P01_ad_f008NO-aovl' nm 'P06_P01_P01_f008NO-aovl' af 2.5 0 0 0 .7 0.05 lv 0 0 0 alfa" + Environment.NewLine +
                Environment.NewLine +
                "ERNS id 'P07_Drain3' nm 'P07_Drain3' cvi 632.38 cvo 4.7134 25.29525 500 500 lv 0.1 0.51054 10 cvs 1 erns" + Environment.NewLine +
                "ERNS id 'P07_Drain4' nm 'P07_Drain4' cvi 601.5 cvo 2 6.01498 500 500 lv 0.1 0.37725 10 cvs 1 erns" + Environment.NewLine +
                "ERNS id 'P07_Drain5' nm 'P07_Drain5' cvi 7518.31 cvo 2.82113 75.18308 500 500 lv 0.1 0.56 10 cvs 1 erns";
            
            var lstErnst = new SobekRRErnstReader().Parse(text).ToList();

            Assert.AreEqual(5, lstErnst.Count);
        }
    }
}