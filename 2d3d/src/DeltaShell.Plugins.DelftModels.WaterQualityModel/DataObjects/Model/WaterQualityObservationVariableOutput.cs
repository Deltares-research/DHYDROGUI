using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Water quality observation variable output
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualityObservationVariableOutput : Unique<long>, INameable, ICloneable
    {
        private readonly IList<TimeSeries> timeSeriesList = new List<TimeSeries>();
        private string name;

        [Obsolete("Implemented for NHibernate only")]
        public WaterQualityObservationVariableOutput() : this(
            Enumerable.Empty<DelftTools.Utils.Tuple<string, string>>()) {}

        /// <summary>
        /// Creates an observation variable output object according to the provided <paramref name="outputVariableTuples"/>:
        /// a time series will be added to <see cref="TimeSeriesList"/> for each element in
        /// <paramref name="outputVariableTuples"/>
        /// </summary>
        /// <param name="outputVariableTuples"> An enumerable of output variable name/unit tuples </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when
        /// <param name="outputVariableTuples"/>
        /// is null
        /// </exception>
        public WaterQualityObservationVariableOutput(
            IEnumerable<DelftTools.Utils.Tuple<string, string>> outputVariableTuples)
        {
            if (outputVariableTuples == null)
            {
                throw new ArgumentNullException(nameof(outputVariableTuples));
            }

            foreach (DelftTools.Utils.Tuple<string, string> outputVariableTuple in outputVariableTuples)
            {
                AddTimeSeries(outputVariableTuple);
            }
        }

        /// <summary>
        /// The observation variable
        /// </summary>
        /// <remarks> Might be null (in case of surface water types) </remarks>
        public IFeature ObservationVariable { get; set; }

        /// <summary>
        /// List of time series for the observation variable
        /// </summary>
        public IEnumerable<TimeSeries> TimeSeriesList => timeSeriesList;

        /// <summary>
        /// The name of the observation variable
        /// </summary>
        public string Name
        {
            get => ObservationVariable != null && ObservationVariable is INameable
                       ? ((INameable) ObservationVariable).Name
                       : name;
            set => name = value;
        }

        /// <summary>
        /// Adds a time series to <see cref="TimeSeriesList"/> for
        /// <param name="outputVariableTuple"/>
        /// </summary>
        /// <param name="outputVariableTuple"> A tuple of output variable name (1st) and output variable unit (2nd) </param>
        /// <param name="insertIndex"> The index to insert the output variable time series at </param>
        public void AddTimeSeries(DelftTools.Utils.Tuple<string, string> outputVariableTuple, int insertIndex = -1)
        {
            if (timeSeriesList.Any(ts => ts.Name == outputVariableTuple.First))
            {
                return; // Prevent duplication of time series (based on name)
            }

            var timeSeries = new TimeSeries
            {
                Name = outputVariableTuple.First,
                IsEditable = false
            };

            timeSeries.Components.Add(new Variable<double>(outputVariableTuple.First) {Unit = new Unit(outputVariableTuple.Second, outputVariableTuple.Second)});

            if (insertIndex < 0 || insertIndex > timeSeriesList.Count)
            {
                timeSeriesList.Add(timeSeries);
            }
            else
            {
                timeSeriesList.Insert(insertIndex, timeSeries);
            }
        }

        /// <summary>
        /// Removes the time series with name
        /// <param name="outputVariableName"/>
        /// from <see cref="TimeSeriesList"/>
        /// </summary>
        /// <param name="outputVariableName"> The name of the output variable to remove the time series for </param>
        public void RemoveTimeSeries(string outputVariableName)
        {
            TimeSeries timeSeries = timeSeriesList.FirstOrDefault(ts => ts.Name.Equals(outputVariableName));
            if (timeSeries != null)
            {
                timeSeriesList.Remove(timeSeries);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            var clone =
                new WaterQualityObservationVariableOutput(Enumerable.Empty<DelftTools.Utils.Tuple<string, string>>())
                {
                    name = name,
                    ObservationVariable = ObservationVariable
                };

            TimeSeriesList.Select(ts => (TimeSeries) ts.Clone()).ForEach(clone.timeSeriesList.Add);

            return clone;
        }
    }
}