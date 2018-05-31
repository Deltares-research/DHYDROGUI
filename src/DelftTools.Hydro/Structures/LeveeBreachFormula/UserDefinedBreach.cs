using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    [Entity]
    public class UserDefinedBreach : LeveeBreachSettings
    {
        public UserDefinedBreach()
        {
            TimeSeries = new TimeSeries();
            //timeSeries.Time.SetValues(series.Select(s => settings.StartTimeBreachGrowth + s.TimeSpan));
            TimeSeries.Components.Add(new Variable<double>("Depth", new Unit("m", "m")));
            //timeSeries.Components[0].SetValues(series.Select(s => s.Height));
            TimeSeries.Components.Add(new Variable<double>("Width", new Unit("m", "m")));
            //timeSeries.Components[1].SetValues(series.Select(s => s.Width));

            CreateDummySeries();
        }

        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.UserDefinedBreach;

        public EventedList<BreachGrowthSetting> ManualBreachGrowthSettings { get; set; } = new EventedList<BreachGrowthSetting>();

        public TimeSeries TimeSeries { get; set; }

        private void CreateDummySeries()
        {
            var timespan = new List<DateTime>()
            {
                new DateTime(2001, 1, 1, 0, 10, 0),
                new DateTime(2001, 1, 1, 0, 20, 0),
                new DateTime(2001, 1, 1, 0, 30, 0),
                new DateTime(2001, 1, 1, 0, 40, 0),
                new DateTime(2001, 1, 1, 0, 50, 0),
            };
            TimeSeries.Time.SetValues(timespan);

            var depth = new List<double>
            {
                1.0,
                2.0,
                3.0,
                4.0,
                5.0,
            };
            TimeSeries.Components[0].SetValues(depth);

            var width = new List<double>
            {
                5.0,
                4.0,
                3.0,
                2.0,
                1.0,
            };
            TimeSeries.Components[1].SetValues(width);

        }
    }
}