using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRHbvSoilReaderTest
    {
        [Test]
        public void ReadBasicSoilRecord()
        {
            var record = @"SOIL id '##1' nm 'SOIL1' be 3.5 fc 200 ef 0.75 soil";

            var soilData = new SobekRRHbvSoilReader().Parse(record).FirstOrDefault();
            Assert.IsNotNull(soilData);
            Assert.AreEqual("##1", soilData.Id);
            Assert.AreEqual(3.5, soilData.Beta);
            Assert.AreEqual(200.0, soilData.FieldCapacity);
            Assert.AreEqual(0.75, soilData.FieldCapacityThreshold);
        }

    }
}