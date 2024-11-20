using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRSacramentoReaderTest
    {
        [Test]
        public void ReadSacramentoRecordWithAreaAdjustmentFactor()
        {
            var record = "SACR id '2' nm '2' ar 1000000 ca '1' uh '2' op '2' ms '' aaf 2e-01 sacr";

            var sacramentoConcept = new SobekRRSacramentoReader().Parse(record).FirstOrDefault();
            Assert.IsNotNull(sacramentoConcept);
            Assert.AreEqual("2", sacramentoConcept.Id);
            Assert.AreEqual("2", sacramentoConcept.Name);
            Assert.AreEqual(1000000, sacramentoConcept.Area);
            Assert.AreEqual("1", sacramentoConcept.CapacityId);
            Assert.AreEqual("2", sacramentoConcept.UnitHydrographId);
            Assert.AreEqual("2", sacramentoConcept.OtherParamsId);
            Assert.AreEqual("", sacramentoConcept.MeteoStationId);
            Assert.AreEqual(0.2, sacramentoConcept.AreaAdjustmentFactor);
        }

        [Test]
        public void ReadSacramentoRecordWithoutAreaAdjustmentFactor()
        {
            var record = "SACR id '2' nm '2' ar 1000000 ca '1' uh '2' op '2' ms '' aaf '' sacr";

            var sacramentoConcept = new SobekRRSacramentoReader().Parse(record).FirstOrDefault();
            Assert.IsNotNull(sacramentoConcept);
            Assert.AreEqual("2", sacramentoConcept.Id);
            Assert.AreEqual("2", sacramentoConcept.Name);
            Assert.AreEqual(1000000, sacramentoConcept.Area);
            Assert.AreEqual("1", sacramentoConcept.CapacityId);
            Assert.AreEqual("2", sacramentoConcept.UnitHydrographId);
            Assert.AreEqual("2", sacramentoConcept.OtherParamsId);
            Assert.AreEqual("", sacramentoConcept.MeteoStationId);
            Assert.AreEqual(1, sacramentoConcept.AreaAdjustmentFactor);
        }
    }
}
