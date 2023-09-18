using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.General;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class CrossSectionDefinitionFileWriter
    {
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

            new IniFileWriter().WriteIniFile(iniSections, targetFile);
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