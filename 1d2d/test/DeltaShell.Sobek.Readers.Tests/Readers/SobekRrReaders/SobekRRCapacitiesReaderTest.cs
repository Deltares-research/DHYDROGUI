using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRCapacitiesReaderTest
    {
        [Test]
        public void ReadBasicCapacityRecord()
        {
            var record = "CAPS id 'Capacities&Contents' uztwm 10 uztwc 20 uzfwm 3E+1 uzfwc 40 lztwm 50 lztwc 60 " +
                         " lzfsm 70 lzfsc 80 lzfpm 90 lzfpc 100 uzk .1 lzsk 2e-01 lzpk 0.3 caps";

            var capacity = new SobekRRCapacitiesReader().Parse(record).FirstOrDefault();
            
            Assert.IsNotNull(capacity);
            Assert.AreEqual("Capacities&Contents", capacity.Id);
            Assert.AreEqual(10, capacity.UpperZoneTensionWaterStorageCapacity);
            Assert.AreEqual(20, capacity.UpperZoneTensionWaterInitialContent);
            Assert.AreEqual(30, capacity.UpperZoneFreeWaterStorageCapacity);
            Assert.AreEqual(40, capacity.UpperZoneFreeWaterInitialContent);
            Assert.AreEqual(50, capacity.LowerZoneTensionWaterStorageCapacity);
            Assert.AreEqual(60, capacity.LowerZoneTensionWaterInitialContent);
            Assert.AreEqual(70, capacity.LowerZoneSupplementalFreeWaterStorageCapacity);
            Assert.AreEqual(80, capacity.LowerZoneSupplementalFreeWaterInitialContent);
            Assert.AreEqual(90, capacity.LowerZonePrimaryFreeWaterStorageCapacity);
            Assert.AreEqual(100, capacity.LowerZonePrimaryFreeWaterInitialContent);
            Assert.AreEqual(0.1, capacity.UpperZoneFreeWaterDrainageRate);
            Assert.AreEqual(0.2, capacity.LowerZoneSupplementalFreeWaterDrainageRate);
            Assert.AreEqual(0.3, capacity.LowerZonePrimaryFreeWaterDrainageRate);
        }
        
        [Test]
        public void ReadCapacityRecordWithLabelInIdString()
        {
            var record = "CAPS id 'Capacity id with uzk eee' uztwm 50 uztwc 50 uzfwm 150 uzfwc 100 lztwm 150 lztwc" +
                         "150 lzfsm 200 lzfsc 100 lzfpm 150 lzfpc 150 uzk .08 lzsk .05 lzpk .003 caps";

            var capacity = new SobekRRCapacitiesReader().Parse(record).FirstOrDefault();
            
            Assert.IsNotNull(capacity);
            Assert.AreEqual("Capacity id with uzk eee", capacity.Id);
            Assert.AreEqual(0.08, capacity.UpperZoneFreeWaterDrainageRate);
        }
    }
}
