using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FlowBoundaryConditionSeriesFactory
    {
        private static readonly Color[] signalColors = new[]
        {
            Color.Red,
            Color.LimeGreen,
            Color.Blue,
            Color.Violet,
            Color.Orange,
            Color.BlueViolet,
            Color.Aqua,
            Color.Gold,
            Color.DarkGreen,
            Color.Tomato
        };

        private readonly List<IFunction> backgroundTimeSeries;
        private IFunction signalTimeSeries;
        private EventedList<FlowBoundaryConditionPointData> backgroundFunctions;
        private FlowBoundaryConditionPointData signalFunction;

        public FlowBoundaryConditionSeriesFactory()
        {
            ModelStartTime = DateTime.MinValue;
            ModelStopTime = DateTime.MaxValue;
            ModelReferenceTime = DateTime.MinValue;
            backgroundTimeSeries = new List<IFunction>();
            BackgroundFunctions = new EventedList<FlowBoundaryConditionPointData>();
            signalTimeSeries = null;
        }

        public DateTime ModelStartTime { private get; set; }

        public DateTime ModelStopTime { private get; set; }

        public DateTime ModelReferenceTime { private get; set; }

        public IDictionary<string, double> AstroComponents { get; set; }

        public EventedList<FlowBoundaryConditionPointData> BackgroundFunctions
        {
            get
            {
                return backgroundFunctions;
            }
            set
            {
                if (backgroundFunctions != null)
                {
                    backgroundFunctions.CollectionChanged -= BackgroundFunctionsCollectionChanged;
                }

                backgroundFunctions = value;
                EvaluateBackgrounds();
                if (backgroundFunctions != null)
                {
                    backgroundFunctions.CollectionChanged += BackgroundFunctionsCollectionChanged;
                }
            }
        }

        public FlowBoundaryConditionPointData SignalFunction
        {
            get
            {
                return signalFunction;
            }
            set
            {
                signalFunction = value;
                EvaluateSignal();
            }
        }

        public IEnumerable<IChartSeries> CreateSeries()
        {
            var i = 0;
            if (signalTimeSeries != null)
            {
                foreach (IVariable component in signalTimeSeries.Components)
                {
                    yield return
                        MakeSeries(signalTimeSeries, component.DisplayName, DashStyle.Solid,
                                   GetSignalColor(i++));
                }
            }

            foreach (IFunction backgroundfunction in backgroundTimeSeries)
            {
                if (backgroundfunction == null)
                {
                    continue;
                }

                foreach (IVariable component in backgroundfunction.Components)
                {
                    yield return
                        MakeSeries(backgroundfunction, component.DisplayName, DashStyle.Solid,
                                   GetSignalColor(i++));
                }
            }
        }

        public void EvaluateSignal()
        {
            if (SignalFunction == null || SignalFunction.Function == null)
            {
                signalTimeSeries = null;
                return;
            }

            signalTimeSeries = SignalFunction.ForcingType == BoundaryConditionDataType.Qh
                                   ? SignalFunction.Function
                                   : CreateTimeSeries(new[]
                                   {
                                       SignalFunction
                                   });
        }

        private DateTime StartTime
        {
            get
            {
                if (SignalFunction != null && SignalFunction.ForcingType == BoundaryConditionDataType.TimeSeries)
                {
                    IMultiDimensionalArray timeValues = SignalFunction.Function.Arguments[0].Values;
                    return timeValues.Count == 0 ? ModelStartTime : (DateTime) timeValues[0];
                }

                return ModelStartTime;
            }
        }

        private DateTime StopTime
        {
            get
            {
                if (SignalFunction != null && SignalFunction.ForcingType == BoundaryConditionDataType.TimeSeries)
                {
                    IMultiDimensionalArray timeValues = SignalFunction.Function.Arguments[0].Values;
                    return timeValues.Count == 0 ? ModelStartTime : (DateTime) timeValues[timeValues.Count - 1];
                }

                return ModelStopTime;
            }
        }

        private static Color GetSignalColor(int i)
        {
            return signalColors[i % signalColors.Length];
        }

        private LineChartSeries MakeSeries(IFunction function, string componentName, DashStyle style, Color color)
        {
            return new LineChartSeries
            {
                ChartSeriesInterpolationType = ChartSeriesInterpolationType.Linear,
                XValuesDataMember = function.Arguments[0].DisplayName,
                YValuesDataMember = componentName,
                Title = componentName,
                DefaultNullValue = (double) function.Components[0].DefaultValue,
                PointerSize = 3,
                PointerVisible = false,
                DataSource = new FunctionBindingList(function),
                UpdateASynchronously = true,
                XAxisIsDateTime = function.Arguments[0].ValueType == typeof(DateTime),
                DashStyle = style,
                Color = color,
                PointerColor = color
            };
        }

        private void EvaluateBackgrounds()
        {
            backgroundTimeSeries.Clear();
            if (BackgroundFunctions == null)
            {
                return;
            }

            foreach (FlowBoundaryConditionPointData flowFunctionWrapper in BackgroundFunctions)
            {
                backgroundTimeSeries.Add(CreateTimeSeries(new[]
                {
                    flowFunctionWrapper
                }));
            }
        }

        private void BackgroundFunctionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    backgroundTimeSeries.Insert(removedOrAddedIndex,
                                                CreateTimeSeries(new[]
                                                {
                                                    (FlowBoundaryConditionPointData) removedOrAddedItem
                                                }));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    backgroundTimeSeries.RemoveAt(removedOrAddedIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    backgroundTimeSeries[removedOrAddedIndex] = CreateTimeSeries(new[]
                    {
                        (FlowBoundaryConditionPointData) removedOrAddedItem
                    });
                    break;
                case NotifyCollectionChangedAction.Reset:
                    backgroundTimeSeries.Clear();
                    break;
            }
        }

        private static bool CanCreateTimeSeries(FlowBoundaryConditionPointData flowFunctionWrapper)
        {
            return flowFunctionWrapper.ForcingType == BoundaryConditionDataType.TimeSeries ||
                   flowFunctionWrapper.ForcingType == BoundaryConditionDataType.AstroComponents ||
                   flowFunctionWrapper.ForcingType == BoundaryConditionDataType.AstroCorrection ||
                   flowFunctionWrapper.ForcingType == BoundaryConditionDataType.Harmonics ||
                   flowFunctionWrapper.ForcingType == BoundaryConditionDataType.HarmonicCorrection;
        }

        private IFunction CreateTimeSeries(IList<FlowBoundaryConditionPointData> pointData)
        {
            FlowBoundaryConditionPointData firstWrapper = pointData?.FirstOrDefault(CanCreateTimeSeries);
            if (firstWrapper == null)
            {
                return null;
            }

            string name = firstWrapper.Function.Name;
            IUnit unit = firstWrapper.Function.Components.First().Unit; //assertion: amplitude comes first.
            IList<DateTime> times =
                ComputeSampleTimeStamps(pointData.Select(f => f.Function.Arguments.FirstOrDefault()), StartTime,
                                        StopTime);

            var function = new Function {Name = pointData.Count == 1 ? name : "Total " + name};
            function.Arguments.Add(new Variable<DateTime>("Time", new Unit("hours", "h")));
            function.Arguments[0].SetValues(times);

            if (firstWrapper.BoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
            {
                for (var variableDimension = 0; variableDimension < firstWrapper.VariableDimension; ++variableDimension)
                {
                    for (var functionComponents = 0; functionComponents < firstWrapper.Function.Components.Count; ++functionComponents)
                    {
                        List<double> values = Enumerable.Repeat((double) 0, times.Count).ToList();
                        foreach (FlowBoundaryConditionPointData condition in pointData)
                        {
                            if (!CanCreateTimeSeries(condition))
                            {
                                continue;
                            }

                            List<IVariable> summedComponents = condition.FilterLayersAndComponents(0, variableDimension).ToList();
                            int totalSummedComponents = summedComponents.Count;
                            if (functionComponents < totalSummedComponents)
                            {
                                summedComponents = new List<IVariable>() {summedComponents[functionComponents]};
                                // only use zeroth layer for sum.
                                FillValues(condition.ForcingType, condition.Function.Arguments[0], summedComponents, times,
                                           values, condition.BoundaryCondition.Factor, condition.BoundaryCondition.Offset);
                            }
                        }

                        function.Components.Add(new Variable<double>(firstWrapper.Function.Components[functionComponents].Name, unit));
                        function.Components.Last().SetValues(values);
                        function.Components.Last().NoDataValue = double.NaN;
                    }
                }

                return function;
            }

            if (pointData.Count > 1) //summation mode
            {
                for (var i = 0; i < firstWrapper.VariableDimension; ++i)
                {
                    List<double> values = Enumerable.Repeat((double) 0, times.Count).ToList();
                    foreach (FlowBoundaryConditionPointData condition in pointData)
                    {
                        if (!CanCreateTimeSeries(condition))
                        {
                            continue;
                        }

                        List<IVariable> summedComponents = condition.FilterLayersAndComponents(0, i).ToList();
                        // only use zeroth layer for sum.

                        FillValues(condition.ForcingType, condition.Function.Arguments[0], summedComponents, times,
                                   values, condition.BoundaryCondition.Factor, condition.BoundaryCondition.Offset);
                    }

                    function.Components.Add(new Variable<double>(name, unit));
                    function.Components.Last().SetValues(values);
                    function.Components.Last().NoDataValue = double.NaN;
                }
            }
            else
            {
                int numLayers = firstWrapper.UseLayers
                                    ? firstWrapper.Function.Components.Count /
                                      (firstWrapper.VariableDimension * firstWrapper.ForcingTypeDimension)
                                    : 1;

                for (var i = 0; i < firstWrapper.VariableDimension; ++i)
                {
                    for (var j = 0; j < numLayers; ++j)
                    {
                        List<double> values = Enumerable.Repeat((double) 0, times.Count).ToList();

                        List<IVariable> summedComponents = firstWrapper.FilterLayersAndComponents(j, i).ToList();

                        FillValues(firstWrapper.ForcingType, firstWrapper.Function.Arguments[0], summedComponents, times,
                                   values, firstWrapper.BoundaryCondition.Factor, firstWrapper.BoundaryCondition.Offset);

                        function.Components.Add(new Variable<double>(
                                                    numLayers == 1 ? name : name + "(" + (j + 1) + ")", unit));

                        function.Components.Last().SetValues(values);
                        function.Components.Last().NoDataValue = double.NaN;
                    }
                }
            }

            return function;
        }

        private void FillValues(BoundaryConditionDataType forcingType, IVariable argument, IList<IVariable> components,
                                IList<DateTime> times, IList<double> values, double factor, double offset)
        {
            const double prefactor = Math.PI / 180;

            switch (forcingType)
            {
                case BoundaryConditionDataType.TimeSeries:

                    for (var i = 0; i < times.Count; i++)
                    {
                        DateTime time = times[i];
                        // no extrapolation: outside range time series are set to zero.
                        if (argument.ValueType == typeof(DateTime) && time >= (DateTime) argument.MinValue &&
                            time <= (DateTime) argument.MaxValue)
                        {
                            values[i] +=
                                (factor *
                                 components[0].Evaluate<double>(new VariableValueFilter<DateTime>(argument, time))) +
                                offset;
                        }
                    }

                    break;

                case BoundaryConditionDataType.AstroComponents:

                    for (var i = 0; i < times.Count; i++)
                    {
                        DateTime time = times[i];
                        double value = offset;
                        TimeSpan timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            double frequency;
                            if (AstroComponents.TryGetValue((string) argument.Values[j], out frequency))
                            {
                                double amplitude = (double) components[0].Values[j] * factor;
                                double phase = (double) components[1].Values[j] % 360.0;

                                if (frequency != 0)
                                {
                                    value += amplitude * Math.Cos(prefactor * ((frequency * timeOffset.TotalHours) - phase));
                                }
                                else
                                {
                                    value += amplitude;
                                }
                            }
                        }

                        values[i] += value;
                    }

                    break;

                case BoundaryConditionDataType.AstroCorrection:

                    for (var i = 0; i < times.Count; i++)
                    {
                        DateTime time = times[i];
                        double value = offset;
                        TimeSpan timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            double frequency;
                            if (AstroComponents.TryGetValue((string) argument.Values[j], out frequency))
                            {
                                double amplitude = (double) components[0].Values[j] * (double) components[2].Values[j] * factor;
                                double phase = ((double) components[1].Values[j] + (double) components[3].Values[j]) % 360.0;

                                if (frequency != 0)
                                {
                                    value += amplitude * Math.Cos(prefactor * ((frequency * timeOffset.TotalHours) - phase));
                                }
                                else
                                {
                                    value += amplitude;
                                }
                            }
                        }

                        values[i] += value;
                    }

                    break;

                case BoundaryConditionDataType.Harmonics:

                    for (var i = 0; i < times.Count; i++)
                    {
                        DateTime time = times[i];
                        double value = offset;

                        TimeSpan timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            var frequency = (double) argument.Values[j];
                            double amplitude = (double) components[0].Values[j] * factor;
                            double phase = (double) components[1].Values[j] % 360.0;
                            if (frequency != 0)
                            {
                                value += amplitude * Math.Cos(prefactor * ((frequency * timeOffset.TotalHours) - phase));
                            }
                            else
                            {
                                value += amplitude;
                            }
                        }

                        values[i] += value;
                    }

                    break;

                case BoundaryConditionDataType.HarmonicCorrection:

                    for (var i = 0; i < times.Count; i++)
                    {
                        DateTime time = times[i];
                        double value = offset;

                        TimeSpan timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            var frequency = (double) argument.Values[j];
                            double amplitude = (double) components[0].Values[j] * (double) components[2].Values[j] * factor;
                            double phase = ((double) components[1].Values[j] + (double) components[3].Values[j]) % 360.0;
                            if (frequency != 0)
                            {
                                value += amplitude * Math.Cos(prefactor * ((frequency * timeOffset.TotalHours) - phase));
                            }
                            else
                            {
                                value += amplitude;
                            }
                        }

                        values[i] += value;
                    }

                    break;

                default:

                    throw new NotImplementedException("Forcing type not recognized in sum of boundary conditions.");
            }
        }

        private IList<DateTime> ComputeSampleTimeStamps(IEnumerable<IVariable> backgroundVariables, DateTime startTime, DateTime stopTime)
        {
            var result = new SortedSet<DateTime>();
            double sampleFrequency = 0;

            foreach (IVariable backgroundVariable in backgroundVariables)
            {
                if (backgroundVariable.ValueType == typeof(DateTime))
                {
                    List<DateTime> dateTimes = backgroundVariable.Values.Cast<DateTime>().ToList();

                    dateTimes.SkipWhile(d => d < startTime)
                             .TakeWhile(d => d <= stopTime)
                             .ForEach(d => result.Add(d));

                    if (result.Count < 2 && backgroundVariable.Values.Count > 2)
                    {
                        if (dateTimes.Any(d => d < startTime))
                        {
                            result.Add(dateTimes.Last(d => d < startTime));
                        }

                        if (dateTimes.Any(d => d > stopTime))
                        {
                            result.Add(dateTimes.FirstOrDefault(d => d > stopTime));
                        }
                    }
                }
                else
                {
                    sampleFrequency = Math.Max(sampleFrequency, MaximalFrequency(backgroundVariable));
                }
            }

            if (sampleFrequency > 0)
            {
                TimeSpan timeSpan = stopTime - startTime;
                int samples = Math.Min(Math.Max((int) (4 * sampleFrequency * timeSpan.TotalHours), 500), 10000);
                long timeTicks = samples == 0 ? 0 : timeSpan.Ticks / samples;
                IEnumerable<DateTime> dateTimes = Enumerable.Range(0, samples).Select(i => startTime + new TimeSpan(i * timeTicks));
                foreach (DateTime dateTime in dateTimes)
                {
                    result.Add(dateTime);
                }
            }
            else
            {
                result.Add(startTime);
                result.Add(stopTime);
            }

            return result.ToList();
        }

        private double MaximalFrequency(IVariable variable)
        {
            if (variable.Values.Count == 0)
            {
                return 0;
            }

            if (variable.ValueType == typeof(string))
            {
                return (2 * Math.PI *
                        variable.Values.Cast<string>()
                                .Select(s => AstroComponents.ContainsKey(s) ? AstroComponents[s] : 0)
                                .Max()) / 360;
            }

            if (variable.ValueType == typeof(double))
            {
                return (2 * Math.PI * variable.Values.Cast<double>().Max()) / 360;
            }

            return 0;
        }
    }
}