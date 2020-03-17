using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileWriters.SpatialData
{
    public static class SpatialDataFileWriter 
    {
        public static void WriteFile(string filename, string quantityName, INetworkCoverage networkCoverage)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.SpatialDataMajorVersion, 
                                                             GeneralRegion.SpatialDataMinorVersion, 
                                                             GeneralRegion.FileTypeName.SpatialData),
                GenerateContentRegion(quantityName, networkCoverage)
            };
            if (networkCoverage != null && networkCoverage.Locations != null && networkCoverage.Locations.Values != null)
            {
                var spatialDataDefinitions = networkCoverage.Locations.Values.Zip(networkCoverage.GetValues<double>(),
                    (l, v) => new KeyValuePair<INetworkLocation, double>(l, v))
                    .Select(kvp => GenerateSpatialDataDefinition(kvp.Key, kvp.Value));
                categories.AddRange(spatialDataDefinitions);
            }

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        public static DelftIniCategory GenerateSpatialDataDefinition(INetworkLocation location, double value)
        {
            if(location.Branch == null) throw new FileWritingException("NetworkLocation does not have a valid Branch property");
            var definition = new DelftIniCategory(SpatialDataRegion.DefinitionIniHeader);
            definition.AddProperty(SpatialDataRegion.BranchId.Key, location.Branch.Name, SpatialDataRegion.BranchId.Description);
            definition.AddProperty(SpatialDataRegion.Chainage.Key, location.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(location.Chainage), SpatialDataRegion.Chainage.Description, SpatialDataRegion.Chainage.Format);
            definition.AddProperty(SpatialDataRegion.Value.Key, value, SpatialDataRegion.Value.Description, SpatialDataRegion.Value.Format);
            return definition;
        }

        private static DelftIniCategory GenerateContentRegion(string quantityName, INetworkCoverage networkCoverage)
        {
            var content = new DelftIniCategory(SpatialDataRegion.ContentIniHeader);
            content.AddProperty(SpatialDataRegion.Quantity.Key, quantityName, SpatialDataRegion.Quantity.Description);
            var interpolationIsLinear = networkCoverage != null && (networkCoverage.Arguments.FirstOrDefault() != null && networkCoverage.Arguments.First().InterpolationType == InterpolationType.Linear) ? 1 : 0;
             content.AddProperty(SpatialDataRegion.Interpolate.Key, interpolationIsLinear, SpatialDataRegion.Interpolate.Description);
            return content;
        }
    }
    
}
