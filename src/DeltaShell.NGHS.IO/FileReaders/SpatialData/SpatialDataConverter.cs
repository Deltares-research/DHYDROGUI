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
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public static class SpatialDataConverter
    {
        public static INetworkCoverage Convert(IList<DelftIniCategory> categories, IList<IChannel> channels, IList<string> errorMessages)
        {
            var networkCoverage = new NetworkCoverage();
            networkCoverage.SetInterpolationType(categories);

            //Definition tabs
            var definitionTabs = categories.Where(category => category.Name == SpatialDataRegion.DefinitionIniHeader);
            var networkLocations = new List<INetworkLocation>();
            var networkValues = new List<double>();
            foreach (var spatialDefinition in definitionTabs)
            {
                try
                {
                    //Extract branchId & chainage properties
                    var generatedSpatialData = ConvertToSpatialData(spatialDefinition, channels);
                    networkLocations.Add(generatedSpatialData);
                    
                    //Extract value property
                    var generatedSpatialValues = ConvertToSpatialValue(spatialDefinition);
                    networkValues.Add(generatedSpatialValues);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }
            networkCoverage.Arguments[0].SetValues(networkLocations);
            networkCoverage.Components[0].SetValues(networkValues);

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

        private static INetworkLocation ConvertToSpatialData(IDelftIniCategory category, IEnumerable<IChannel> channels)
        {
            // Essential Properties (an error will be generated if these fail)
            var branchName = category.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
            var chainage = category.ReadProperty<double>(SpatialDataRegion.Chainage.Key);

            var branch = channels.FirstOrDefault(c => c.Name == branchName);

            if (branch == null)
            {
                var errorMessage =
                    string.Format(Resources.SpatialDataConverter_ConvertToSpatialData_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_, category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

            return new NetworkLocation
            {
                Branch = branch,
                Chainage = chainage,
                Geometry = new Point(LengthLocationMap.GetLocation(branch.Geometry, chainage).GetCoordinate(branch.Geometry)),
            };

        }
        private static double ConvertToSpatialValue(IDelftIniCategory category)
        {
            var value = category.ReadProperty<double>(SpatialDataRegion.Value.Key);

            return value;
        }
    }
}
