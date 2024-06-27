using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using System;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    /// <summary>
    /// Writer for location files of cross sections, laterals and observation points.
    /// </summary>
    public static class LocationFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LocationFileWriter));

        /// <summary>
        /// Writes lateral locations to the specified <paramref name="targetFile"/>.
        /// </summary>
        /// <param name="targetFile">The file to write to.</param>
        /// <param name="lateralSources">The lateral sources to write the locations for.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetFile"/> is <c>null</c> or white space.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lateralSources"/> is <c>null</c>.</exception>
        public static void WriteFileLateralDischargeLocations(string targetFile, IEnumerable<ILateralSource> lateralSources)
        {
            Ensure.NotNullOrWhiteSpace(targetFile, nameof(targetFile));
            Ensure.NotNull(lateralSources, nameof(lateralSources));
            
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.LateralDischargeLocationsMajorVersion,
                    GeneralRegion.LateralDischargeLocationsMinorVersion,
                    GeneralRegion.FileTypeName.LateralDischargeLocation),
            };

            IEnumerable<IniSection> lateralDischargeDefinitions = GenerateFeatureDefinition(lateralSources);
            iniSections.AddRange(lateralDischargeDefinitions);
            
            WriteIniFile(targetFile, iniSections);
        }
        
        /// <summary>
        /// Writes cross section locations to the specified <paramref name="targetFile"/>.
        /// </summary>
        /// <param name="targetFile">The file to write to.</param>
        /// <param name="crossSections">The cross sections to write the locations for.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetFile"/> is <c>null</c> or white space.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="crossSections"/> is <c>null</c>.</exception>
        public static void WriteFileCrossSectionLocations(string targetFile, IEnumerable<ICrossSection> crossSections)
        {
            Ensure.NotNullOrWhiteSpace(targetFile, nameof(targetFile));
            Ensure.NotNull(crossSections, nameof(crossSections));
            
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.CrossSectionLocationsMajorVersion,
                    GeneralRegion.CrossSectionLocationsMinorVersion,
                    GeneralRegion.FileTypeName.CrossSectionLocation),
            };
            
            IEnumerable<IniSection> crossSectionLocationsIniSections = GenerateFeatureDefinition(crossSections);
            iniSections.AddRange(crossSectionLocationsIniSections);
            
            WriteIniFile(targetFile, iniSections);
        }

        /// <summary>
        /// Writes observation point locations to the specified <paramref name="targetFile"/>.
        /// </summary>
        /// <param name="targetFile">The file to write to.</param>
        /// <param name="observationPointLocations">The observation points to write the locations for.</param>
        /// <param name="useObsCrs">Whether or not to use observation cross sections. Defaults to <c>false</c>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetFile"/> is <c>null</c> or white space.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observationPointLocations"/> is <c>null</c>.</exception>
        public static void WriteFileObservationPointLocations(string targetFile, 
                                                              IEnumerable<IObservationPoint> observationPointLocations, 
                                                              bool useObsCrs = false)
        {
            Ensure.NotNullOrWhiteSpace(targetFile, nameof(targetFile));
            Ensure.NotNull(observationPointLocations, nameof(observationPointLocations));
            
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.ObservationPointLocationsMajorVersion,
                    GeneralRegion.ObservationPointLocationsMinorVersion,
                    useObsCrs
                        ? GeneralRegion.FileTypeName.ObservationCross
                        : GeneralRegion.FileTypeName.ObservationPoint)
            };
            
            IEnumerable<IniSection> observationPointLocationsDefinitions = GenerateFeatureDefinition(observationPointLocations, useObsCrs);
            iniSections.AddRange(observationPointLocationsDefinitions);

            WriteIniFile(targetFile, iniSections);
        }

        private static IEnumerable<IniSection> GenerateFeatureDefinition(
            IEnumerable<IBranchFeature> branchFeatures, bool useObsCrs = false)
        {
            var definitions = new List<IniSection>();
            if (branchFeatures == null)
            {
                return Enumerable.Empty<IniSection>();
            }
            
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

        private static void WriteIniFile(string targetFile, IEnumerable<IniSection> iniSections)
        {
            var iniFormatter = new IniFormatter()
            {
                Configuration =
                {
                    WriteComments = false,
                    PropertyIndentationLevel = 4
                }
            };

            var iniData = new IniData();
            iniData.AddMultipleSections(iniSections);
            
            log.InfoFormat(Resources.LocationFileWriter_WriteIniFile_Writing_locations_to__0__, targetFile);
            using (Stream iniStream = File.Open(targetFile, FileMode.Create))
            {
                iniFormatter.Format(iniData, iniStream);
            }
        }
    }
}
