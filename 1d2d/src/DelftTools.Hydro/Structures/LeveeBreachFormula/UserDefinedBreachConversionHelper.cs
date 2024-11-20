using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    public static class UserDefinedBreachConversionHelper
    {
        public static TimeSeries GetFormattedTimeSeries()
        {
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("Depth", new Unit("m", "m")));
            timeSeries.Components.Add(new Variable<double>("Width", new Unit("m", "m")));

            return timeSeries;
        }

        public static TimeSeries CreateTimeSeriesFromTable(this UserDefinedBreachSettings settings)
        {
            // Conversion 
            var timeSeries = GetFormattedTimeSeries();
            timeSeries.Time.SetValues(settings.ManualBreachGrowthSettings.Select(s => settings.StartTimeBreachGrowth + s.TimeSpan));
            timeSeries.Components[0].SetValues(settings.ManualBreachGrowthSettings.Select(s => s.Height));
            timeSeries.Components[1].SetValues(settings.ManualBreachGrowthSettings.Select(s => s.Width));
            return timeSeries;
        }

        public static void CreateTableFromTimeSeries(this UserDefinedBreachSettings settings, TimeSeries timeSeries)
        {
            var dateTimes = timeSeries.Time.AllValues.Select(d => d - settings.StartTimeBreachGrowth).ToList();
            var heights = timeSeries.Components[0].Values.Cast<double>().ToList();
            var widths = timeSeries.Components[1].Values.Cast<double>().ToList();

            settings.ManualBreachGrowthSettings = new EventedList<BreachGrowthSetting>();

            for (var i = 0; i < dateTimes.Count(); i++)
            {
                settings.ManualBreachGrowthSettings.Add(new BreachGrowthSetting
                {
                    TimeSpan = dateTimes[i],
                    Height = heights[i],
                    Width = widths[i],
                });
            }
        }
    }
}