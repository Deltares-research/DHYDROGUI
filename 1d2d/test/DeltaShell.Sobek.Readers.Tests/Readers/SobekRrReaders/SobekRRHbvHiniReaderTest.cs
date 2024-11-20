using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRHbvHiniReaderTest
    {
        [Test]
        public void ReadBasicRecord()
        {
            var record = @"HINI id 'hini 1' nm 'RRNF initialisation parameters' ds 0.0 fw 3.0e+01 sm 0.6 uz 0.0 lz 180.0 hini";

            var hiniData = new SobekRRHbvHiniReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(hiniData);
            Assert.AreEqual("hini 1", hiniData.Id);
            Assert.AreEqual(0.0, hiniData.InitialDrySnowContent);
            Assert.AreEqual(30.0, hiniData.InitialFreeWaterContent);
            Assert.AreEqual(0.6, hiniData.InitialSoilMoistureContents);
            Assert.AreEqual(0.0, hiniData.InitialUpperZoneContent);
            Assert.AreEqual(180.0, hiniData.InitialLowerZoneContent);
        }
    }
}