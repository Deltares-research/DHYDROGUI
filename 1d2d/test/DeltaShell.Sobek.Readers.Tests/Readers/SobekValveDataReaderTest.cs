using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekValveDataReaderTest
    {
        [Test]
        public void ReadIdString()
        {
            string tabFileData = @"VLVE id 'culvert_5' nm 'Valve for culvert 5' lt lc" + Environment.NewLine +
            @"TBLE" + Environment.NewLine +
            @"0.0 2.10 <" + Environment.NewLine +
            @"0.1 1.96 <" + Environment.NewLine +
            @"0.2 1.80 <" + Environment.NewLine +
            @"0.3 1.74 <" + Environment.NewLine +
            @"0.4 1.71 <" + Environment.NewLine +
            @"0.5 1.71 <" + Environment.NewLine +
            @"0.6 1.71 <" + Environment.NewLine +
            @"0.7 1.64 <" + Environment.NewLine +
            @"0.8 1.51 <" + Environment.NewLine +
            @"0.9 1.36 <" + Environment.NewLine +
            @"1.0 1.19 <" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @" vlve" + Environment.NewLine +
            @"";
            var valveData = new SobekValveDataReader().Parse(tabFileData).ToList();

            Assert.AreEqual(1,valveData.Count);
            Assert.AreEqual("culvert_5", valveData[0].Id);
        }

        [Test]
        public void ReadSingleTable()
        {
            string tabFileData = @"VLVE id '5' nm 'Valve for culvert 5' lt lc" + Environment.NewLine +
            @"TBLE" + Environment.NewLine +
            @"0.0 2.10 <" + Environment.NewLine +
            @"0.1 1.96 <" + Environment.NewLine +
            @"0.2 1.80 <" + Environment.NewLine +
            @"0.3 1.74 <" + Environment.NewLine +
            @"0.4 1.71 <" + Environment.NewLine +
            @"0.5 1.71 <" + Environment.NewLine +
            @"0.6 1.71 <" + Environment.NewLine +
            @"0.7 1.64 <" + Environment.NewLine +
            @"0.8 1.51 <" + Environment.NewLine +
            @"0.9 1.36 <" + Environment.NewLine +
            @"1.0 1.19 <" + Environment.NewLine +
            @"tble" + Environment.NewLine +
            @" vlve" + Environment.NewLine +
            @"";
            var valveData = new SobekValveDataReader().Parse(tabFileData).ToList();

            Assert.AreEqual(1, valveData.Count);
            Assert.AreEqual("5", valveData[0].Id);
            Assert.AreEqual(11, valveData[0].DataTable.Rows.Count);
            Assert.AreEqual(1.36, valveData[0].DataTable.Rows[9][1]);
        }

        [Test]
        public void ReadTableWithTwoEntries()
        {
            var source = @"VLVE id '5' nm 'Valve for culvert 5' lt lc" + Environment.NewLine +
                         @"TBLE" + Environment.NewLine +
                         @"0.0 2.10 <" + Environment.NewLine +
                         @"0.1 1.96 <" + Environment.NewLine +
                         @"0.2 1.80 <" + Environment.NewLine +
                         @"0.3 1.74 <" + Environment.NewLine +
                         @"0.4 1.71 <" + Environment.NewLine +
                         @"0.5 1.71 <" + Environment.NewLine +
                         @"0.6 1.71 <" + Environment.NewLine +
                         @"0.7 1.64 <" + Environment.NewLine +
                         @"0.8 1.51 <" + Environment.NewLine +
                         @"0.9 1.36 <" + Environment.NewLine +
                         @"1.0 1.19 <" + Environment.NewLine +
                         @"tble" + Environment.NewLine +
                         @"vlve" + Environment.NewLine +
                         @"VLVE id '6' nm 'Valve for culvert 6' lt lc" + Environment.NewLine +
                         @"TBLE" + Environment.NewLine +
                         @"0.0 2.10 <" + Environment.NewLine +
                         @"0.1 1.96 <" + Environment.NewLine +
                         @"0.2 1.80 <" + Environment.NewLine +
                         @"0.3 1.74 <" + Environment.NewLine +
                         @"0.4 1.71 <" + Environment.NewLine +
                         @"0.5 1.71 <" + Environment.NewLine +
                         @"0.6 1.71 <" + Environment.NewLine +
                         @"0.7 1.64 <" + Environment.NewLine +
                         @"0.8 1.51 <" + Environment.NewLine +
                         @"0.9 1.36 <" + Environment.NewLine +
                         @"1.0 1.19 <" + Environment.NewLine +
                         @"tble" + Environment.NewLine +
                         @" vlve" + Environment.NewLine +
                         @"";

            var valveData = new SobekValveDataReader().Parse(source).ToList();
            Assert.AreEqual(2, valveData.Count);
            Assert.AreEqual(new[] {"5", "6"}, Enumerable.ToArray<string>(valveData.Select(vd => vd.Id)));
        }
    }
}
