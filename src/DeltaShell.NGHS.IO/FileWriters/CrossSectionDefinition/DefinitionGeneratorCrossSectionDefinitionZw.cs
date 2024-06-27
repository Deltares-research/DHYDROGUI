using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionZw : DefinitionGeneratorCrossSectionDefinition
    {
        protected DefinitionGeneratorCrossSectionDefinitionZw(string definitionType)
            : base(definitionType)
        {
        }

        public DefinitionGeneratorCrossSectionDefinitionZw()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Zw)
        {
        }

        public override IniSection CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);
            
            var crossSectionDefinitionZw = crossSectionDefinition as CrossSectionDefinitionZW;
            if (crossSectionDefinitionZw == null) return IniSection;

            GenerateTabulatedProfile(crossSectionDefinitionZw);

            var summerDike = crossSectionDefinitionZw.SummerDike;
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.CrestLevee, summerDike.CrestLevel);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FlowAreaLevee, summerDike.FloodSurface);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.TotalAreaLevee, summerDike.TotalSurface);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.BaseLevelLevee, summerDike.FloodPlainLevel);

            AddFrictionData(crossSectionDefinitionZw, writeFrictionFromDefinition, defaultFrictionId);
            
            return IniSection;
        }

        private void AddFrictionData(
            CrossSectionDefinitionZW crossSectionDefinitionZw,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            if (crossSectionDefinitionZw.Sections.Count > 0)
            {
                if (crossSectionDefinitionZw.Sections.Count == 1
                    && !crossSectionDefinitionZw.Sections.First().SectionType.Name.Equals(RoughnessDataSet.MainSectionTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Main, crossSectionDefinitionZw.GetSectionWidth(crossSectionDefinitionZw.Sections.First().SectionType.Name));
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FloodPlain1, 0);
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FloodPlain2, 0);
                    var frictionId = crossSectionDefinitionZw.Sections.First().SectionType.Name;
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionIds, $"{frictionId};{frictionId};{frictionId}");
                    return;
                }
                
                if (!writeFrictionFromDefinition)
                {
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionIds, $"{defaultFrictionId};{defaultFrictionId};{defaultFrictionId}");
                    return;
                }
                
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Main, crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.MainSectionTypeName));

                if (crossSectionDefinitionZw.Sections.Count > 1)
                {
                    double fp1 = crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName);
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FloodPlain1, fp1);
                }

                if (crossSectionDefinitionZw.Sections.Count > 2)
                {
                    double fp2 = crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain2SectionTypeName);
                    IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FloodPlain2, fp2);
                }
            }
            else // crossSectionDefinition came from a Culvert or Bridge
            {
                var largestTotalWidth = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z).Select(r => r.Width).Max();
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Main, largestTotalWidth);
            }
        }

        protected void GenerateTabulatedProfile(CrossSectionDefinitionZW crossSectionDefinitionZw)
        {
            var sortedData = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z).ToArray();

            var levels = sortedData.Select(r => r.Z).ToList();
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.NumLevels, levels.Count);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.Levels, levels);
            if (BinFileForLevelTables != null)
            {
                double[] levelsAsDoubles = sortedData.Select(r => r.Z).ToArray();
                double[] flowWidthsAsDoubles = sortedData.Select(r => r.Width - r.StorageWidth).ToArray();
                double[] totalWidthsAsDoubles = sortedData.Select(r => r.Width).ToArray();

                var levelsAsBytes = DoublesToBytes(levelsAsDoubles);
                var flowWidthsAsBytes = DoublesToBytes(flowWidthsAsDoubles);
                var totalWidthsAsBytes = DoublesToBytes(totalWidthsAsDoubles);

                BinFileForLevelTables.Write(levelsAsBytes);
                BinFileForLevelTables.Write(flowWidthsAsBytes);
                BinFileForLevelTables.Write(totalWidthsAsBytes);
            }

            var flowWidth = sortedData.Select(r => r.Width - r.StorageWidth);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.FlowWidths, flowWidth);

            var totalWidths = sortedData.Select(r => r.Width).ToList();
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.TotalWidths, totalWidths);
        }

        private static byte[] DoublesToBytes(double[] valuesAsDoubles)
        {
            byte[] valuesAsBytes = new byte[valuesAsDoubles.Length*sizeof (double)];
            Buffer.BlockCopy(valuesAsDoubles, 0, valuesAsBytes, 0, valuesAsBytes.Length);
            return valuesAsBytes;
        }
    }
}