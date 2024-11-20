using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public static class TimeSeriesHelper
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TimeSeriesHelper));

        /// <summary>
        /// There is discrepantion between the sobek periodic extrapolation and the function periodic extrapolation
        /// A function periodic extrapolation is repeating the given serie
        /// A sobek periodic extrapolation has a period. Ex. the serie is one year, the period a day: the timeserie will be extrapolated by repeating the last day
        /// </summary>
        /// <param name="function"></param>
        /// <param name="sobekPeriodDefinition"></param>
        public static void SetPeriodicExtrapolationRtc(IFunction function, string sobekPeriodDefinition)
        {
            if (string.IsNullOrEmpty(sobekPeriodDefinition)) return;

            var period = sobekPeriodDefinition.Replace("'", "");
            var periodSplitted = period.Split(new[] { ';', ':' });
            var periodSpan = new TimeSpan();

            switch (periodSplitted.Length)
            {
                case 1:
                    periodSpan = new TimeSpan(0, 0, 0, Convert.ToInt32(periodSplitted[0]));
                    break;
                case 3:
                    periodSpan = new TimeSpan(0, Convert.ToInt32(periodSplitted[0]), Convert.ToInt32(periodSplitted[1]), Convert.ToInt32(periodSplitted[2]));
                    break;
                case 4:
                    periodSpan = new TimeSpan(Convert.ToInt32(periodSplitted[0]), Convert.ToInt32(periodSplitted[1]), Convert.ToInt32(periodSplitted[2]), Convert.ToInt32(periodSplitted[3]));
                    break;
                default:
                    Log.ErrorFormat("Period format {0} is not supported yet.", period);
                    break;
            }

            var timeVariable = function.Arguments[0];
            var firstTimeStep = (DateTime)timeVariable.MinValue;
            var lastTimeStep = (DateTime)timeVariable.MaxValue;
            var diffTimeSpan = lastTimeStep - firstTimeStep;

            if (periodSpan <= diffTimeSpan)
            {
                Log.ErrorFormat("The timeseries {0} can not be imported with extrapolation type 'Periodic'. Type of extrapolation has been set to 'None'.", function.Name);
                timeVariable.ExtrapolationType = ExtrapolationType.None;
            }
            else
            {
                timeVariable.ExtrapolationType = ExtrapolationType.Periodic;
                AddPeriodSpanAttributeToVariable(timeVariable, periodSpan);
            }
        }

        public static void SetPeriodicExtrapolationSobek(IFunction function, string sobekPeriodDefinition)
        {
            if (string.IsNullOrEmpty(sobekPeriodDefinition)) return;

            var period = sobekPeriodDefinition.Replace("'", "");
            var periodSplitted = period.Split(new[] { ';', ':' });
            var periodSpan = new TimeSpan();

            switch (periodSplitted.Length)
            {
                case 1:
                    periodSpan = new TimeSpan(0, 0, 0, Convert.ToInt32(periodSplitted[0]));
                    break;
                case 3:
                    periodSpan = new TimeSpan(0, Convert.ToInt32(periodSplitted[0]), Convert.ToInt32(periodSplitted[1]), Convert.ToInt32(periodSplitted[2]));
                    break;
                case 4:
                    periodSpan = new TimeSpan(Convert.ToInt32(periodSplitted[0]), Convert.ToInt32(periodSplitted[1]), Convert.ToInt32(periodSplitted[2]), Convert.ToInt32(periodSplitted[3]));
                    break;
                default:
                    Log.ErrorFormat("Period format {0} is not supported yet.", period);
                    break;
            }

            var timeVariable = function.Arguments[0];

            var firstTimeStep = (DateTime)timeVariable.MinValue;
            var lastTimeStep = (DateTime)timeVariable.MaxValue;
            var diffTimeSpan = lastTimeStep - firstTimeStep;

            if (periodSpan < diffTimeSpan)
            {
                Log.ErrorFormat("The timeserie {0} can not be imported with extrapolation type 'periodic'. Type of extrapolation has been set to 'None'.", function.Name);
                timeVariable.ExtrapolationType = ExtrapolationType.None;
                return;
            }

            if (periodSpan == diffTimeSpan)
            {
                timeVariable.ExtrapolationType = ExtrapolationType.Periodic;
                AddPeriodSpanAttributeToVariable(timeVariable, periodSpan);
                return;
            }

            if (periodSpan > diffTimeSpan)
            {
                var newDateTime = new DateTime(firstTimeStep.Ticks);
                newDateTime = newDateTime.Add(periodSpan);

                if (!function.Arguments.Any() || !function.Components.Any() || function.Components[0].Values.Count == 0)
                    return;

                function[newDateTime] = function.Components[0].Values[0];
                timeVariable.ExtrapolationType = ExtrapolationType.Periodic;
                AddPeriodSpanAttributeToVariable(timeVariable, periodSpan);
            }
        }

        private static void AddPeriodSpanAttributeToVariable(IVariable variable, TimeSpan periodSpan)
        {
            if (variable.Attributes.ContainsKey("PeriodSpan"))
            {
                variable.Attributes.Remove("PeriodSpan");
            }

            variable.Attributes.Add("PeriodSpan", periodSpan.ToString("c"));
        }
    }
}
