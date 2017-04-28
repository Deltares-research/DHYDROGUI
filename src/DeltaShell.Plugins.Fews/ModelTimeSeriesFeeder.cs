using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using DelftTools.Units;
using Deltares.IO.FewsPI;
using TimeSeries = Deltares.IO.FewsPI.TimeSeries;

namespace DeltaShell.Plugins.Fews
{
    public class ModelTimeSeriesFeeder
    {
        private readonly List<Exception> errors;
        private readonly List<string> warnings;
        private readonly List<string> debugInfos;

        public ModelTimeSeriesFeeder()
        {
            errors = new List<Exception>();
            warnings = new List<string>();
            debugInfos = new List<string>();
        }

        /// <summary>
        /// Gets or sets the global start date time (will be used to filter the feeding)
        /// </summary>
        public DateTime GlobalStartDateTime { private get; set; }

        /// <summary>
        /// Gets or sets the global end date time (will be used to filter the feeding)
        /// </summary>
        public DateTime GlobalEndDateTime { private get; set; }

        /// <summary>
        /// Sets the input time series collection
        /// </summary>
        public IEnumerable<TimeSeries> SourceCollection { private get; set; }
        
        /// <summary>
        /// Sets the context of the model (to search in)
        /// </summary>
        public IExtendedQueryContext Context { private get; set; }

        /// <summary>
        /// Gets the list of possible errors
        /// </summary>
        public IEnumerable<Exception> Errors { get { return errors; } }

        /// <summary>
        /// Gets a list of possible warnings, that were gathered by feeding the model
        /// </summary>
        public IEnumerable<string> Warnings { get { return warnings; } }

        
        /// <summary>
        /// Gets a list of debug info messages, that were gathered by feeding the model
        /// </summary>
        public IEnumerable<string> DebugInfos { get { return debugInfos; } }

        /// <summary>
        /// Gets a value indicating that there were errors while feeding the model
        /// </summary>
        public bool HasErrors
        {
            get { return errors.Count > 0; }
        }

        /// <summary>
        /// Feeds the target time series with the data from the source collection
        /// </summary>
        public void FeedModel(bool checkBackwardCompatibility)
        {
            Context.CacheResults = true;
            try
            {
                foreach (TimeSeries timeSeriesFromFews in SourceCollection)
                {
                    string locationId = timeSeriesFromFews.LocationId;
                    string parameterId = timeSeriesFromFews.ParameterId;

                    // find deltashell time series  
                    AggregationResult aggregationResult;
                    try
                    {
                        aggregationResult = Context.GetSingleInputTimeSeries(parameterId, locationId);
                    }
                    catch (Exception e)
                    {
                        var errorMessage = string.Format("There are multiple items found matching locationId: '{0}' and parameterId: '{1}'. ",
                                        locationId, parameterId);
                        errorMessage += Environment.NewLine + string.Format("Exception message related to previous error: {0}", e.Message);
                        AddError(new InvalidOperationException(errorMessage, e));
                        continue;
                    }

                    if ((aggregationResult == null || aggregationResult.TimeSeries == null) && checkBackwardCompatibility)
                    {
                        string oldParameterId = null;
                        if (parameterId.Equals(FunctionAttributes.StandardNames.WaterDischarge))
                        {
                            oldParameterId = "flow time series";
                        }
                        if (parameterId.Equals(FunctionAttributes.StandardNames.WaterLevel))
                        {
                            oldParameterId = "water level time series";
                        }
                        if (oldParameterId != null)
                        {
                            aggregationResult = Context.GetSingleInputTimeSeries(oldParameterId, locationId);
                        }
                    }

                    if (aggregationResult == null || aggregationResult.TimeSeries == null)
                    {
                        var errorMessage = string.Format("There are no input time series found for locationId: '{0}' and parameterId: '{1}'",
                            locationId, parameterId);
                        AddError(new InvalidOperationException(errorMessage));
                        continue;
                    }

                    // set input values 
                    IFunction modelTimeSeries = aggregationResult.TimeSeries;
                    modelTimeSeries.Clear();
                    var component = modelTimeSeries.Components.FirstOrDefault();
                    if (component == null)
                    {
                        var errorMessage = string.Format(
                            "There is a problem getting the time series component value for locationId: '{0}' and parameterId: '{1}'", locationId,
                            parameterId);

                        AddError(new InvalidOperationException(errorMessage));
                        continue;
                    }

                    // check for equal units
                    if (!IsSameUnit(component.Unit, new Unit(timeSeriesFromFews.Unit, timeSeriesFromFews.Unit)))
                    {
                        var warningMessage = string.Format(
                            "The component variable for locationId: '{0}' and parameterId: '{1}' have different units then the unit used in the input time series. " +
                            "Flow 1D unit: {2}, Fews unit: {3} ", locationId, parameterId, component.Unit, timeSeriesFromFews.Unit);
                        AddWarning(warningMessage);
                    }

                    // sets the missing value on the target time series
                    // TODO: check (the model should be one that defines it's no data value)
                    // component.NoDataValue = fewsTimeSeries.MissingValue;
                    // update the time step summary table to determine the minimal (and maybe the max) delta time step
                    // for all time series
                    // UpdateTimeStepSummary(fewsTimeSeries.TimeStep);
                    AddDebugInfo(string.Format("Feeding timeseries for locationId '{0}' and parameterId '{1}' to model", locationId, parameterId));

                    foreach (TimeEvent timeEvent in timeSeriesFromFews.Events)
                    {
                        DateTime timeEventKey = timeEvent.Time;
                        if (typeof(bool) == component.ValueType)
                        {
                            modelTimeSeries[timeEventKey] = Math.Abs(timeEvent.Value - 0.0) > 1.0e-5;
                        }
                        else
                        {
                            modelTimeSeries[timeEventKey] = timeEvent.Value;
                        }
                    }
                }
            }
            finally
            {
                Context.CacheResults = false;
            }
        }

        private static bool IsSameUnit(IUnit left, IUnit right)
        {
            if (left != null && right != null)
            {
                return left.Symbol == right.Symbol;
            }
            return true;
        }

        private void AddDebugInfo(string message)
        {
            debugInfos.Add(message);
        }

        private void AddWarning(string message)
        {
            warnings.Add(message);
        }

        private void AddError(Exception exception)
        {
            errors.Add(exception);
        }
    }

}