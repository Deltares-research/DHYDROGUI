using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRHbvReaderTest
    {
        [Test]
        public void ReadHbvRecord()
        {
            var record =
                @"HBV id '2' nm '2' ar 1000000 sl 20 snow '##1' soil '##1' flow '##1' hini '##1' ms 'De Bilt' aaf 1 ts 'De Bilt' hbv";

            var hbvData = new SobekRRHbvReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(hbvData);
            Assert.AreEqual("2", hbvData.Id);
            Assert.AreEqual("2", hbvData.Name);
            Assert.AreEqual(1e+06, hbvData.Area, 0.1);
            Assert.AreEqual(20.0, hbvData.SurfaceLevel);
            Assert.AreEqual("##1", hbvData.SnowId);
            Assert.AreEqual("##1", hbvData.SoilId);
            Assert.AreEqual("##1", hbvData.FlowId);
            Assert.AreEqual("##1", hbvData.HiniId);
            Assert.AreEqual("De Bilt", hbvData.MeteoStationId);
            Assert.AreEqual("De Bilt", hbvData.TemperatureStationId);
            Assert.AreEqual(1, hbvData.AreaAdjustmentFactor);
        }
    }
}