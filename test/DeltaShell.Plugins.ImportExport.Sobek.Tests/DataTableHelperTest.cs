using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class DataTableHelperTest
    {
        [Test]
        public void ConvertTimeTable()
        {
            var dataTable = SobekFlowBoundaryCondition.TimeTableStructure;
            var row = dataTable.NewRow();
            row[0] = new DateTime(2000, 1, 1, 2, 2, 3);
            row[1] = 67.0;
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row[0] = new DateTime(2001, 1, 1, 2, 2, 3);
            row[1] = 13.0;
            dataTable.Rows.Add(row);
            IFunction timeSeries = new TimeSeries();
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("concentration"));

            DataTableHelper.SetTableToFunction(dataTable, timeSeries);

            Assert.AreEqual(2, timeSeries.Arguments[0].Values.Count);
        }

        [Test]
        public void ConvertTimeTableNonUnique()
        {
            var dataTable = SobekFlowBoundaryCondition.TimeTableStructure;
            var row = dataTable.NewRow();
            row[0] = new DateTime(2000, 1, 1, 2, 2, 3);
            row[1] = 67.0;
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row[0] = new DateTime(2001, 1, 1, 2, 2, 3);
            row[1] = 13.0;
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row[0] = new DateTime(2001, 1, 1, 2, 2, 3);
            row[1] = 14.0;
            dataTable.Rows.Add(row);
            IFunction timeSeries = new TimeSeries();
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("concentration"));

            DataTableHelper.SetTableToFunction(dataTable, timeSeries);

            Assert.AreEqual(2, timeSeries.Arguments[0].Values.Count);
        }

        [Test]
        public void ConvertTableWithDescendingValuesToSortedFunction()
        {
            var dataTable = SobekFlowBoundaryCondition.QhTableStructure;
            var row = dataTable.NewRow();
            row[0] = 10.0;
            row[1] = 10.0;
            dataTable.Rows.Add(row);
            row = dataTable.NewRow();
            row[0] = 5.0;
            row[1] = 5.0;
            dataTable.Rows.Add(row);
            IFunction function = new Function();
            function.Arguments.Add(new Variable<double>("input"));
            function.Components.Add(new Variable<double>("output"));

            DataTableHelper.SetTableToFunction(dataTable, function);

            Assert.AreEqual(2, function.Arguments[0].Values.Count);
        }
    }
}
