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

        public static void WriteFileCrossSectionLocations(string targetFile, IEnumerable<ICrossSection> crossSections)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionLocationsMajorVersion, 
                                      GeneralRegion.CrossSectionLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.CrossSectionLocation),
            };
            
            var crossSectionLocationsCategories = GenerateFeatureDefinition(crossSections);
            if (crossSectionLocationsCategories != null)
                categories.AddRange(crossSectionLocationsCategories);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        public static void WriteFileObservationPointLocations(string targetFile, IEnumerable<IObservationPoint> observationPointLocations, bool useObsCrs = false)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.ObservationPointLocationsMajorVersion,
                    GeneralRegion.ObservationPointLocationsMinorVersion,
                    useObsCrs
                        ? GeneralRegion.FileTypeName.ObservationCross
                        : GeneralRegion.FileTypeName.ObservationPoint)
            };
            
            var observationPointLocationsDefinitions = GenerateFeatureDefinition(observationPointLocations, useObsCrs);
            if (observationPointLocationsDefinitions != null)
                categories.AddRange(observationPointLocationsDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static IEnumerable<DelftIniCategory> GenerateFeatureDefinition(
            IEnumerable<IBranchFeature> branchFeatures, bool useObsCrs = false)
        {
            var definitions = new List<DelftIniCategory>(); 
            if (branchFeatures == null) return null;
            
            branchFeatures.ForEach(branchFeature =>
            {
                var definitionGeneratorLocation = DefinitionGeneratorFactory.GetDefinitionGeneratorLocation(branchFeature, useObsCrs);
                if (definitionGeneratorLocation != null)
                {
                    definitions.AddRange(definitionGeneratorLocation.CreateIniRegion(branchFeature));
                }
            });
            return definitions;
        }
    }
}
