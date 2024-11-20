using System;
using System.Data;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekRiverWeirReaderTest
    {
        private static readonly SobekRiverWeirReader Reader;

        static SobekRiverWeirReaderTest()
        {
            Reader = new SobekRiverWeirReader();
        }

        [Test]
        public void Read()
        {
            Assert.AreEqual(0, Reader.Type);

            string source = @"cl 5 cw 10 cs 0 po 1 ps 0.82 pt pr" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0.82 1.00 <" + Environment.NewLine +
                            @"0.86 0.95 <" + Environment.NewLine +
                            @"0.90 0.90 <" + Environment.NewLine +
                            @"0.94 0.80 <" + Environment.NewLine +
                            @"0.96 0.70 <" + Environment.NewLine +
                            @"0.97 0.60 <" + Environment.NewLine +
                            @"1.00 0.00 <" + Environment.NewLine +
                            @"tble no 0.9 ns 0.83 nt nr PDIN 0 0  pdin" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0.82 1.00 <" + Environment.NewLine +
                            @"0.86 0.95 <" + Environment.NewLine +
                            @"0.90 0.90 <" + Environment.NewLine +
                            @"0.94 0.80 <" + Environment.NewLine +
                            @"0.96 0.70 <" + Environment.NewLine +
                            @"0.97 0.60 <" + Environment.NewLine +
                            @"1.00 0.00 <" + Environment.NewLine +
                            @"tble";

            var weir = (SobekRiverWeir)Reader.GetStructure(source);

            Assert.AreEqual(5.0f, weir.CrestLevel);
            Assert.AreEqual(10.0f, weir.CrestWidth);
            Assert.AreEqual(0, weir.CrestShape);
            Assert.AreEqual(1.0f, weir.CorrectionCoefficientPos);
            Assert.AreEqual(0.82f, weir.SubmergeLimitPos);
            DataTable positiveReductionTable = weir.PositiveReductionTable;
            Assert.AreEqual(7, positiveReductionTable.Rows.Count);
            Assert.AreEqual(0.97f, (float)positiveReductionTable.Rows[5][0]);

            //TODO: more asserts about the table
            Assert.AreEqual(0.9f, weir.CorrectionCoefficientNeg);
            Assert.AreEqual(0.83f, weir.SubmergeLimitNeg);
            DataTable negativeReductionTable = weir.NegativeReductionTable;
            Assert.AreEqual(7, negativeReductionTable.Rows.Count);
            Assert.AreEqual(0.60f, (float)negativeReductionTable.Rows[5][1]);
        }
        [Test]
        public void ReadOtherFormat()
        {
            string source =
                @"cl 2.53 cs 0 cw 130 po 1 ps 0.82 pt pr 'P_Reduction curve table' PDIN 0 0 '' pdin CLTT 'h2/h1' " +
                @"'Reduction' cltt CLID '(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0.82 1 <" + Environment.NewLine +
                @"0.86 0.95 <" + Environment.NewLine +
                @"0.9 0.9 <" + Environment.NewLine +
                @"0.94 0.8 <" + Environment.NewLine +
                @"0.96 0.7 <" + Environment.NewLine +
                @"0.97 0.6 <" + Environment.NewLine +
                @"1 0 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"no 1 ns 0.82 nt nr 'P_Reduction curve table' PDIN 0 0 '' pdin CLTT 'h2/h1' 'Reduction' cltt CLID " +
                @"'(null)' '(null)' clid TBLE" + Environment.NewLine +
                @"0.82 1 <" + Environment.NewLine +
                @"0.86 0.95 <" + Environment.NewLine +
                @"0.9 0.9 <" + Environment.NewLine +
                @"0.94 0.8 <" + Environment.NewLine +
                @"0.96 0.7 <" + Environment.NewLine +
                @"0.97 0.6 <" + Environment.NewLine +
                @"1 0 <" + Environment.NewLine +
                @"tble";
            /*@"sw 9.9999e+009 ni 31073 ph 10 pw 3 pp 0.01 pa 0.1 nh 10 nw 3 np 0.01 na 0.1 w1 9.9999e+009 wl " +
            @"9.9999e+009 ws 9.9999e+009 wr 9.9999e+009 w2 9.9999e+009 z1 9.9999e+009 zl 9.9999e+009 zs " +
            @"9.9999e+009 zr 9.9999e+009 z2 9.9999e+009 gh 9.9999e+009 pg 9.9999e+009 pd 9.9999e+009 pi " +
            @"9.9999e+009 pr 9.9999e+009 pc 9.9999e+009 ng 9.9999e+009 nd 9.9999e+009 nf 9.9999e+009 nr " +
            @"9.9999e+009 nc 9.9999e+009 dn 0 rt cr 0 9.9999e+009 9.9999e+009 cy 9.9999e+009 lv 9.9999e+009 tl " +
            @"9.9999e+009 di 0 dm 1 d2 -99999";*/
            var weir = (SobekRiverWeir)Reader.GetStructure(source);
            Assert.AreEqual(130, weir.CrestWidth);
            Assert.AreEqual(0, weir.CrestShape);
        }
    }
}