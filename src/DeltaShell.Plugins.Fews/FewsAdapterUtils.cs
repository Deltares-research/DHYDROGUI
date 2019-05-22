using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DeltaShell.Plugins.Fews.Queries;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.Fews
{
    using DelftTools.Shell.Core.Workflow.DataItems;

    public static class FewsAdapterUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FewsAdapterUtils));
        private static IDiscretization computationalGrid;

        /// <summary>
        /// Gets the first output time series that matches the location id and parameter id
        /// </summary>
        /// <param name="project">The project to get the input data items from</param>
        /// <param name="locationId"></param>
        /// <param name="parameterId"></param>
        /// <returns></returns>
        public static IFunction GetInputTimeSeries(this Project project, string locationId, string parameterId)
        {
            if (project == null) throw new ArgumentNullException("project");

            IEnumerable<IDataItem> dataItems = project.GetAllItemsRecursive()
                .OfType<IDataItem>();

            return GetInputTimeSeries(dataItems, locationId, parameterId);
        }

        /// <summary>
        /// Gets the first output time series that matches the location id and parameter id
        /// </summary>
        /// <param name="dataItems"></param>
        /// <param name="locationId"></param>
        /// <param name="parameterId"></param>
        /// <returns></returns>
        public static IFunction GetInputTimeSeries(this IEnumerable<IDataItem> dataItems, string locationId,
                                                   string parameterId)
        {
            if (dataItems == null) throw new ArgumentNullException("dataItems");

            // filter out all roles except for input
            IEnumerable<IDataItem> inputDataItems =
                dataItems.Where(di => di.Role == DataItemRole.Input || di.Role == DataItemRole.None);

            // For use of search networkcoverage by location
            computationalGrid = GetComputationalGrid(dataItems);

            return GetTimeSeries(inputDataItems, locationId, parameterId);
        }


        /// <summary>
        /// Gets the first input time series that matches the location id and parameter id
        /// </summary>
        /// <param name="project">The project to get the output data items from</param>
        /// <param name="locationId"></param>
        /// <param name="parameterId"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<DateTime, double>> GetOutputTimeSeries(this Project project, string locationId,
                                                                               string parameterId)
        {
            if (project == null) throw new ArgumentNullException("project");

            IEnumerable<IDataItem> dataItems = project.GetAllItemsRecursive().OfType<IDataItem>();

            return dataItems.GetOutputTimeSeries(locationId, parameterId);
        }

        /// <summary>
        /// Gets time series based on location and parameter id from the given data items
        /// </summary>
        /// <param name="dataItems">The data items to inspect for time series data</param>
        /// <param name="locationId">The location of the object</param>
        /// <param name="parameterId">The parameter name to get the time series for</param>
        /// <returns>A sequence of tuples containing time steps and values only</returns>
        public static IEnumerable<Tuple<DateTime, double>> GetOutputTimeSeries(this IEnumerable<IDataItem> dataItems,
                                                                               string locationId, string parameterId)
        {
            if (dataItems == null) throw new ArgumentNullException("dataItems");
            if (string.IsNullOrEmpty(locationId)) throw new ArgumentException("locationId");
            if (string.IsNullOrEmpty(parameterId)) throw new ArgumentException("parameterId");

            // For use of search networkcoverage by location 
            computationalGrid = GetComputationalGrid(dataItems);

            // filter out all roles except for output
            IEnumerable<IDataItem> outputDataItems =
                dataItems.Where(di => di.Role == DataItemRole.Output || di.Role == DataItemRole.None);

            IFunction timeSeries = GetTimeSeries(outputDataItems, locationId, parameterId);
            if (timeSeries != null)
            {
                //
                // Get Feature Coverage time series data

                var featureCoverage = timeSeries as IFeatureCoverage;
                if (featureCoverage != null)
                {
                    foreach (var tuple in ToTimeValueTuples(featureCoverage, locationId))
                        yield return tuple;
                    yield break;
                }

                //
                // Get Network Coverage time series data

                var networkCoverage = timeSeries as INetworkCoverage;
                if (networkCoverage != null)
                {
                    if (networkCoverage.Name == parameterId)
                    {
                        foreach (var tuple in ToTimeValueTuples(networkCoverage, locationId))
                            yield return tuple;
                        yield break;
                    }
                }
                
                //
                // Find timeseries within boundary and lateral source objects

                var featureData = timeSeries as IFeatureData;
                if (featureData != null)
                {
                    foreach (var tuple in ToTimeValueTuples(featureData))
                        yield return tuple;

                    yield break;
                }
            }
        }

        /// <summary>
        /// computational grid (discretization) contains the networklocation by name
        /// this information is needed to find locations by name in other networkcoverages
        /// </summary>
        /// <param name="dataItems"></param>
        /// <returns></returns>
        private static IDiscretization GetComputationalGrid(IEnumerable<IDataItem> dataItems)
        {
            return (from item in dataItems
                    where item.ValueType == typeof (Discretization)
                    select item.Value).OfType<IDiscretization>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first time serie found in the data item list using the location id and parameter id
        /// </summary>
        /// <param name="dataItems">The data items to evaluate</param>
        /// <param name="locationId">The location argument (required)</param>
        /// <param name="parameterId">The paramater argument (required)</param>
        /// <returns>A function object</returns>
        /// <exception cref="InvalidOperationException">When an item is found, matching the location id and paramter id, and it is not time dependent an invalid operation is thrown</exception>
        internal static IFunction GetTimeSeries(this IEnumerable<IDataItem> dataItems, string locationId,
                                                string parameterId)
        {
            if (dataItems == null) throw new ArgumentNullException("dataItems");
            if (string.IsNullOrEmpty(locationId)) throw new ArgumentException("locationId");
            if (string.IsNullOrEmpty(parameterId)) throw new ArgumentException("parameterId");

            string name = string.Concat(locationId, ".", parameterId);
            foreach (IDataItem dataItem in dataItems)
            {
                var dataItemFunction = dataItem.Value as Function;
                if (dataItemFunction != null)
                {
                    if (dataItem.Name == name)
                    {
                        return dataItem.Value as IFunction;
                    }

                    if (dataItem.IsLinked)
                    {
                        if (dataItem.LinkedTo.Name == name)
                        {
                            return dataItem.Value as IFunction;
                        }
                    }
                }

                //
                // Get Feature Coverage time series data

                var featureCoverage = dataItem.Value as IFeatureCoverage;
                if (featureCoverage != null && featureCoverage.Name == parameterId &&
                    featureCoverage.Features.OfType<IBranchFeature>().Any(f => f.Name == locationId))
                {
                    if (!featureCoverage.IsTimeDependent)
                        throw new InvalidOperationException(
                            string.Format(
                                "Found an object with the given locationId '{0}' and parameterId '{1}' but this is not a time series",
                                locationId, parameterId));

                    return featureCoverage;
                }

                //
                // Get Network Coverage time series data

                var networkCoverage = dataItem.Value as INetworkCoverage;
                if (networkCoverage != null && networkCoverage.Name == parameterId)
                {
                    NetworkLocation location = GetNetworkLocation(locationId, networkCoverage, computationalGrid);

                    if (location != null)
                    {
                        if (!networkCoverage.IsTimeDependent)
                            throw new InvalidOperationException(
                                string.Format(
                                    "Found an object with the given locationId '{0}' and parameterId '{1}' but this is not a time series",
                                    locationId, parameterId));

                        return networkCoverage;
                    }
                }

                //
                // Find timeseries within boundary and lateral source objects
                var featureData = dataItem.Value as IFeatureData;
                if (featureData != null)
                {
                    if (featureData.Feature != null)
                    {
                        if (((INameable) featureData.Feature).Name == locationId)
                        {
                            if (((Function) featureData.Data).Name == parameterId)
                            {
                                return ((Function) (featureData.Data));
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static NetworkLocation GetNetworkLocation(string locationId, INetworkCoverage networkCoverage,
                                                          INetworkCoverage grid)
        {
            var location = (NetworkLocation) networkCoverage.Locations.Values.FirstOrDefault(l => l.Name == locationId);
            if (location == null && grid != null)
            {
                location = (NetworkLocation) grid.Locations.Values.FirstOrDefault(l => l.Name == locationId);
            }

            return location ?? (location = GetLocationByBranchOffsetValues(networkCoverage, locationId));
        }

        /// <summary>
        /// some timeseries are located on stagered grid 
        /// to avoid empty timeseries for these locations, values are interpolated
        /// </summary>
        /// <param name="networkCoverage"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private static IFunction GetFunctionForNetworkLocation(INetworkCoverage networkCoverage,
                                                               NetworkLocation location)
        {
            IFunction function = new TimeSeries();
            function.Components.Add(new Variable<double>(networkCoverage.Name));

            var values = new double[networkCoverage.Time.Values.Count];
            int index = 0;
            foreach (DateTime time in networkCoverage.Time.Values)
            {
                values[index++] = networkCoverage.Evaluate(time, location);
            }
            (function as TimeSeries).Time.SetValues(networkCoverage.Time.Values);
            function.Components[0].SetValues(values);
            return function;
        }

        private static NetworkLocation GetLocationByBranchOffsetValues(INetworkCoverage networkCoverage,
                                                                       string locationId)
        {
            double offset = 0;
            string branchName = "";
            var stringSeparators = new[] {";"};
            string[] results = locationId.Split(stringSeparators, StringSplitOptions.None);
            foreach (string result in results)
            {
                double chnainage;
                if (double.TryParse(result, out chnainage))
                {
                    offset = chnainage;
                }
                else
                {
                    branchName = result;
                }
            }
            IBranch branch = networkCoverage.Network.Branches.FirstOrDefault(b => b.Name == branchName);
            var location = new NetworkLocation {Branch = branch, Offset = offset};
            return location;
        }

        /// <summary>
        /// Transforms the IFeatureCoverage to a sequence of (DateTime, double) tuples
        /// </summary>
        /// <param name="featureCoverage"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        internal static IEnumerable<Tuple<DateTime, double>> ToTimeValueTuples(this IFeatureCoverage featureCoverage,
                                                                               string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) throw new ArgumentException("locationId");

            IBranchFeature branchFeature =
                featureCoverage.Features.OfType<IBranchFeature>().FirstOrDefault(f => f.Name == locationId);
            if (branchFeature != null)
            {
                IMultiDimensionalArray<DateTime> times = featureCoverage.Time.Values;

                var branchFeatureFilter = new VariableValueFilter<IFeature>(featureCoverage.FeatureVariable,
                                                                            branchFeature);

                IMultiDimensionalArray<double> values = featureCoverage.GetValues<double>(branchFeatureFilter);

                if (values == null)
                    throw new InvalidOperationException(
                        "Cant cast values from feature coverage to IMultiDimensionalArray<double>");

                for (int i = 0; i < values.Count; i++)
                    yield return new Tuple<DateTime, double>(times[i], values[i]);
            }
        }

        /// <summary>
        /// Transforms the INetworkCoverage to a sequence of (DateTime, double) tuples
        /// </summary>
        /// <param name="networkCoverage"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        internal static IEnumerable<Tuple<DateTime, double>> ToTimeValueTuples(this INetworkCoverage networkCoverage,
                                                                               string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) throw new ArgumentException("locationId");

            if (locationId == "all")
            {
                IMultiDimensionalArray<INetworkLocation> locations = networkCoverage.Locations.Values;
                foreach (INetworkLocation networkLocation in locations)
                {
                    IFunction func = GetFunctionForNetworkLocation(networkCoverage, (NetworkLocation) networkLocation);
                    foreach (var tuple in QueryResult.ToTimesValuesTuples(func))
                        yield return tuple;
                }
            }
            else
            {
                var location =
                    (NetworkLocation) networkCoverage.Locations.Values.FirstOrDefault(l => l.Name == locationId);
                if (location == null && computationalGrid != null) // STATIC?!?!?
                {
                    location =
                        (NetworkLocation) computationalGrid.Locations.Values.FirstOrDefault(l => l.Name == locationId);
                }
                if (location == null)
                {
                    location = GetLocationByBranchOffsetValues(networkCoverage, locationId);
                }
                if (location == null)
                {
                    log.ErrorFormat("Location: {0} not found for parameter: {1}", locationId, networkCoverage.Name);
                    throw new Exception("Location: " + locationId + " not found for parameter: " + networkCoverage.Name);
                }
                IFunction function = GetFunctionForNetworkLocation(networkCoverage, location);

                //function = networkCoverage.GetTimeSeries(location);

                if (function != null)
                {
                    foreach (var tuple in function.ToTimesValuesTuples())
                        yield return tuple;
                }
            }
        }

        /// <summary>
        /// Transforms the IFeatureData to a sequence of (DateTime, double) tuples
        /// </summary>
        /// <param name="featureData"></param>
        /// <returns></returns>
        internal static IEnumerable<Tuple<DateTime, double>> ToTimeValueTuples(IFeatureData featureData)
        {
            var function = featureData.Data as IFunction;
            if (function == null)
                yield break;

            foreach (var tuple in function.ToTimesValuesTuples())
                yield return tuple;
        }
    }
}