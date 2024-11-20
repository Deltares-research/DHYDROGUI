using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRHbvSnowReaderTest
    {
        [Test]
        public void ReadBasicSnowRecord()
        {
            var record =
                @"SNOW id '1' nm 'RRNF Snowmelt parameters' mc 4.0 sft 0.0 smt 0.0 tac 6.0 fe 0.005 fwf 0.1 snow";

            var snowRecord = new SobekRRHbvSnowReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(snowRecord);
            Assert.AreEqual("1", snowRecord.Id);
            Assert.AreEqual(4.0, snowRecord.SnowMeltingConstant);
            Assert.AreEqual(0.0, snowRecord.SnowFallTemperature);
            Assert.AreEqual(0.0, snowRecord.SnowMeltTemperature);
            Assert.AreEqual(6.0, snowRecord.TemperatureAltitudeConstant);
            Assert.AreEqual(0.005, snowRecord.FreezingEfficiency);
            Assert.AreEqual(0.1, snowRecord.FreeWaterFraction);
        }
    }
}