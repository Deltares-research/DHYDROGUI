using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRPavedReaderTest
    {

        //id   =          node identification
        //ar   =          area (in m2)  
        //lv   =          street level (m NAP) 
        //sd   =          storage identification
        //ss   =          sewer system type (0=mixed, 1=separated,  2=improved separated)
        //qc   =          capacity of sewer pump (m3/s)
        //                qc 0 0.2 0.0 = capacity as a constant, with value 0.2 (mixed/rainfall sewer) and 0.0 (DWA in separated or improved separated systems). So, first value is for mixed/rainfall sewer, second value for the dry weather flow (DWA) sewer.
        //                qc 1 'qctable1' = capacity as a table, with table identification qctable1. 
        //qo   =          1 1       =          both sewer pumps discharge to open water (=default)
        //                0 0       =          both sewer pumps discharge to boundary
        //                0 1       =          rainfall or mixed part of the sewer pumps to open water, 
        //                                     DWA-part (if separated) to boundary
        //                1 0       =          rainfall or mixed part of the sewer discharges to boundary, 
        //                                     DWA-part (if separated) to open water
        //                2 2       =          both sewer pumps discharge to WWTP
        //                2 1       =          rainfall or mixed part of the sewer pumps to open water, 
        //                                     DWA-part (if separated) to WWTP
        //                                     etc. 
        //                Note: first position of record is allocated to DWA sewer, second position is allocated to mixed/rainfall sewer; 0=to boundary, 1= to openwater, 2=to WWTP.  In all other keywords the order is just the other way around!!!!
        //so   =          sewer overflow level (first value for RWA/Mixed sewer, second value for DWA sewer). If missing, the surface level will be used. The level is used to verify whether sewer overflows can occur (no overflows can occur if the related boundary or open water level is higher)
        //si   =          sewer inflow from open water/boundary possible yes/no (1=yes,0=no); first value for RWA/Mixed sewer, second value for DWA sewer). Default value is 0, meaning that no external inflow is possible.
        //ms   =          identification of the meteostation by a character id
        //                If this id is missing in the rainfall file, data from the first station in the rainfall file will be used.
        //is   =          initial salt concentration (g/m3). Default 0.
        //np   =          number of people
        //dw   =          dry weather flow identification 
        //ro   =          runoff option
        //                0 = default,  no delay (=previous situation) 
        //                1 = using runoff delay factor (as in NWRW model)
        //                2 = using Qh relation  (not yet implemented)
        //     ru   =     runoff delay factor in (1/min) (as in NWRW model)  
        //                (only needed and used if option ro 1 is specified)
        //     qh   =     reference to Qh-relation 
        //                (only needed and used if option ro 2 is specified)

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadPavedLineFromManual()
        {
            string line =
                @"PAVE  id '1'  ar  100000. lv 1.0 sd 'stor_1mm' ss 0 qc 0 0.2 0 qo 1 1 so 0.5 0.5 si 0 0 ms 'meteostat1'  is 100. np 5000 dw  '1'  ro 1 ru 0.5 qh 'Qhrelation' pave";

            SobekRRPaved pavedData = null;
            void Call() => pavedData = new SobekRRPavedReader().Parse(line).First();

            var warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
            Assert.That(warnings, Has.Length.EqualTo(2));
            Assert.That(warnings[0], Is.EqualTo("Unsupported dry weather flow discharge target for paved data with id 1. Will be set to lateral source or boundary node."));
            Assert.That(warnings[1], Is.EqualTo("Unsupported mixed/rainfall discharge target for paved data with id 1. Will be set to lateral source or boundary node."));
            Assert.AreEqual("1",pavedData.Id);
            Assert.AreEqual(100000.0, pavedData.Area);
            Assert.AreEqual(1.0, pavedData.StreetLevel);
            Assert.AreEqual("stor_1mm", pavedData.StorageId);
            Assert.AreEqual(0.2, pavedData.CapacitySewerConstantRainfallInM3S);
            Assert.AreEqual(0.0, pavedData.CapacitySewerConstantDWAInM3S);
            Assert.AreEqual(SewerSystemType.Mixed, pavedData.SewerSystem);
            Assert.AreEqual(0.5, pavedData.SewerOverflowLevelRWAMixed);
            Assert.AreEqual(0.5, pavedData.SewerOverFlowLevelDWA);
            Assert.IsFalse(pavedData.SewerInflowRWAMixed);
            Assert.IsFalse(pavedData.SewerInflowDWA);
            Assert.AreEqual("meteostat1", pavedData.MeteoStationId);
            Assert.AreEqual(100.0, pavedData.InitialSaltConcentration);
            Assert.AreEqual(5000, pavedData.NumberOfPeople);
            Assert.AreEqual("1", pavedData.DryWeatherFlowId);
            Assert.AreEqual(SpillingOption.UsingCoefficient, pavedData.SpillingOption);
            Assert.AreEqual(0.5, pavedData.SpillingRunoffCoefficient);
            Assert.AreEqual("Qhrelation", pavedData.QHTableId);
            Assert.That(pavedData.DryWeatherFlowSewerPumpDischarge, Is.EqualTo(SewerDischargeType.BoundaryNode));
            Assert.That(pavedData.MixedAndOrRainfallSewerPumpDischarge, Is.EqualTo(SewerDischargeType.BoundaryNode));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadPavedLineFromTholen()
        {
            string line =
                @"PAVE id 'GS01' ar 5400 lv 9.99 ss 1 sd 'PAV1' qc 0 0 0.0315 qo 2 0 ms 'GFE1000' aaf 1 is 0 np 70 dw '1' ro 1 ru 2 qh 'haha' pave";

            var pavedData = new SobekRRPavedReader().Parse(line).First();

            Assert.AreEqual("GS01", pavedData.Id);
            Assert.AreEqual(5400.0, pavedData.Area);
            Assert.AreEqual(9.99, pavedData.StreetLevel);
            Assert.AreEqual(SewerSystemType.Separated, pavedData.SewerSystem);
            Assert.AreEqual("PAV1", pavedData.StorageId);
            Assert.AreEqual(0.0, pavedData.CapacitySewerConstantRainfallInM3S);
            Assert.AreEqual(0.0315, pavedData.CapacitySewerConstantDWAInM3S);
            Assert.AreEqual("GFE1000", pavedData.MeteoStationId);
            Assert.AreEqual(1.0, pavedData.AreaAjustmentFactor);
            Assert.AreEqual(0, pavedData.InitialSaltConcentration);
            Assert.AreEqual(70, pavedData.NumberOfPeople);
            Assert.AreEqual(2, pavedData.SpillingRunoffCoefficient);
            Assert.AreEqual(SpillingOption.UsingCoefficient, pavedData.SpillingOption);
            Assert.AreEqual("haha", pavedData.QHTableId);
            Assert.That(pavedData.DryWeatherFlowSewerPumpDischarge, Is.EqualTo(SewerDischargeType.WWTP));
            Assert.That(pavedData.MixedAndOrRainfallSewerPumpDischarge, Is.EqualTo(SewerDischargeType.BoundaryNode));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadPavedFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"Tholen.lit\29\Paved.3B");
            var lstPavedData = new SobekRRPavedReader().Read(path);
            Assert.AreEqual(48, lstPavedData.Count()); 
        }
    }
}
