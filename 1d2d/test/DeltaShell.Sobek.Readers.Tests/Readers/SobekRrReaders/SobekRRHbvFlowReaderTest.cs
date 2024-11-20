using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRHbvFlowReaderTest
    {
        [Test]
        public void ReadBasicFlowRecord()
        {
            var record = @"FLOW id '1' nm 'RRNF reservoir flow parameters' kb 0.005 ki 0.1 kq 0.3 qt 20.0 mp 0.6 flow";

            var flowRecord = new SobekRRHbvFlowReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(flowRecord);
            Assert.AreEqual("1", flowRecord.Id);
            Assert.AreEqual(0.005, flowRecord.BaseFlowReservoirConstant);
            Assert.AreEqual(0.1, flowRecord.InterflowReservoirConstant);
            Assert.AreEqual(0.3, flowRecord.QuickFlowReservoirConstant);
            Assert.AreEqual(20.0, flowRecord.UpperZoneThreshold);
            Assert.AreEqual(0.6, flowRecord.MaximumPercolation);
        }
    }
}