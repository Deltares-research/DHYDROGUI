using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class CrossSectionDefinitionFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionDefinitionFileWriter));
        
        public static void WriteFile(
            string targetFile,
            IHydroNetwork network,
            Func<IChannel, bool> writeFrictionFromCrossSectionDefinitionsForChannel,
            string defaultFrictionId)
        {
            var crossSectionDefinitions = network.GetNetworkCrossSectionDefinitions();
            var sharedCrossSectionDefinitions = network.SharedCrossSectionDefinitions;
            var channelsPerCrossSectionDefinitionLookup = network.GetChannelsPerCrossSectionDefinitionLookup();

            WriteFile(targetFile, crossSectionDefinitions, sharedCrossSectionDefinitions,
                csd => WriteFrictionFromCrossSectionDefinition(writeFrictionFromCrossSectionDefinitionsForChannel, channelsPerCrossSectionDefinitionLookup, csd),
                defaultFrictionId);
        }

        private static void WriteFile(
            string targetFile,
            IEnumerable<ICrossSectionDefinition> crossSectionDefinitions,
            IEnumerable<ICrossSectionDefinition> sharedCrossSectionDefinitions,
            Func<ICrossSectionDefinition, bool> writeFrictionFromCrossSectionDefinition,
            string defaultFrictionId)
        {
            if (File.Exists(targetFile)) File.Delete(targetFile);

            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion, 
                    GeneralRegion.CrossSectionDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.CrossSectionDefinition)
            };

            var processedCsDefinitions = new List<string>();
            var processedCsDef = new Dictionary<ICrossSectionDefinition, IniSection>();
            foreach (var crossSectionDefinition in crossSectionDefinitions)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(crossSectionDefinition);
                
                if (definitionGeneratorCrossSectionDefinition == null) continue;

                var csDefinitionId = crossSectionDefinition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(
                    crossSectionDefinition,
                    writeFrictionFromCrossSectionDefinition(crossSectionDefinition),
                    defaultFrictionId);

                iniSections.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
                processedCsDef[crossSectionDefinition] = definitionRegion;
            }

            foreach (var sharedCrossSectionDefinition in sharedCrossSectionDefinitions)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(sharedCrossSectionDefinition);

                if (definitionGeneratorCrossSectionDefinition == null) continue;

                var csDefinitionId = sharedCrossSectionDefinition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId))
                {
                    var processedCrossSectionDefinition = processedCsDef.SingleOrDefault(pcsd => pcsd.Key.Name.Equals(csDefinitionId, StringComparison.InvariantCultureIgnoreCase)).Key;
                    if (processedCrossSectionDefinition != null &&
                        sharedCrossSectionDefinition.Equals(processedCrossSectionDefinition))
                    {
                        if(processedCsDef.TryGetValue(processedCrossSectionDefinition, out var iniSection))
                            iniSection.AddProperty(DefinitionPropertySettings.IsShared.Key, true);
                        continue;
                    }
                }

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(
                    sharedCrossSectionDefinition,
                    writeFrictionFromCrossSectionDefinition(sharedCrossSectionDefinition),
                    defaultFrictionId);
                definitionRegion.AddProperty(DefinitionPropertySettings.IsShared.Key,true);
                iniSections.Add(definitionRegion);
            }

            WriteIniFile(targetFile, iniSections);
        }

        private static void WriteIniFile(string targetFile, IEnumerable<IniSection> iniSections)
        {
            var iniFormatter = new IniFormatter
            {
                Configuration =
                {
                    WriteComments = false,
                    PropertyIndentationLevel = 4,
                }
            };
            IniData crossSectionDefinitionIniData = GetIniDataFromSections(iniSections);

            log.InfoFormat(Resources.CrossSectionDefinitionFileWriter_WriteIniFile_Writing_cross_section_definitions_to__0__, targetFile);
            using (Stream iniStream = File.Open(targetFile, FileMode.Create))
            {
                iniFormatter.Format(crossSectionDefinitionIniData, iniStream);
            }
        }

        private static IniData GetIniDataFromSections(IEnumerable<IniSection> iniSections)
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(iniSections);
            return iniData;
        }

        private static bool WriteFrictionFromCrossSectionDefinition(
            Func<IChannel, bool> writeFrictionFromCrossSectionDefinitionsForChannel,
            IDictionary<ICrossSectionDefinition, IEnumerable<IChannel>> channelsPerCrossSectionDefinitionLookup,
            ICrossSectionDefinition crossSectionDefinition)
        {
            if (!channelsPerCrossSectionDefinitionLookup.TryGetValue(crossSectionDefinition, out var channels))
            {
                return true; // Always write friction for unused shared cross section definitions
            }

            return channels.Any(writeFrictionFromCrossSectionDefinitionsForChannel);
        }
    }
}