using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public static class LocationFileWriter
    {
        public static void WriteFileLateralDischargeLocations(string targetFile, IEnumerable<ILateralSource> lateralSources)
        {
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.LateralDischargeLocationsMajorVersion, 
                                      GeneralRegion.LateralDischargeLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.LateralDischargeLocation),
            };

            var lateralDischargeDefinitions = GenerateFeatureDefinition(lateralSources);
            if (lateralDischargeDefinitions != null) 
                iniSections.AddRange(lateralDischargeDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(iniSections, targetFile);
        }

        public static void WriteFileCrossSectionLocations(string targetFile, IEnumerable<ICrossSection> crossSections)
        {
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionLocationsMajorVersion, 
                                      GeneralRegion.CrossSectionLocationsMinorVersion, 
                                      GeneralRegion.FileTypeName.CrossSectionLocation),
            };
            
            var crossSectionLocationsIniSections = GenerateFeatureDefinition(crossSections);
            if (crossSectionLocationsIniSections != null)
                iniSections.AddRange(crossSectionLocationsIniSections);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(iniSections, targetFile);
        }

        public static void WriteFileObservationPointLocations(string targetFile, IEnumerable<IObservationPoint> observationPointLocations, bool useObsCrs = false)
        {
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.ObservationPointLocationsMajorVersion,
                    GeneralRegion.ObservationPointLocationsMinorVersion,
                    useObsCrs
                        ? GeneralRegion.FileTypeName.ObservationCross
                        : GeneralRegion.FileTypeName.ObservationPoint)
            };
            
            var observationPointLocationsDefinitions = GenerateFeatureDefinition(observationPointLocations, useObsCrs);
            if (observationPointLocationsDefinitions != null)
                iniSections.AddRange(observationPointLocationsDefinitions);

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(iniSections, targetFile);
        }

        private static IEnumerable<IniSection> GenerateFeatureDefinition(
            IEnumerable<IBranchFeature> branchFeatures, bool useObsCrs = false)
        {
            var definitions = new List<IniSection>(); 
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
