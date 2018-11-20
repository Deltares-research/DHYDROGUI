using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    public static class BoundaryTestHelper
    {
        public static IList<IDelftBcQuantityData> GetBcQuantityDataTable(DateTime startTime,
            IEnumerable<DateTime> timeValues,
            IList<double> values,
            string quantityStr,
            string unitStr)
        {
            var table = new List<IDelftBcQuantityData>();

            var timeQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.Time, BoundaryRegion.Quantity.Description);
            var timeUnitString = String.Format("{0} {1}", BoundaryRegion.UnitStrings.TimeMinutes, startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat));
            var timeUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, timeUnitString, BoundaryRegion.Unit.Description);

            var formattedDateTimes = timeValues.Select(t => (t - startTime).TotalMinutes).ToList();
            table.Add(new DelftBcQuantityData(timeQuantity, timeUnit, formattedDateTimes));

            var quantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, quantityStr, BoundaryRegion.Quantity.Description);
            var unit = new DelftIniProperty(BoundaryRegion.Unit.Key, unitStr, BoundaryRegion.Unit.Description);

            var data = values;
            if (!data.Any()) data = Enumerable.Repeat(0.0, formattedDateTimes.Count).ToList();

            table.Add(new DelftBcQuantityData(quantity, unit, data));
            return table;
        }
    }
}
