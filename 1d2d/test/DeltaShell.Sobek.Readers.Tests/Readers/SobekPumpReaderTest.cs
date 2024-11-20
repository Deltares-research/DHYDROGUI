using System;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekPumpReaderTest
    {
        // test parsing pumps without leading properties and closing tag. This is how SobekStructureDefFileReader
        // will use the SobekPumpReader

        [Test]
        public void PumpWithTablesTest()
        {
            string source = @"STDS id '4' nm 'UrbanPump02' ty 9 dn 2 rt cr 1" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"2 0.5 <" + Environment.NewLine +
                @"5 0.7 <" + Environment.NewLine +
                @"8 0.9 <" + Environment.NewLine +
                @"tble ct lt 1" + Environment.NewLine +
                @"TBLE 8 0.05 -0.20 0.05 1.50 <" + Environment.NewLine +
                @"10 0.10 -0.10 0.10 1.25 <" + Environment.NewLine +
                @"12 0.20 -0.05 0.20 1.00 <" + Environment.NewLine +
                @"tble stds";
            var reader = new SobekPumpReader();
            Assert.AreEqual(9, reader.Type);

            var sobekPump = (SobekPump) reader.GetStructure(source);
            Assert.AreEqual(2, sobekPump.Direction);
            Assert.AreEqual(3, sobekPump.ReductionTable.Rows.Count);
            Assert.AreEqual(5.0, (double)sobekPump.ReductionTable.Rows[1][0], 1.0e-6);
            Assert.AreEqual(0.7, (double)sobekPump.ReductionTable.Rows[1][1], 1.0e-6);
            Assert.AreEqual(3, sobekPump.CapacityTable.Rows.Count);
            Assert.AreEqual(10.0, (double)sobekPump.CapacityTable.Rows[1][0], 1.0e-6);
            Assert.AreEqual(0.1, (double)sobekPump.CapacityTable.Rows[1][1], 1.0e-6);
            Assert.AreEqual(-0.10, (double)sobekPump.CapacityTable.Rows[1][2], 1.0e-6);
            Assert.AreEqual(0.1, (double)sobekPump.CapacityTable.Rows[1][3], 1.0e-6);
            Assert.AreEqual(1.25, (double)sobekPump.CapacityTable.Rows[1][4], 1.0e-6);
        }

        [Test]
        public void PumpWithConstantReduction()
        {
            string source = @"STDS id 'l_14' nm '' ty 9 dn 1 rt cr 0 1.15 0 ct lt 1" + Environment.NewLine +
                    @"TBLE" + Environment.NewLine +
                    @"0.33 0.8 0.6 0 0 <" + Environment.NewLine +
                    @"tble stds";

            var reader = new SobekPumpReader();
            Assert.AreEqual(9, reader.Type);

            var sobekPump = (SobekPump)reader.GetStructure(source);
            Assert.AreEqual(1, sobekPump.Direction);
            Assert.AreEqual(0.33, (double)sobekPump.CapacityTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(1.15, (double)sobekPump.ReductionTable.Rows[0][1], 1.0e-6);
        }

        [Test]
        public void AnotherFailedPumpReadTest()
        {
            // string ending with "tble"
            string source = @"dn 1 rt cr 0 1 0 ct lt 1" + Environment.NewLine +
                    @"TBLE" + Environment.NewLine +
                    @"4.5 2.4 2.2 0 0 <" + Environment.NewLine +
                    @"tble";
            var reader = new SobekPumpReader();
            Assert.AreEqual(9, reader.Type);

            var sobekPump = (SobekPump)reader.GetStructure(source);
            Assert.AreEqual(1, sobekPump.Direction);
            Assert.AreEqual(0, (double)sobekPump.ReductionTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(1, (double)sobekPump.ReductionTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(4.5, (double)sobekPump.CapacityTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(2.4, (double)sobekPump.CapacityTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(2.2, (double)sobekPump.CapacityTable.Rows[0][2], 1.0e-6);
            Assert.AreEqual(0, (double)sobekPump.CapacityTable.Rows[0][3], 1.0e-6);
            Assert.AreEqual(0, (double)sobekPump.CapacityTable.Rows[0][4], 1.0e-6);
        }

        [Test]
        public void ImportRiverPump()
        {
            string source = 
                    //@"STDS id '17127' nm 'Pomp Example' ty 3 dn -1 rt cr 1 PDIN 0 0 pdin" + Environment.NewLine +
                    @"STDS id '4' nm 'UrbanPump02' ty 9 dn -1 rt cr 1 PDIN 0 0 pdin" + Environment.NewLine +
                    @"TBLE" + Environment.NewLine +
                    @"0 1 <" + Environment.NewLine +
                    @"1 .8 <" + Environment.NewLine +
                    @"2 .6 <" + Environment.NewLine +
                    @"3 .2 <" + Environment.NewLine +
                    @"tble ct lt 1 PDIN 0 0 '' pdin" + Environment.NewLine +
                    @"TBLE" + Environment.NewLine +
                    @"12 1.7 1.5 0 0 <" + Environment.NewLine +
                    @"tble stds";

            var reader = new SobekRiverPumpReader();
            Assert.AreEqual(3, reader.Type);

            var sobekPump = (SobekPump)reader.GetStructure(source);
            Assert.AreEqual(-1, sobekPump.Direction);
            Assert.AreEqual(4, sobekPump.ReductionTable.Rows.Count);
            Assert.AreEqual(0.0, (double)sobekPump.ReductionTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(1.0, (double)sobekPump.ReductionTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1, sobekPump.CapacityTable.Rows.Count);
            Assert.AreEqual(12.0, (double)sobekPump.CapacityTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(1.7, (double)sobekPump.CapacityTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(1.5, (double)sobekPump.CapacityTable.Rows[0][2], 1.0e-6);
            Assert.AreEqual(0.0, (double)sobekPump.CapacityTable.Rows[0][3], 1.0e-6);
            Assert.AreEqual(0.0, (double)sobekPump.CapacityTable.Rows[0][4], 1.0e-6);
        }

        /// <summary>
        /// from case TLS-1610.lit
        /// </summary>
        [Test]
        public void PumpFrom()
        {
            string source =
                @"STDS id '9' nm '' ty 9 dn 1 pu 2 rt cr 2" + Environment.NewLine +
                //@"STDS id '9' nm '' ty 9 dn 1 rt cr 2" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"-4 0.5 <" + Environment.NewLine +
                @"-3 0.7 <" + Environment.NewLine +
                @"-2 0.85 <" + Environment.NewLine +
                @"-1 0.95 <" + Environment.NewLine +
                @"0 1 <" + Environment.NewLine +
                @"tble ct lt 1" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"50 6 2 0 0 <" + Environment.NewLine +
                @"tble stds";
            var reader = new SobekRiverPumpReader();
            Assert.AreEqual(3, reader.Type);

            var sobekPump = (SobekPump)reader.GetStructure(source);
            Assert.AreEqual(1, sobekPump.Direction);
            Assert.AreEqual(-2, (double)sobekPump.ReductionTable.Rows[2][0], 1.0e-6);
            Assert.AreEqual(0.85, (double)sobekPump.ReductionTable.Rows[2][1], 1.0e-6);
            Assert.AreEqual(50, (double)sobekPump.CapacityTable.Rows[0][0], 1.0e-6);
            Assert.AreEqual(6, (double)sobekPump.CapacityTable.Rows[0][1], 1.0e-6);
            Assert.AreEqual(2, (double)sobekPump.CapacityTable.Rows[0][2], 1.0e-6);
        }
    }
}
