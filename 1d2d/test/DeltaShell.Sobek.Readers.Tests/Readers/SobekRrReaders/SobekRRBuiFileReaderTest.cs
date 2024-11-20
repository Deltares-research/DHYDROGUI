using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekRRBuiFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBuiFile()
        {
            string buiFilePath = TestHelper.GetTestDataDirectory() + @"\Meteo\STNBUI09.BUI";
            var reader = new SobekRRBuiFileReader();
            var measurements = reader.ReadMeasurementData(buiFilePath).ToList();

            Assert.AreEqual(25, measurements.Count);
            Assert.AreEqual(1995, measurements.First().TimeOfMeasurement.Year);
            Assert.AreEqual(2.7, measurements[1].MeasuredValues[0], 1e-8);
        }



        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBuiFile1()
        {
            var t = DateTime.Now;
            string buiFilePath = TestHelper.GetTestDataDirectory() + @"\RD-02X.bui";
            var reader = new SobekRRBuiFileReader();
            var measurements = reader.ReadMeasurementData(buiFilePath).ToList();

            var dt = DateTime.Now.Subtract(t).TotalMilliseconds;

            Assert.AreEqual(70176, measurements.Count);
            Assert.AreEqual(1955, measurements.First().TimeOfMeasurement.Year);
            Assert.AreEqual(1956, measurements[70175].TimeOfMeasurement.Year);
            Assert.AreEqual(0.0, measurements[1].MeasuredValues[0], 1e-8);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBuiFileTholen()
        {
            string buiFilePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"FIXED\THOL2010.BUI");
            var reader = new SobekRRBuiFileReader();

            reader.ReadMeasurementData(buiFilePath).ToList();
            Assert.AreEqual(274, reader.StationNames.Count);
            Assert.AreEqual("GFE978", reader.StationNames[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadMultipleStationsBuiFileAndCheckData()
        {
            string buiFilePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"FIXED\Test2Stations4TimeSteps.BUI");
            var reader = new SobekRRBuiFileReader();
            var measurements = reader.ReadMeasurementData(buiFilePath).ToList();

            Assert.AreEqual(2, reader.StationNames.Count);
            Assert.AreEqual("Aap", reader.StationNames[0]);
            Assert.AreEqual("Noot", reader.StationNames[1]);

            Assert.AreEqual(4, measurements.Count);

            // Aap:
            Assert.AreEqual(1.1, measurements[0].MeasuredValues[0]);
            Assert.AreEqual(2.2, measurements[1].MeasuredValues[0]);
            Assert.AreEqual(3.3, measurements[2].MeasuredValues[0]);
            Assert.AreEqual(4.4, measurements[3].MeasuredValues[0]);

            // Noot:
            Assert.AreEqual(11.11, measurements[0].MeasuredValues[1]);
            Assert.AreEqual(22.22, measurements[1].MeasuredValues[1]);
            Assert.AreEqual(33.33, measurements[2].MeasuredValues[1]);
            Assert.AreEqual(44.44, measurements[3].MeasuredValues[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadLargeBuiFileMemoryBenchmark()
        {
            string buiFilePath = TestHelper.GetTestFilePath(@"meteo\LMW08t11.BUI");
            var reader = new SobekRRBuiFileReader();

            long startMemory = GC.GetTotalMemory(true);

            List<MeteoStationsMeasurement> measurements = null;
            TestHelper.AssertIsFasterThan(25000, () =>
                {
                    measurements = reader.ReadMeasurementData(buiFilePath).ToList();
                });
            
            Assert.AreEqual(35040, measurements.Count, "number of measurements read");
            Assert.AreEqual(410, reader.StationNames.Count, "number of meteo stations");

            long consumedMemory = GC.GetTotalMemory(true) - startMemory;
            Console.WriteLine("Consumed memory: " + consumedMemory);
            Assert.LessOrEqual(consumedMemory, 130000000L , "Memory consumption should not be higher than 130 MB for a text file of size 48MB.");
        }
    }
}