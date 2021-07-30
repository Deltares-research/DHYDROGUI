using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileRowTests
    {
        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            // Arrange, Act
            var sut = new FouFileRow();
            
            Assert.IsNotNull(sut);
            Assert.IsNull(sut.Elp);
            Assert.IsNull(sut.Var);
        }

        [Test]
        public void ValuesCorrectlySet()
        {
            const string var = "var";
            const double tsrts = 1.0;
            const double sstop = 2.0;
            const int numcyc = 12;
            const int knfac = 42;
            const int v0plu = 3;
            int? layno = 56;
            const string elp = "elp";
            
            // Arrange, Act
            var sut = new FouFileRow
            {
                Var = @var,
                Tsrts = tsrts,
                Sstop = sstop,
                Numcyc = numcyc,
                Knfac = knfac,
                V0plu = v0plu,
                Layno = layno,
                Elp = elp
            };

            StringAssert.AreEqualIgnoringCase(@var, sut.Var);
            Assert.AreEqual(tsrts, sut.Tsrts);
            Assert.AreEqual(sstop, sut.Sstop);
            Assert.AreEqual(numcyc, sut.Numcyc);
            Assert.AreEqual(knfac, sut.Knfac);
            Assert.AreEqual(v0plu, sut.V0plu);
            Assert.AreEqual(layno, sut.Layno);
            StringAssert.AreEqualIgnoringCase(elp, sut.Elp);
        }
    }
}