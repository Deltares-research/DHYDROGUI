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
    public class FlowBoundaryConditionPointData
    {
        public FlowBoundaryConditionPointData(FlowBoundaryCondition boundaryCondition, int supportPoint, bool useLayers)
        {
            BoundaryCondition = boundaryCondition;
            SupportPoint = supportPoint;
            UseLayers = useLayers;
        }

        public FlowBoundaryCondition BoundaryCondition { get; private set; }

        private int SupportPoint { get; set; }

        public IFunction Function
        {
            get { return BoundaryCondition == null ? null : BoundaryCondition.GetDataAtPoint(SupportPoint); }
        }

        public BoundaryConditionDataType ForcingType
        {
            get { return BoundaryCondition == null ? BoundaryConditionDataType.Empty : BoundaryCondition.DataType; }
        }
        
        public int ForcingTypeDimension
        {
            get
            {
                if (BoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    return BoundaryCondition.SedimentFractionNames.Count;
                switch (ForcingType)
                {
                    case BoundaryConditionDataType.Empty:
                        return 0;
                    case BoundaryConditionDataType.TimeSeries:
                    case BoundaryConditionDataType.Qh:
                        return 1;
                    case BoundaryConditionDataType.AstroComponents:
                    case BoundaryConditionDataType.Harmonics:
                        return 2;
                    case BoundaryConditionDataType.AstroCorrection:
                    case BoundaryConditionDataType.HarmonicCorrection:
                        return 4;
                    default:
                        throw new NotImplementedException("Forcing type unknown to flow module.");
                }
            }
        }

        public int VariableDimension
        {
            get { return BoundaryCondition == null ? 0 : BoundaryCondition.VariableDimension; }
        }

        public bool UseLayers { get; private set; }

        public IEnumerable<IVariable> FilterLayersAndComponents(int layer, int variableComponent)
        {
            if (Function == null) yield break;

            var startIndex = (variableComponent + layer*VariableDimension)*ForcingTypeDimension;

            for (var i = startIndex; i < startIndex + ForcingTypeDimension; i++)
            {
                yield return Function.Components[i];
            }
        }
    }

    public class FlowBoundaryConditionSeriesFactory
    {
        private static readonly Color[] signalColors = new[]
            {
                Color.Red, Color.LimeGreen, Color.Blue, Color.Violet, Color.Orange, Color.BlueViolet, Color.Aqua,
                Color.Gold, Color.DarkGreen, Color.Tomato
            };


        private static Color GetSignalColor(int i)
        {
            return signalColors[i % signalColors.Length];
        }

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

        public IEnumerable<IChartSeries> CreateSeries()
        {
            var i = 0;
            if (signalTimeSeries != null)
            {
                foreach (var component in signalTimeSeries.Components)
                {
                    yield return
                        MakeSeries(signalTimeSeries, component.DisplayName, DashStyle.Solid,
                                   GetSignalColor(i++));
                }
            }
            foreach (var backgroundfunction in backgroundTimeSeries)
            {
                if (backgroundfunction == null) continue;
                foreach (var component in backgroundfunction.Components)
                {
                    yield return
                        MakeSeries(backgroundfunction, component.DisplayName, DashStyle.Solid,
                                   GetSignalColor(i++));
                }
            }
        }
        
        public DateTime ModelStartTime { private get; set; }

        public DateTime ModelStopTime { private get; set; }

        public DateTime ModelReferenceTime { private get; set; }

        public IDictionary<string, double> AstroComponents { get; set; }

        public EventedList<FlowBoundaryConditionPointData> BackgroundFunctions
        {
            get { return backgroundFunctions; }
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
            get { return signalFunction; }
            set
            {
                signalFunction = value;
                EvaluateSignal();
            }
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
                    XAxisIsDateTime = function.Arguments[0].ValueType == typeof (DateTime),
                    DashStyle = style,
                    Color = color,
                    PointerColor = color
                };
        }

        private DateTime StartTime
        {
            get
            {
                if (SignalFunction != null && SignalFunction.ForcingType == BoundaryConditionDataType.TimeSeries)
                {
                    var timeValues = SignalFunction.Function.Arguments[0].Values;
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
                    var timeValues = SignalFunction.Function.Arguments[0].Values;
                    return timeValues.Count == 0 ? ModelStartTime : (DateTime) timeValues[timeValues.Count - 1];
                }
                return ModelStopTime;
            }
        }

        private void EvaluateBackgrounds()
        {
            backgroundTimeSeries.Clear();
            if (BackgroundFunctions == null) return;
            foreach (var flowFunctionWrapper in BackgroundFunctions)
            {
                backgroundTimeSeries.Add(CreateTimeSeries(new[] {flowFunctionWrapper}));
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
                : CreateTimeSeries(new[] {SignalFunction});
        }

        private void BackgroundFunctionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    backgroundTimeSeries.Insert(e.GetRemovedOrAddedIndex(),
                                                CreateTimeSeries(new[] { (FlowBoundaryConditionPointData)e.GetRemovedOrAddedItem() }));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    backgroundTimeSeries.RemoveAt(e.GetRemovedOrAddedIndex());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    backgroundTimeSeries[e.GetRemovedOrAddedIndex()] = CreateTimeSeries(new[] { (FlowBoundaryConditionPointData)e.GetRemovedOrAddedItem() });
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
            if (pointData == null) return null;

            var firstWrapper = pointData.FirstOrDefault(CanCreateTimeSeries);

            if (firstWrapper == null)
            {
                return null;
            }

            var name = firstWrapper.Function.Name;

            var unit = firstWrapper.Function.Components.First().Unit; //assertion: amplitude comes first.

            var times = ComputeSampleTimeStamps(pointData.Select(f => f.Function.Arguments.FirstOrDefault()),
                                                StartTime, StopTime);

            var function = new Function {Name = pointData.Count == 1 ? name : ("Total " + name)};
            function.Arguments.Add(new Variable<DateTime>("Time", new Unit("hours", "h")));
            function.Arguments[0].SetValues(times);

            if (firstWrapper.BoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
            {
                for (var i = 0; i < firstWrapper.VariableDimension; ++i)
                {
                    for(var comp= 0; comp < firstWrapper.Function.Components.Count; ++comp)
                    {
                        var values = Enumerable.Repeat((double)0, times.Count).ToList();
                        foreach (var condition in pointData)
                        {
                            if (!CanCreateTimeSeries(condition)) continue;

                            var summedComponents = condition.FilterLayersAndComponents(0, i).ToList();

                            summedComponents = new List<IVariable>() { summedComponents[comp] };
                            // only use zeroth layer for sum.
                            FillValues(condition.ForcingType, condition.Function.Arguments[0], summedComponents, times,
                                  values, condition.BoundaryCondition.Factor, condition.BoundaryCondition.Offset);
                        }
                        function.Components.Add(new Variable<double>(firstWrapper.Function.Components[comp].Name, unit));
                        function.Components.Last().SetValues(values);
                        function.Components.Last().NoDataValue = Double.NaN;
                    }
                }
                return function;
            }

            if (pointData.Count > 1) //summation mode
            {
                for (var i = 0; i < firstWrapper.VariableDimension; ++i)
                {
                    var values = Enumerable.Repeat((double) 0, times.Count).ToList();
                    foreach (var condition in pointData)
                    {
                        if (!CanCreateTimeSeries(condition)) continue;

                        var summedComponents = condition.FilterLayersAndComponents(0, i).ToList();
                        // only use zeroth layer for sum.

                        FillValues(condition.ForcingType, condition.Function.Arguments[0], summedComponents, times,
                                   values, condition.BoundaryCondition.Factor, condition.BoundaryCondition.Offset);
                    }

                    function.Components.Add(new Variable<double>(name, unit));
                    function.Components.Last().SetValues(values);
                    function.Components.Last().NoDataValue = Double.NaN;
                }
            }
            else
            {
                var numLayers = firstWrapper.UseLayers
                    ? firstWrapper.Function.Components.Count/
                      (firstWrapper.VariableDimension*firstWrapper.ForcingTypeDimension)
                    : 1;

                for (var i = 0; i < firstWrapper.VariableDimension; ++i)
                {
                    for (var j = 0; j < numLayers; ++j)
                    {
                        var values = Enumerable.Repeat((double) 0, times.Count).ToList();

                        var summedComponents = firstWrapper.FilterLayersAndComponents(j, i).ToList();

                        FillValues(firstWrapper.ForcingType, firstWrapper.Function.Arguments[0], summedComponents, times,
                                   values, firstWrapper.BoundaryCondition.Factor, firstWrapper.BoundaryCondition.Offset);

                        function.Components.Add(new Variable<double>(
                                                    numLayers == 1 ? name : name + "(" + (j + 1) + ")", unit));

                        function.Components.Last().SetValues(values);
                        function.Components.Last().NoDataValue = Double.NaN;
                    }
                }
            }

            return function;
        }

        private void FillValues(BoundaryConditionDataType forcingType, IVariable argument, IList<IVariable> components,
                                IList<DateTime> times, IList<double> values, double factor, double offset)
        {
            const double prefactor = Math.PI/180;

            switch (forcingType)
            {
                case BoundaryConditionDataType.TimeSeries:

                    for (var i = 0; i < times.Count; i++)
                    {
                        var time = times[i];
                        // no extrapolation: outside range time series are set to zero.
                        if (argument.ValueType == typeof (DateTime) && time >= (DateTime) argument.MinValue &&
                            time <= (DateTime) argument.MaxValue)
                        {
                            values[i] +=
                                (factor*
                                 components[0].Evaluate<double>(new VariableValueFilter<DateTime>(argument, time)) +
                                 offset);
                        }
                    }

                    break;

                case BoundaryConditionDataType.AstroComponents:

                    for (var i = 0; i < times.Count; i++)
                    {
                        var time = times[i];
                        double value = offset;
                        var timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            double frequency;
                            if (AstroComponents.TryGetValue((string) argument.Values[j], out frequency))
                            {
                                var amplitude = (double) components[0].Values[j]*factor;
                                var phase = ((double) components[1].Values[j] % 360.0);

                                if (frequency != 0)
                                {
                                    value += amplitude*Math.Cos(prefactor*(frequency*timeOffset.TotalHours - phase));
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
                        var time = times[i];
                        double value = offset;
                        var timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            double frequency;
                            if (AstroComponents.TryGetValue((string) argument.Values[j], out frequency))
                            {
                                var amplitude = (double) components[0].Values[j]*(double) components[2].Values[j]*factor;
                                var phase = ((double) components[1].Values[j] + (double) components[3].Values[j]) % 360.0;

                                if (frequency != 0)
                                {
                                    value += amplitude*Math.Cos(prefactor*(frequency*timeOffset.TotalHours - phase));
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
                        var time = times[i];
                        double value = offset;

                        var timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            var frequency = (double) argument.Values[j];
                            var amplitude = (double) components[0].Values[j]*factor;
                            var phase = (double) components[1].Values[j] % 360.0;
                            if (frequency != 0)
                            {
                                value += amplitude*Math.Cos(prefactor*(frequency*timeOffset.TotalHours - phase));
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
                        var time = times[i];
                        double value = offset;

                        var timeOffset = time - ModelReferenceTime;
                        for (var j = 0; j < argument.Values.Count; j++)
                        {
                            var frequency = (double) argument.Values[j];
                            var amplitude = (double) components[0].Values[j]*(double) components[2].Values[j]*factor;
                            var phase = ((double) components[1].Values[j] + (double) components[3].Values[j]) % 360.0;
                            if (frequency != 0)
                            {
                                value += amplitude*Math.Cos(prefactor*(frequency*timeOffset.TotalHours - phase));
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

            foreach (var backgroundVariable in backgroundVariables)
            {
                if (backgroundVariable.ValueType == typeof(DateTime))
                {
                    var dateTimes = backgroundVariable.Values.Cast<DateTime>().ToList();

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
                var timeSpan = stopTime - startTime;
                var samples = Math.Min(Math.Max((int)(4 * sampleFrequency * timeSpan.TotalHours), 500), 10000);
                var timeTicks = samples == 0 ? 0 : timeSpan.Ticks / samples;
                var dateTimes = Enumerable.Range(0, samples).Select(i => startTime + new TimeSpan(i * timeTicks));
                foreach (var dateTime in dateTimes)
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
                return 2 * Math.PI *
                       variable.Values.Cast<string>()
                               .Select(s => AstroComponents.ContainsKey(s) ? AstroComponents[s] : 0)
                               .Max() / 360;
            }
            if (variable.ValueType == typeof(double))
            {
                return 2 * Math.PI * variable.Values.Cast<double>().Max() / 360;
            }
            return 0;
        }
    }
}
