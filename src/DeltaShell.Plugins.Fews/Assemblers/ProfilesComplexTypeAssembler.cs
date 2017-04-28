using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.IO.FewsPI;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using nl.wldelft.util.coverage;
using nl.wldelft.util.timeseries;
using Coverage = nl.wldelft.util.coverage.Coverage;

namespace DeltaShell.Plugins.Fews.Assemblers
{
    public class ProfilesComplexTypeAssembler : LongitudinalAssemblerBase
    {
        internal void AssembleProfileTimeSeries(TimeSeriesArray timeSeriesArray, NetworkCoverage networkCoverage)
        {
            ThrowIfNetworkCoverageIsNotValid();

            if (!NetworkCoverage.IsTimeDependent)
                throw new InvalidOperationException("The network spatial data is not time dependent");

            SetTimeEventValues(timeSeriesArray, networkCoverage, Route, GetLocationsInRoute().ToList());
        }

        public static void SetTimeEventValues(TimeSeriesArray timeSeriesArray, NetworkCoverage networkCoverage, Route route, IList<INetworkLocation> routeLocations)
        {
            string[] locationLabels = new string[routeLocations.Count()];
            double[] locationChainages = new double[routeLocations.Count()];
            int[] locationIndices = new int[routeLocations.Count()];

            for (int index = 0; index < routeLocations.Count; index++)
            {
                if (route != null)
                {
                    locationChainages[index] = RouteHelper.GetRouteChainage(route, routeLocations[index]);
                }
                else
                {
                    locationChainages[index] = routeLocations[index].Chainage;
                }
                int locationIndex = networkCoverage.Locations.Values.IndexOf(routeLocations[index]);
                if (locationIndex < 0)
                {
                    throw new IndexOutOfRangeException(String.Format("Location '{0}' not found in network spatial data '{1}'",
                        routeLocations[index], networkCoverage.Name));
                }
                locationIndices[index] = locationIndex;
                locationLabels[index] = networkCoverage.Locations.Values[locationIndex].Name;
            }

            ProfileGeometry profileGeometry = new ProfileGeometry(locationChainages, locationLabels);
            timeSeriesArray.setRequiredGeometry(profileGeometry);

            IEnumerable<DateTime> timeSteps = networkCoverage.Time.AllValues;
            foreach (DateTime timeStep in timeSteps)
            {
                IList<double> values = (IList<double>)networkCoverage[timeStep];
                float[] coverageValues = new float[locationIndices.Length];
                for (int i = 0; i < locationIndices.Length; i++)
                {
                    coverageValues[i] = (float) values[locationIndices[i]];
                }
                Coverage coverage = new Coverage(profileGeometry, coverageValues);
                long javaTimeInMillies = Java2DotNetHelper.JavaMilliesFromDotNetDateTime(timeStep);
                timeSeriesArray.putValue(javaTimeInMillies, coverage);
            }
        }
    }
}