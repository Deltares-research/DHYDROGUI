using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekGeneralStructureReaderTest
    {
        private SobekGeneralStructureReader reader;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            reader = new SobekGeneralStructureReader(SobekType.Unknown);
        }
        [Test]
        public void Read()
        {
            Assert.AreEqual(2, reader.Type);

            string source =
                @"w1 1.1 wl 1.2 ws 1.3 wr 1.4 w2 1.5 z1 2.1 zl 2.2 zs 2.3 zr 2.4 z2 2.5 gh 5"+
                @"pg 0.5 pd 0.3 pi 0.52 pr 0.9 pc 0.8 ng 0.3 " +
                @"nd 0.12 nf 0.6 nr 0.5 nc 0.9";

            var generalStructure = (SobekGeneralStructure) reader.GetStructure(source);

            Assert.AreEqual(1.1f, generalStructure.WidthLeftSideOfStructure);
            Assert.AreEqual(1.2f, generalStructure.WidthStructureLeftSide);
            Assert.AreEqual(1.3f, generalStructure.WidthStructureCentre);
            Assert.AreEqual(1.4f, generalStructure.WidthStructureRightSide);
            Assert.AreEqual(1.5f, generalStructure.WidthRightSideOfStructure);

            Assert.AreEqual(2.1f, generalStructure.BedLevelLeftSideOfStructure);
            Assert.AreEqual(2.2f, generalStructure.BedLevelLeftSideStructure);
            Assert.AreEqual(2.3f, generalStructure.BedLevelStructureCentre);
            Assert.AreEqual(2.4f, generalStructure.BedLevelRightSideStructure);
            Assert.AreEqual(2.5f, generalStructure.BedLevelRightSideOfStructure);

            Assert.AreEqual(5.0f, generalStructure.GateHeight);
            
            Assert.AreEqual(0.5f, generalStructure.PositiveFreeGateFlow);
            Assert.AreEqual(0.3f, generalStructure.PositiveDrownedGateFlow);
            Assert.AreEqual(0.52f, generalStructure.PositiveFreeWeirFlow);
            Assert.AreEqual(0.9f, generalStructure.PositiveDrownedWeirFlow);
            Assert.AreEqual(0.8f,generalStructure.PositiveContractionCoefficient);

            Assert.AreEqual(0.3f, generalStructure.NegativeFreeGateFlow);
            Assert.AreEqual(0.12f, generalStructure.NegativeDrownedGateFlow);
            Assert.AreEqual(0.6f, generalStructure.NegativeFreeWeirFlow);
            Assert.AreEqual(0.5f, generalStructure.NegativeDrownedWeirFlow);
            Assert.AreEqual(0.9f, generalStructure.NegativeContractionCoefficient);
            Assert.IsNull(generalStructure.ExtraResistance);
        }


        [Test]
        public void ReadWithExtraResistance()
        {
            
            string source =
                @"w1 1.1 wl 1.2 ws 1.3 wr 1.4 w2 1.5 z1 2.1 zl 2.2 zs 2.3 zr 2.4 z2 2.5 gh 5 " +
                @"pg 0.5 pd 0.3 pi 0.52 pr 0.9 pc 0.8 ng 0.3 " +
                @"nd 0.12 nf 0.6 nr 0.5 nc 0.9 er 2.2";

            var generalStructure = (SobekGeneralStructure)reader.GetStructure(source);
            Assert.AreEqual(2.2f,generalStructure.ExtraResistance);

        }
        [Test]
        public void ReadFromOtherFormat()
        {
            string source =
                @"w1 58.5 wl 58.5 ws 58.5 wr 58.5 w2 58.5 z1 -8 zl -8 zs -5.54 zr -8 z2 -8 gh 0 "+
                @"pg 1 pd 1 pi 1 pr 1 pc 0.83 ng 1 "+
                @"nd 1 nf 1 nr 1 nc 0.83 er 0";
                            
                
            var generalStructure = (SobekGeneralStructure)reader.GetStructure(source);
        }

        [Test]
        public void ReadSobekREGeneralStructure()
        {
            string source =
                @"STDS id '285' nm 'General' ty 2 cl 9.9999e+009 cs 0 cw 9.9999e+009 po 1 ps 0.82 pt pr no 1 ns 0.82 nt nr sw 9.9999e+009 ni 31073 ph 10 pw 3 pp 0.01 pa 0.1 nh 10 nw 3 np 0.01 na 0.1 w1 25 wl 25 ws 15 wr 25 w2 25 z1 -3 zl -3 zs -1.5 zr -3 z2 -3 gh 2 pg 0.5 pd 0.5 pi 0.5 pr 0.5 pc 0.5 ng 1 nd 1 nf 1 nr 1 nc 1 dn 0 rt cr 0 9.9999e+009 9.9999e+009 cy 9.9999e+009 lv 9.9999e+009 tl 9.9999e+009 di 0 dm 1 d2 31073 stds";

            reader = new SobekGeneralStructureReader(SobekType.SobekRE);
            var sobekGeneralStructure = (SobekGeneralStructure)reader.GetStructure(source);
            Assert.IsTrue(sobekGeneralStructure.ImportFromRE);
        }
    }
}