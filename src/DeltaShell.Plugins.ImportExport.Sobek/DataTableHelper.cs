using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public static class DataTableHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataTableHelper));

        /// <summary>
        /// Set a datatable to an IFunction object
        /// assumption is that columns match
        /// First column will be argument in function
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="function"></param>
        public static void SetTableToFunction(DataTable dataTable, IFunction function)
        {
            var values = new SortedDictionary<object, object>();

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var argumentValue = dataTable.Rows[i][0];

                if (!values.ContainsKey(argumentValue))
                {
                    values[argumentValue] = dataTable.Rows[i][1];
                }
                else
                {
                    log.WarnFormat("Duplicate entry during import {0}: {1} with both value {2} and {3}.", dataTable.TableName, argumentValue, function[argumentValue], dataTable.Rows[i][1]);
                }
            }

            var arrayArgumentValues = function.Arguments[0].Values;
            var arrayComponentValues = function.Components[0].Values;

            arrayArgumentValues.FireEvents = false;
            arrayComponentValues.FireEvents = false;
            arrayArgumentValues.AddRange(values.Keys.ToArray());
            arrayComponentValues.AddRange(values.Values.ToArray());
            arrayArgumentValues.FireEvents = true;
            arrayComponentValues.FireEvents = true;
        }

        public static DataTable SwapColumns(DataTable dataTable)
        {
            if (dataTable.Columns.Count != 2)
            {
                throw new ArgumentException("Can only swap table with 2 columns", "dataTable");
            }
            var newTable = new DataTable();
            newTable.Columns.Add(dataTable.Columns[1].ColumnName, dataTable.Columns[1].DataType);
            newTable.Columns.Add(dataTable.Columns[0].ColumnName, dataTable.Columns[0].DataType);
            foreach (DataRow row in dataTable.Rows)
            {
                var newRow = newTable.NewRow();
                newRow[0] = row[1];
                newRow[1] = row[0];
                newTable.Rows.Add(newRow);
            }
            return newTable;
        }

        public static TimeSeries ConvertDataTableToTimeSeries(DataTable dataTable, string name)
        {
            var timeSeries = new TimeSeries { Name = name + " table" };
            timeSeries.Components.Add(new Variable<double>(name));

            if (dataTable.ExtendedProperties.ContainsKey("block-interpolation"))
            {
                var isBlock = (bool) dataTable.ExtendedProperties["block-interpolation"];
                timeSeries.Arguments[0].InterpolationType = isBlock
                                                                ? InterpolationType.Constant
                                                                : InterpolationType.Linear;
            }

            var sortedDataRowCollection = dataTable.Rows.OfType<DataRow>().OrderBy(r => (DateTime) r[0]);
            var times = sortedDataRowCollection.Select(r => r[0]).OfType<DateTime>().ToList();
            var values = sortedDataRowCollection.Select(r => Convert.ToDouble(r[1],CultureInfo.InvariantCulture)).ToList();
           
            timeSeries.Time.SetValues(times);
            timeSeries.Components[0].SetValues(values);

            return timeSeries;
        }
    }
}
