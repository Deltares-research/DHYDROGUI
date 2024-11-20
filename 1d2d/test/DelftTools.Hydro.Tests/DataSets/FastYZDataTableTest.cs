using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.DataSets
{
    [TestFixture]
    public class FastYZDataTableTest
    {
        readonly Random random = new Random();
        
        [Test]
        [NUnit.Framework.Category(TestCategory.Performance)]
        public void SerializeAndDeserialize()
        {
            FastDataTableTestHelper.TestSerializationIsFastAndCorrect<FastYZDataTable>(7, 30,
                                                                                       (t) =>
                                                                                       t.AddCrossSectionYZRow(
                                                                                           random.NextDouble(),
                                                                                           random.NextDouble()));
        }

        [Test]
        public void TestMemoryConsumption()
        {
            var memoryBefore = GC.GetTotalMemory(true);

            int numTables = 10;
            int numRows = 30;

            var tables = new List<FastYZDataTable>(numTables);
            for (var i = 0; i < numTables; i++)
            {
                var table = new FastYZDataTable();
                tables.Add(table);
            }

            var memoryAfterCreate = GC.GetTotalMemory(true);

            for (int i = 0; i < numTables; i++)
            {
                for (int j = 0; j < numRows; j++)
                    tables[i].AddCrossSectionYZRow(j, 0.0);
            }

            var memoryAfterAddingRows = GC.GetTotalMemory(true);

            var consumptionPerTable = (memoryAfterCreate - memoryBefore) / numTables;
            var consumptionPerRow = ((memoryAfterAddingRows - memoryAfterCreate)/numTables)/numRows;
            Console.WriteLine("{0:0.00}kb\n{1:0.00}kb", consumptionPerTable / 1024.0, consumptionPerRow / 1024.0);

            Console.WriteLine("{0:0.0} Mb = Size of 1000 30-row crosssections",
                              (1000*consumptionPerTable + 1000*30*consumptionPerRow)/(1024.0*1024.0));
        }

        [Test]
        public void AddAndChangeRowsCheckSorting()
        {
            var table = new FastYZDataTable();

            int moves = 0;
            table.Rows.ListChanged += (s, e) => { if (e.ListChangedType == ListChangedType.ItemMoved) moves++; };

            table.AddCrossSectionYZRow(100, 0);
            Assert.AreEqual(0, moves);

            table.AddCrossSectionYZRow(10, 0);
            table.AddCrossSectionYZRow(50, 0);
            table.AddCrossSectionYZRow(0, 0);
            table.AddCrossSectionYZRow(-10, 0);
            table.AddCrossSectionYZRow(120, 0);

            Assert.AreEqual(4, moves);
            Assert.AreEqual(new[] {-10.0, 0, 10, 50, 100, 120}, table.Rows.Select(r => r[0]).ToArray());

            table[0][0] = 130; //was -10
            table[3][0] = -20; //was 100

            Assert.AreEqual(new[] { -20.0, 0, 10, 50, 120, 130 }, table.Rows.Select(r => r[0]).ToArray());
        }
    }
}