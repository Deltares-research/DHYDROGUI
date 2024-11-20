using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRUnitHydrographReaderTest
    {
        [Test]
        public void ReadBasicHydrographRecord()
        {
            var record =
                @"UNIH id 'UnitHydrograph' uh 1 2 3 4.0 5 6 7 8 9 10.0 11.0 12 13 14 15 16 17 18 19 2e+01 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 dt 1 unih";

            var unitHydrograph = new SobekRRUnitHydrographReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(unitHydrograph);
            Assert.AreEqual("UnitHydrograph", unitHydrograph.Id);

            var array = Enumerable.Range(1, 36).Select(i => (double) i);
            Assert.AreEqual(array.ToList(), unitHydrograph.Values);

            Assert.AreEqual(1.0, unitHydrograph.Stepsize);
        }

        [Test]
        public void ReadBasicHydrographRecordUhAndDtReversed()
        {
            var record =
                @"UNIH id 'UnitHydrograph' dt 10 uh 1 2 3 4.0 5 6 7 8 9 10.0 11.0 12 13 14 15 16 17 18 19 2e+01 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 unih";

            var unitHydrograph = new SobekRRUnitHydrographReader().Parse(record).FirstOrDefault();
            Assert.AreEqual(10.0, unitHydrograph.Stepsize);
            Assert.AreEqual(36.0, unitHydrograph.Values[35]);
        }
    }
}