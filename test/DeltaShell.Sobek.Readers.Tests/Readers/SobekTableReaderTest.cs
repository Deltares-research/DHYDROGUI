using System;
using System.Collections.Generic;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekTableReaderTest
    {
        [Test]
        public void Read()
        {
            string source = @"TBLE" + Environment.NewLine +
                            @"0.82 1.00 <" + Environment.NewLine +
                            @"0.86 0.95 <" + Environment.NewLine +
                            @"0.90 0.90 <" + Environment.NewLine +
                            @"0.94 0.80 <" + Environment.NewLine +
                            @"0.96 0.70 <" + Environment.NewLine +
                            @"0.97 0.60 <" + Environment.NewLine +
                            @"1.00 0.00 <" + Environment.NewLine +
                            @"tble";

            var table = SobekDataTableReader.GetTable(source, new Dictionary<string, Type>
                                                                  {
                                                                      {"een", typeof (float)},
                                                                      {"twee", typeof (float)}
                                                                  });

            Assert.AreEqual(7, table.Rows.Count);
            Assert.AreEqual("een", table.Columns[0].ColumnName);
            Assert.AreEqual("twee", table.Columns[1].ColumnName);
            Assert.AreEqual(0.94f, (float) table.Rows[3][0]);
        }

        /// <summary>
        /// TOOLS-4423 Cannot import Test_120 (Testbenchtest 120 fails)
        /// line end with "<" and not " <"
        /// </summary>
        [Test]
        public void ParseDifferentLineEnding()
        {
            var source = @"3 3 PDIN 1 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine + 
                @"75 34 34<" + Environment.NewLine + 
                @"870 30 30<" + Environment.NewLine + 
                @"1500 31 31<" + Environment.NewLine + 
                @"2500 28 28<" + Environment.NewLine + 
                @"tble" + Environment.NewLine + @"d";

            var dataTable = SobekDataTableReader.CreateDataTableDefinitionFromColumNames(source);
            Assert.AreEqual(3, dataTable.Columns.Count);
            Assert.AreEqual("h", dataTable.Columns[0].ColumnName);
            Assert.AreEqual("0", dataTable.Columns[1].ColumnName);
            Assert.AreEqual("2000", dataTable.Columns[2].ColumnName);
            dataTable = SobekDataTableReader.GetTable(source, dataTable);
            Assert.AreEqual(4, dataTable.Rows.Count);
        }

        /// <summary>
        /// TOOLS-4423 Cannot import Test_120 (Testbenchtest 120 fails)
        /// too greedy quantifier
        /// </summary>
        [Test]
        public void ParseTableWithJunkAttached()
        {
            var source = @"3 3 PDIN 1 0 '' pdin CLTT 'h' '0' '2000' cltt TBLE" + Environment.NewLine +
                @"75 34 34.2 <" + Environment.NewLine + 
                "870 30 30.2 <" + Environment.NewLine +
                @"1500 31 31.2 <" + Environment.NewLine +
                @"2500 28 28.2 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"d9 f9 2  0.0007 PDIN 1 0 '' pdin TBLE" + Environment.NewLine +
                @"0 0.0007 <" + Environment.NewLine +
                @"2000 1 <" + Environment.NewLine +
                @"tble" + Environment.NewLine + 
                @"sf 1 st cp 0 1234 0 sr cp 0 1234 bdfr";

            var dataTable = SobekDataTableReader.CreateDataTableDefinitionFromColumNames(source);
            Assert.AreEqual(3, dataTable.Columns.Count);
            Assert.AreEqual("h", dataTable.Columns[0].ColumnName);
            Assert.AreEqual("0", dataTable.Columns[1].ColumnName);
            Assert.AreEqual("2000", dataTable.Columns[2].ColumnName);
            dataTable = SobekDataTableReader.GetTable(source, dataTable);
            Assert.AreEqual(4, dataTable.Rows.Count);
        }
    }
}