using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
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

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            AddCommonProperties(crossSectionDefinition);
            
            var crossSectionDefinitionZw = crossSectionDefinition as CrossSectionDefinitionZW;
            if (crossSectionDefinitionZw == null) return IniCategory;

            GenerateTabulatedProfile(crossSectionDefinitionZw);

            var sortedData = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z);
            var totalWidths = sortedData.Select(r => r.Width).ToList();
            IniCategory.AddProperty(DefinitionPropertySettings.TotalWidths, totalWidths);

            var summerDike = crossSectionDefinitionZw.SummerDike;
            IniCategory.AddProperty(DefinitionPropertySettings.CrestLevee, summerDike.CrestLevel);
            IniCategory.AddProperty(DefinitionPropertySettings.FlowAreaLevee, summerDike.FloodSurface);
            IniCategory.AddProperty(DefinitionPropertySettings.TotalAreaLevee, summerDike.TotalSurface);
            IniCategory.AddProperty(DefinitionPropertySettings.BaseLevelLevee, summerDike.FloodPlainLevel);

            if (crossSectionDefinitionZw.Sections.Count > 0)
            {
                IniCategory.AddProperty(DefinitionPropertySettings.Main, crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.MainSectionTypeName));
                IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain1, crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName));
                IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain2, crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain2SectionTypeName));
            }
            else // crossSectionDefinition came from a Culvert or Bridge
            {
                var largestTotalWidth = totalWidths.Max();
                IniCategory.AddProperty(DefinitionPropertySettings.Main, largestTotalWidth);
            }
            
            return IniCategory;
        }

        protected void GenerateTabulatedProfile(CrossSectionDefinitionZW crossSectionDefinitionZw)
        {
            var sortedData = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z).ToArray();

            var levels = sortedData.Select(r => r.Z).ToList();
            IniCategory.AddProperty(DefinitionPropertySettings.NumLevels, levels.Count);
            IniCategory.AddProperty(DefinitionPropertySettings.Levels, levels);
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
            IniCategory.AddProperty(DefinitionPropertySettings.FlowWidths, flowWidth);
        }

        private static byte[] DoublesToBytes(double[] valuesAsDoubles)
        {
            byte[] valuesAsBytes = new byte[valuesAsDoubles.Length*sizeof (double)];
            Buffer.BlockCopy(valuesAsDoubles, 0, valuesAsBytes, 0, valuesAsBytes.Length);
            return valuesAsBytes;
        }
    }
}