using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public class BoundaryFileWriter
    {
        protected static string GetTimeSeriesIsPeriodicProperty(IFunction timeSeries)
        {
            string periodic = null;
            if (timeSeries != null && timeSeries.Arguments != null && timeSeries.Arguments.Count > 0)
            {
                periodic = timeSeries.Arguments[0].ExtrapolationType == ExtrapolationType.Periodic ? "true" : null;
            }
            return periodic;
        }
 
        protected static IList<IBcQuantityData> GenerateTableForConstantData(string quantityType, string unitType, double value)
        {
            var quantity = new IniProperty(BoundaryRegion.Quantity.Key, quantityType, BoundaryRegion.Quantity.Description);
            var unit = new IniProperty(BoundaryRegion.Unit.Key, unitType, BoundaryRegion.Unit.Description);
            return new List<IBcQuantityData>() { new BcQuantityData(quantity, unit, new List<double>() { value }) };
        }
 
        protected static IList<IBcQuantityData> GenerateTableForTimeSeriesData(QuantityUnitPair quantityUnitPair, IFunction functionData, DateTime startTime)
        {
            var table = new List<IBcQuantityData>();
            if (!functionData.Arguments.Any()) return table;
            
            var timeQuantity = new IniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.Time, BoundaryRegion.Quantity.Description);
            var timeUnitString = String.Format("{0} {1}", BoundaryRegion.UnitStrings.TimeMinutes, startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat));
            var timeUnit = new IniProperty(BoundaryRegion.Unit.Key, timeUnitString, BoundaryRegion.Unit.Description);
            
            var timeData = ((MultiDimensionalArray<DateTime>)functionData.Arguments[0].Values).ToList();
            var formattedDateTimes = ConvertDateTimeDataToMinutesSinceReferenceDateTime(timeData, startTime).ToList();
            if (!formattedDateTimes.Any()) formattedDateTimes = new List<double> { 0.0 };

            table.Add(new BcQuantityData(timeQuantity, timeUnit, formattedDateTimes));


            var quantity = new IniProperty(BoundaryRegion.Quantity.Key, quantityUnitPair.Quantity, BoundaryRegion.Quantity.Description);
            var unit = new IniProperty(BoundaryRegion.Unit.Key, quantityUnitPair.Unit, BoundaryRegion.Unit.Description);

            var data = new List<double>();
            if (functionData.Components.Any())
            {
                data = ((MultiDimensionalArray<double>) functionData.Components[0].Values).ToList();
            }
            if(!data.Any()) data = Enumerable.Repeat(0.0, formattedDateTimes.Count).ToList();

            table.Add(new BcQuantityData(quantity, unit, data));
            

            return table;
        }

        protected static IList<IBcQuantityData> GenerateTableForDischargeWaterLevelData(IFunction data,
            string lateralDischarge = null)
        {
            var levelQuantity = new IniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.QHDischargeWaterLevelDependency + " " + BoundaryRegion.QuantityStrings.QHWaterLevelDependencyKey, BoundaryRegion.Quantity.Description);
            var levelUnit = new IniProperty(BoundaryRegion.Unit.Key, BoundaryRegion.UnitStrings.WaterLevel, BoundaryRegion.Unit.Description);
            var levelData = ((MultiDimensionalArray<double>)(data.Arguments[0].Values)).ToList();

            var dischargeQuantity = new IniProperty(BoundaryRegion.Quantity.Key, lateralDischarge ?? BoundaryRegion.QuantityStrings.QHDischargeWaterLevelDependency + " " + BoundaryRegion.QuantityStrings.QHDischargeDependencyKey, BoundaryRegion.Quantity.Description);
            var dischargeUnit = new IniProperty(BoundaryRegion.Unit.Key, BoundaryRegion.UnitStrings.WaterDischarge, BoundaryRegion.Unit.Description);
            var dischargeData = ((MultiDimensionalArray<double>)(data.Components[0].Values)).ToList();
            if (levelData.Count == 0 && dischargeData.Count == 0)
            {
                levelData.Add(0.0);
                dischargeData.Add(0.0);
            }

            return new List<IBcQuantityData>()
            {
                new BcQuantityData(levelQuantity, levelUnit, levelData), new BcQuantityData(dischargeQuantity, dischargeUnit, dischargeData) 
            };
        }
 
        private static IEnumerable<double> ConvertDateTimeDataToMinutesSinceReferenceDateTime(IEnumerable<DateTime> dateTimes, DateTime refDateTime)
        {
            // this is our prefered format of datetime
            return dateTimes.Select(t => (t - refDateTime).TotalMinutes).ToList();
        }
    }
}