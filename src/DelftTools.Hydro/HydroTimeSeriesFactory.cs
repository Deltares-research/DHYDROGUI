using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DelftTools.Hydro
{
    public static class HydroTimeSeriesFactory
    {
        public static TimeSeries CreateWaterLevelTimeSeries()
        {
            TimeSeries ts = CreateTimeSeries("water level time series", "level", "m AD");

            ts.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevel;
            return ts;
        }

        public static TimeSeries CreateTimeSeries(string seriesName, string componentName, string unit)
        {
            var ts = new TimeSeries
            {
                Components = {new Variable<double>(componentName, new Unit(unit, unit))},
                Name = seriesName
            };

            ts.Time.DefaultValue = new DateTime(2000, 1, 1);
            ts.Time.InterpolationType = InterpolationType.Linear;
            ts.Time.ExtrapolationType = ExtrapolationType.Constant;

            return ts;
        }
    }
}