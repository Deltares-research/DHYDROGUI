using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    public class SobekRRStorageReaderTest
    {
        // general info
        //id   =          storage identification
        //nm   =          name (optional)

        // for unpaved data:
        //ml   =          maximum storage on land (mm). Default 1 mm.
        //il   =          initial storage on land (mm). Default 0.

        // for paved data:
        //ms   =          maximum storage on streets (mm). Default 1 mm.
        //is   =          initial storage on streets (mm). Default 0.
        //mr   =          maximum storage sewer (mm). Default 7 mm.    
        //ir   =          initial storage in sewer (mm). Default 0.
        //                For mr and ir different sewer systems are distinghuished (mixed systems, separated systems, improved separated system).
        //                The first value is for mixed and rainfall sewer, the second value for DWA sewer.

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedStorageLineFromManual()
        {
            string line =
                @"STDF id 'ovhstor_1mm'   nm '1 mm storage'    ml 1.1 il 0.1 stdf";

            var StorageData = Enumerable.First<SobekRRStorage>(new SobekRRStorageReader().Parse(line));

            Assert.AreEqual("ovhstor_1mm", StorageData.Id);
            Assert.AreEqual("1 mm storage", StorageData.Name);
            Assert.AreEqual(1.1, StorageData.MaxLandStorage);
            Assert.AreEqual(0.1, StorageData.InitialLandStorage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadPavedStorageLineFromManual()
        {
            string line =
                @"STDF id 'stor_1mm'  nm 'storage 1mm'   ms 1. is 0. mr 7.0  0.0 ir 0.0  0.0 stdf";

            var StorageData = Enumerable.First<SobekRRStorage>(new SobekRRStorageReader().Parse(line));

            Assert.AreEqual("stor_1mm", StorageData.Id);
            Assert.AreEqual("storage 1mm", StorageData.Name);
            Assert.AreEqual(1.0, StorageData.MaxStreetStorage);
            Assert.AreEqual(0.0, StorageData.InitialStreetStorage);
            Assert.AreEqual(7.0, StorageData.MaxSewerStorageMixedRainfall);
            Assert.AreEqual(0.0, StorageData.MaxSewerStorageDWA);
            Assert.AreEqual(0.0, StorageData.InitialSewerStorageMixedRainfall);
            Assert.AreEqual(0.0, StorageData.InitialSewerStorageDWA);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadUnpavedStorageLineFromTholen()
        {
            string line =
                @"STDF id 'STOR1' nm 'STOR1' ml 10 il 0 stdf";

            var StorageData = Enumerable.First<SobekRRStorage>(new SobekRRStorageReader().Parse(line));

            Assert.AreEqual("STOR1", StorageData.Id);
            Assert.AreEqual("STOR1", StorageData.Name);
            Assert.AreEqual(10.0, StorageData.MaxLandStorage);
            Assert.AreEqual(0.0, StorageData.InitialLandStorage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadPavedStorageLine()
        {
            string line =
                @"STDF id 'PAV1' nm 'PAV1' ms 1.1 is 2.2 mr 3.3 4.4 ir 5.5 6.6 stdf";

            var StorageData = Enumerable.First<SobekRRStorage>(new SobekRRStorageReader().Parse(line));

            Assert.AreEqual("PAV1", StorageData.Id);
            Assert.AreEqual("PAV1", StorageData.Name);
            Assert.AreEqual(1.1, StorageData.MaxStreetStorage);
            Assert.AreEqual(2.2, StorageData.InitialStreetStorage);
            Assert.AreEqual(3.3, StorageData.MaxSewerStorageMixedRainfall);
            Assert.AreEqual(4.4, StorageData.MaxSewerStorageDWA);
            Assert.AreEqual(5.5, StorageData.InitialSewerStorageMixedRainfall);
            Assert.AreEqual(6.6, StorageData.InitialSewerStorageDWA);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStorageFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Unpaved.sto");
            var lstStorage = new SobekRRStorageReader().Read(path);
            Assert.AreEqual(1, lstStorage.Count());
        }
    }
}