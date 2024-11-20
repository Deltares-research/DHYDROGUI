using System;
using System.Data;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    class SobekDataTableReaderTest
    {
        [Test]
        public void ParseBasicCrossSectionTable()
        {
            var tableText = "TBLE" + Environment.NewLine +
                            "0 0 <" + Environment.NewLine +
                            "1 -0.5 <" + Environment.NewLine +
                            "1.5 -0.5 <" + Environment.NewLine +
                            "2.75 -1 <" + Environment.NewLine +
                            "4.75 -1.5 <" + Environment.NewLine +
                            "8.15 -1.5 <" + Environment.NewLine +
                            "10.15 -1 <" + Environment.NewLine +
                            "11.4 -0.5 <" + Environment.NewLine +
                            "12.9 -0.5 <" + Environment.NewLine +
                            "13.9 -0.25 <" + Environment.NewLine +
                            "14.4 0 <" + Environment.NewLine +
                            "tble" + Environment.NewLine;

            var targetTable = new DataTable();
            targetTable.Columns.Add(new DataColumn("x", typeof(double)));
            targetTable.Columns.Add(new DataColumn("y", typeof(double)));

            var table = SobekDataTableReader.GetTable(tableText, targetTable);
            Assert.AreEqual(11, table.Rows.Count);
            Assert.AreEqual(14.4, table.Rows[10][0]);
        }

        [Test]
        public void ParseTableWithRedundantSpaces()
        {
            var tableText = " TBLE " + Environment.NewLine +
                            "0  0   <" + Environment.NewLine +
                            "1  -0.5 <" + Environment.NewLine +
                            "tble " + Environment.NewLine;

            var targetTable = new DataTable();
            targetTable.Columns.Add(new DataColumn("x", typeof(double)));
            targetTable.Columns.Add(new DataColumn("y", typeof(double)));

            var table = SobekDataTableReader.GetTable(tableText, targetTable);
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(0, table.Rows[0][0]);
            Assert.AreEqual(-0.5, table.Rows[1][1]);
        }

        [Test]
        public void ParseTableWithMissingEndCharacter()
        {
            var tableText = "TBLE" + Environment.NewLine +
                            "0 0 <" + Environment.NewLine +
                            "1 -0.5" + Environment.NewLine +
                            "tble" + Environment.NewLine;

            var targetTable = new DataTable();
            targetTable.Columns.Add(new DataColumn("x", typeof(double)));
            targetTable.Columns.Add(new DataColumn("y", typeof(double)));

            var table = SobekDataTableReader.GetTable(tableText, targetTable);
            Assert.AreEqual(2, table.Rows.Count);
        }

        [Test]
        public void ParseTableOnSingleLine()
        {
            var tableText = "TBLE 0 0 < 1 -0.5 < tble";

            var targetTable = new DataTable();
            targetTable.Columns.Add(new DataColumn("x", typeof(double)));
            targetTable.Columns.Add(new DataColumn("y", typeof(double)));

            var table = SobekDataTableReader.GetTable(tableText, targetTable);
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(-0.5, table.Rows[1][1]);
        }

        [Test]
        public void ParseTableWithSplitCharacterInString()
        {
            var name = "'back <=> forth'";
            var tableText = "TBLE 0 'a name' < 1 " + name + " < tble";

            var targetTable = new DataTable();
            targetTable.Columns.Add(new DataColumn("x", typeof(double)));
            targetTable.Columns.Add(new DataColumn("name", typeof(string)));

            var table = SobekDataTableReader.GetTable(tableText, targetTable);
            Assert.AreEqual(2, table.Rows.Count);
            Assert.AreEqual(name, table.Rows[1][1]);
        }
    }
}
