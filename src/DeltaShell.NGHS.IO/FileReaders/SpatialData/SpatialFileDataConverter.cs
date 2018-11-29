using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    class SpatialFileDataConverter
    {
        public static INetworkCoverage Convert(IList<DelftIniCategory> categories, IList<IChannel> channelsList, IList<string> errorMessages)
        {
            var networkCoverage = new NetworkCoverage();
            //Content tab
            var contentTab = categories.Where(category => category.Name == SpatialDataRegion.ContentIniHeader).ToList();
            var spatialDataType = contentTab[0].ReadProperty<string>(SpatialDataRegion.Quantity.Key);

            var interpolated = contentTab[0].ReadProperty<string>(SpatialDataRegion.Interpolate.Key);
            networkCoverage.Arguments[0].InterpolationType = interpolated == "1" ? InterpolationType.Linear : InterpolationType.Constant;
            
            //Definition tabs
            var definitionTabs = categories.Where(category => category.Name == SpatialDataRegion.DefinitionIniHeader);
            var networkLocations = new List<INetworkLocation>();
            var networkValues = new List<double>();
            foreach (var spatialDefinition in definitionTabs)
            {
                try
                {
                    //Extract branchId & chainage properties
                    var generatedSpatialData = ConvertToSpatialData(spatialDefinition, channelsList);
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

        private static INetworkLocation ConvertToSpatialData(IDelftIniCategory category, IList<IChannel> channelsList)
        {
            // Essential Properties (an error will be generated if these fail)
            var branchName = category.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
            var branch = channelsList.FirstOrDefault(c => c.Name == branchName);
            var chainage = category.ReadProperty<double>(SpatialDataRegion.Chainage.Key);
            
            if (branch == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

            return new NetworkLocation()
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
