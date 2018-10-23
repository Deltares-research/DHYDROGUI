using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public static class LocationFileWriter
    {
        public static void WriteFileLateralDischargeLocations(string targetFile, IEnumerable<ILateralSource> lateralSources)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.LateralDischargeLocationsMajorVersion, 
                                      GeneralRegion.LateralDischargeLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.LateralDischargeLocation),
            };

            var lateralDischargeDefinitions = GenerateFeatureDefinition(lateralSources);
            if (lateralDischargeDefinitions != null) 
                categories.AddRange(lateralDischargeDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        public static void WriteFileCrossSectionLocations(string targetFile, IEnumerable<ICrossSection> crossSectionLocations)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionLocationsMajorVersion, 
                                      GeneralRegion.CrossSectionLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.CrossSectionLocation),
            };
            
            var crossSectionLocationsDefinitions = GenerateFeatureDefinition(crossSectionLocations);
            if (crossSectionLocationsDefinitions != null)
                categories.AddRange(crossSectionLocationsDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        public static void WriteFileObservationPointLocations(string targetFile, IEnumerable<IObservationPoint> observationPointLocations)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.ObservationPointLocationsMajorVersion, 
                                      GeneralRegion.ObservationPointLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.ObservationPoint),
            };
            
            var observationPointLocationsDefinitions = GenerateFeatureDefinition(observationPointLocations);
            if (observationPointLocationsDefinitions != null)
                categories.AddRange(observationPointLocationsDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static IEnumerable<DelftIniCategory> GenerateFeatureDefinition(IEnumerable<IBranchFeature> branchFeatures)
        {
            var definitions = new List<DelftIniCategory>(); 
            if (branchFeatures == null) return null;
            
            branchFeatures.ForEach(branchFeature =>
            {
                var definitionGeneratorLocation = DefinitionGeneratorFactory.GetDefinitionGeneratorLocation(branchFeature);
                if (definitionGeneratorLocation != null)
                {
                    definitions.Add(definitionGeneratorLocation.CreateIniRegion(branchFeature));
                }
            });
            return definitions;
        }
    }
}
