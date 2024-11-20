using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRUnpavedReaderTest
    {

      //id   =          node identification
      //na   =          number of areas (at the moment fixed at 16)
      //ar   =          area (in m2)  for all crops.  In the user interface either the total area can be specified, or the different areas per crop. In case the total area is specified, it is put at the first crop (grass).
      //ga   =          area for groundwater computations. Default = sum of crop areas. 
      //lv    =          surface level (=ground level) in m NAP 
      //co  =         computation option (1=Hellinga de Zeeuw (default), 2=Krayenhoff van de Leur, 3=Ernst)
      //rc   =          reservoir coefficient (for Krayenhoff van de Leur only); 
      //su   =          Indicator Scurve used 
      //                  su 0 = No Scurve used (Default)
      //                  su 1 ‘Scurve-id’ = Scurve used; Unpaved.Tbl contains  defniition of table with id ‘Scurve-id’.
      //sd   =          storage identification
      //ad  =          alfa-level identification (for Hellinga de Zeeuw drainage formula only)
      //ed  =          Ernst definition (for Ernst drainage formula only)
      //sp   =          seepage identification.
      //ic    =          infiltration identification
      //bt   =          soil type (from file BERGCOEF or BergCoef.Cap)
      //                  Indices >100 are from Bergcoef.Cap.
      //ig    =          initial groundwater level; constant, or as a table
      //                  ig 0 0.2 = initial groundwaterlevel as a constant, with value 0.2 m below the surface.
      //                  ig 1 'igtable1' = initial groundwater level as a table, with table identification igtable1. 
      //mg  =          maximum allowed groundwater level (in m NAP)
      //gl    =          initial depth of groundwater layer in meters (for salt computations)                      
      //ms  =          identification of the meteostation
      //is    =          initial salt concentration (mg/l) Default 100 mg/l

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineFromManual()
        {
            // This is indeed in the manual, but this floating point format ('6.' => 6) will not be recognised in SOBEK3. 
            //string line =
            //    @"UNPV id '1'  na 16 ar  1. 0. 3. 0. 0. 6. 0. 0. 0. 10. 11. 12. 13. 14. 15. 16.  lv 1.0 ga 110. co 1 su 0 sd 'ovhstor_1mm'  ad  'alfa_1'  sp 'seepage_1'   ic 'infcap_5'  bt 1 0  ig 0 0.5 mg 0.8 gl 1.5 ms 'meteostat1' is 100. unpv";
 
            string line = @"UNPV id '1'  na 16 ar  1 0.0 3 0 0 6 0 0 0 10 11 12 13 14 15 16  lv 1.0 ga 110 co 1 su 0 sd 'ovhstor_1mm'  ad  'alfa_1'  sp 'seepage_1'   ic 'infcap_5'  bt 1 0  ig 0 0.5 mg 0.8 gl 1.5 ms 'meteostat1' is 100 unpv";


            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.AreEqual("1", unpavedData.Id);
            Assert.AreEqual(new[]{1.0, 0.0, 3.0, 0.0, 0.0, 6.0, 0.0, 0.0, 0.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0}, unpavedData.CropAreas);
            Assert.AreEqual(1.0, unpavedData.SurfaceLevel);
            Assert.AreEqual(110.0, unpavedData.GroundWaterArea);
            Assert.AreEqual(SobekUnpavedComputationOption.HellingaDeZeeuw, unpavedData.ComputationOption);
            Assert.AreEqual(false, unpavedData.ScurveUsed);
            Assert.AreEqual("ovhstor_1mm", unpavedData.StorageId);
            Assert.AreEqual("alfa_1", unpavedData.AlfaLevelId);
            Assert.AreEqual("seepage_1", unpavedData.SeepageId);
            Assert.AreEqual(null, unpavedData.ErnstId);
            Assert.AreEqual("infcap_5", unpavedData.InfiltrationId);
            Assert.AreEqual(1, unpavedData.SoilType);
            Assert.AreEqual(0.5, unpavedData.InitialGroundwaterLevelConstant);
            Assert.AreEqual(0.8, unpavedData.MaximumGroundwaterLevel);
            Assert.AreEqual(1.5, unpavedData.InitialDepthGroundwaterLayer);
            Assert.AreEqual("meteostat1", unpavedData.MeteoStationId);
            Assert.AreEqual(100.0, unpavedData.InitialSaltConcentration);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineFromTholen()
        {
            string line =
                @"UNPV id 'upGFE820' na 16 ga 99826 ar 99392 0 0 0 196 238 0 0 0 0 0 0 0 0 0 0 su 0 '' lv -0.18 co 3 ad '' rc 0 ed 'Drain1' sp 'GFE820' ic 'INF1' sd 'STOR1' ig 0  1.07 gl 2 bt 115 is 2566.2 ms 'Station1' aaf 0.9 unpv";

            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.AreEqual("upGFE820", unpavedData.Id);
            Assert.AreEqual(new[] { 99392.0, 0.0, 0.0, 0.0, 196.0, 238.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, }, unpavedData.CropAreas);
            Assert.AreEqual(-0.18, unpavedData.SurfaceLevel);
            Assert.AreEqual(99826.0, unpavedData.GroundWaterArea);
            Assert.AreEqual(SobekUnpavedComputationOption.Ernst, unpavedData.ComputationOption);
            Assert.AreEqual(false, unpavedData.ScurveUsed);
            Assert.AreEqual("", unpavedData.AlfaLevelId);
            Assert.AreEqual(0.0, unpavedData.ReservoirCoefficient);
            Assert.AreEqual("Drain1", unpavedData.ErnstId);
            Assert.AreEqual("GFE820", unpavedData.SeepageId);
            Assert.AreEqual("INF1", unpavedData.InfiltrationId);
            Assert.AreEqual("STOR1", unpavedData.StorageId);
            Assert.AreEqual(1.07, unpavedData.InitialGroundwaterLevelConstant);
            Assert.AreEqual(2.0, unpavedData.InitialDepthGroundwaterLayer);
            Assert.AreEqual(115, unpavedData.SoilType);
            Assert.AreEqual(2566.2, unpavedData.InitialSaltConcentration);
            Assert.AreEqual("Station1", unpavedData.MeteoStationId);
            Assert.AreEqual(0.9, unpavedData.AreaAjustmentFactor);

            //not in line
            Assert.AreEqual(0.0, unpavedData.MaximumGroundwaterLevel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedInitialGroundWaterFromBoundary()
        {
            string line =
                @"UNPV id 'GFE1021' na  16 ar 258954 139268 0 33750 151041 190209 0 63288 0 0 0 0 0 0 0 0 ga 836510 lv -1.47 co 2 rc 1.5 sd '1' ad '1' ed '2' sp '4' ic '1' bt 7 ig 0 -999.99 su 0 ' '  gl 2 mg -1.47 ms 'GFE1021' aaf 1 is 0 unpv";

            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.IsTrue(unpavedData.InitialGroundwaterLevelFromBoundary);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineFromZBO()
        {
            string line =
                @"UNPV id 'upGFE1094' na 16 ar 40952 0 0 0 213 705 0 0 132 0 0 0 224341 0 0 0 ga 266510 lv -0.38 co 3 rc 0 sd 'STOR1' ad '' ed 'Drain1' sp 'SEEP1' ic 'INF1' bt 41 ig 0 0.86 su 0 '' gl 5 mg 0 ms 'GFE1094' is 0 unpv";

            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.AreEqual("upGFE1094", unpavedData.Id);
            Assert.AreEqual(new[] { 40952.0, 0.0, 0.0, 0.0, 213.0, 705.0, 0.0, 0.0, 132.0, 0.0, 0.0, 0.0, 224341.0, 0.0, 0.0, 0.0 }, unpavedData.CropAreas);
            Assert.AreEqual(-0.38, unpavedData.SurfaceLevel);
            Assert.AreEqual(266510.0, unpavedData.GroundWaterArea);
            Assert.AreEqual(SobekUnpavedComputationOption.Ernst, unpavedData.ComputationOption);
            Assert.AreEqual(false, unpavedData.ScurveUsed);
            Assert.AreEqual("", unpavedData.AlfaLevelId);
            Assert.AreEqual(0.0, unpavedData.ReservoirCoefficient);
            Assert.AreEqual("Drain1", unpavedData.ErnstId);
            Assert.AreEqual("SEEP1", unpavedData.SeepageId);
            Assert.AreEqual("INF1", unpavedData.InfiltrationId);
            Assert.AreEqual("STOR1", unpavedData.StorageId);
            Assert.AreEqual(0.86, unpavedData.InitialGroundwaterLevelConstant);
            Assert.AreEqual(5.0, unpavedData.InitialDepthGroundwaterLayer);
            Assert.AreEqual(41, unpavedData.SoilType);
            Assert.AreEqual(0.0, unpavedData.InitialSaltConcentration);
            Assert.AreEqual("GFE1094", unpavedData.MeteoStationId);
            Assert.AreEqual(1.0, unpavedData.AreaAjustmentFactor);
            Assert.AreEqual(0.0, unpavedData.MaximumGroundwaterLevel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLineWithSCurveTable()
        {
            string line =
                @"UNPV id 'upGFE820' na 16 ga 99826 ar 99392 0 0 0 196 238 0 0 0 0 0 0 0 0 0 0 su 1 'haha' lv -0.18 co 3 ad '' rc 0 ed 'Drain1' sp 'GFE820' ic 'INF1' sd 'STOR1' ig 0  1.07 gl 2 bt 115 is 2566.2 ms 'Station1' aaf 0.9 unpv";

            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.AreEqual(true, unpavedData.ScurveUsed);
            Assert.AreEqual("haha", unpavedData.ScurveTableName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineWithInitialGroundLayerTable()
        {
            string line =
                @"UNPV id '1' na  16 ar 1000000 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 lv 1.5 co 1 rc 1 sd '1' ad '1' ed '' sp '1' ic '1' bt 1 ig 1 'groundwater_level_table' su 1 '1' gl 5 mg 1.5 ms 'Station1' aaf 1 is 0 unpv";


            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();

            Assert.AreEqual("groundwater_level_table", unpavedData.InitialGroundwaterLevelTableId);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineWithLowerCaseTagInId()
        {
            string line =
                @"UNPV id 'unpv_duinen' na  16 ar 1000000 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 lv 1.5 co 1 rc 1 sd '1' ad '1' ed '' sp '1' ic '1' bt 1 ig 1 'groundwater_level_table' su 1 '1' gl 5 mg 1.5 ms 'Station1' aaf 1 is 0 unpv";
            
            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();
            Assert.AreEqual("unpv_duinen", unpavedData.Id);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedLineWithUpperCaseTagInId()
        {
            string line =
                @"UNPV id 'UNPV_1' na  16 ar 1000000 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 lv 1.5 co 1 rc 1 sd '1' ad '1' ed '' sp '1' ic '1' bt 1 ig 1 'groundwater_level_table' su 1 '1' gl 5 mg 1.5 ms 'Station1' aaf 1 is 0 unpv";

            var unpavedData = new SobekRRUnpavedReader().Parse(line).First();
            Assert.AreEqual("UNPV_1", unpavedData.Id);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Unpaved.3B");
            var lstUnpavedData = new SobekRRUnpavedReader().Read(path);
            Assert.AreEqual(329, lstUnpavedData.Count()); //should be 328. There is one dummy. Don't know how to filter yet.
        }
    }
}
