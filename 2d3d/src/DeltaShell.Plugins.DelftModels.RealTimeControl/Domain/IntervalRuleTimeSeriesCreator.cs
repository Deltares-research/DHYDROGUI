using DelftTools.Functions;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public static class IntervalRuleTimeSeriesCreator
    {
        public static TimeSeries Create()
        {
            var localTimeSeries = new TimeSeries {Name = "Setpoints"};
            localTimeSeries.Time.InterpolationType = InterpolationType.Constant;
            localTimeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            localTimeSeries.Components.Add(new Variable<double>
            {
                Name = "Value",
                NoDataValue = -999.0
            });
            localTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                FunctionAttributes.StandardNames.RtcIntervalRule;

            return localTimeSeries;
        }
    }
}