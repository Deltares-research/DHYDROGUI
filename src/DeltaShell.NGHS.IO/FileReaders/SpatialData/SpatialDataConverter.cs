using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    /// <summary>
    /// This class is responsible for converting a collection of <see cref="IDelftIniCategory"/> into an <see cref="INetworkCoverage"/>.
    /// </summary>
    public static class SpatialDataConverter
    {
        /// <summary>
        /// Converts the specified categories to an <see cref="INetworkCoverage"/> object with <see cref="INetworkLocation"/> as arguments and
        /// <see cref="double"/> as function values. The <param name="channels"/> are needed here, because they will be put on the appropriate
        /// <see cref="INetworkLocation"/> objects.
        /// </summary>
        /// <param name="categories">The data model for the <see cref="INetworkCoverage"/> objects.</param>
        /// <param name="channels">The channels that will be used for the <see cref="INetworkLocation"/> argument objects.</param>
        /// <param name="warningMessages">The warning messages that will be shown to the user when something exceptional has happened during converting.</param>
        /// <returns>An <see cref="INetworkCoverage"/> object with <see cref="INetworkLocation"/> as arguments and <see cref="double"/> as function values.</returns>
        public static INetworkCoverage Convert(IList<DelftIniCategory> categories, IList<IChannel> channels, IList<string> warningMessages)
        {
            var networkCoverage = new NetworkCoverage();
            networkCoverage.SetInterpolationType(categories);

            var definitionCategories = categories.Where(category =>
                string.Equals(category.Name, SpatialDataRegion.DefinitionIniHeader, StringComparison.OrdinalIgnoreCase));

            var networkLocations = new List<INetworkLocation>();
            var networkCoverageValues = new List<double>();
            foreach (var category in definitionCategories)
            {
                try
                {
                    networkLocations.Add(CreateNetworkLocation(category, channels));
                    networkCoverageValues.Add(GetNetworkCoverageValue(category));
                }
                catch (Exception e) when (e is PropertyNotFoundInFileException || e is ArgumentException)
                {
                    warningMessages.Add(e.Message);
                }
            }
            networkCoverage.Arguments[0].SetValues(networkLocations);
            networkCoverage.Components[0].SetValues(networkCoverageValues);

            return networkCoverage;
        }

        private static void SetInterpolationType(this IFunction function, IEnumerable<IDelftIniCategory> categories)
        {
            var contentTab = categories.FirstOrDefault(category => category.Name == SpatialDataRegion.ContentIniHeader);
            if (contentTab == null)
            {
                function.Arguments[0].InterpolationType = InterpolationType.Constant;
                return;
            }

            var interpolated = contentTab.ReadProperty<string>(SpatialDataRegion.Interpolate.Key);
            function.Arguments[0].InterpolationType = interpolated == "1" ? InterpolationType.Linear : InterpolationType.Constant;
        }

        private static INetworkLocation CreateNetworkLocation(IDelftIniCategory category, IEnumerable<IChannel> channels)
        {
            var channel = GetMatchingChannel(category, channels);
            var networkLocation = category.ConvertToNetworkLocationWithBranch(channel);
            return networkLocation;
        }

        private static IChannel GetMatchingChannel(IDelftIniCategory category, IEnumerable<IChannel> channels)
        {
            var branchName = category.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
            return channels.FirstOrDefault(c => string.Equals(c.Name, branchName, StringComparison.OrdinalIgnoreCase));
        }

        private static INetworkLocation ConvertToNetworkLocationWithBranch(this IDelftIniCategory category, IBranch channel)
        {
            // Essential Properties (an error will be generated if these fail)
            var chainage = category.ReadProperty<double>(SpatialDataRegion.Chainage.Key);

            if (channel == null)
            {
                var errorMessage =
                    string.Format(Resources.SpatialDataConverter_ConvertToSpatialData_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_, category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new ArgumentException(errorMessage);
            }

            return new NetworkLocation
            {
                Branch = channel,
                Chainage = chainage,
                Geometry = new Point(LengthLocationMap.GetLocation(channel.Geometry, chainage).GetCoordinate(channel.Geometry)),
            };
        }

        private static double GetNetworkCoverageValue(DelftIniCategory category)
        {
            return category.ReadProperty<double>(SpatialDataRegion.Value.Key);
        }
    }
}
