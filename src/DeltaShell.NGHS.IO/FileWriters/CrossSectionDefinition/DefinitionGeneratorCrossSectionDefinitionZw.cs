using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
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
            AddCommonRegionElements(crossSectionDefinition);
            
            var crossSectionDefinitionZw = crossSectionDefinition as CrossSectionDefinitionZW;
            if (crossSectionDefinitionZw == null) return IniCategory;

            GenerateTabulatedProfile(crossSectionDefinitionZw);

            var sortedData = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z);
            var totalWidths = sortedData.Select(r => r.Width).ToList();
            IniCategory.AddProperty(DefinitionRegion.TotalWidths.Key, totalWidths, DefinitionRegion.TotalWidths.Description, DefinitionRegion.TotalWidths.Format);

            var summerDike = crossSectionDefinitionZw.SummerDike;
            IniCategory.AddProperty(DefinitionRegion.CrestSummerdike.Key, summerDike.CrestLevel, DefinitionRegion.CrestSummerdike.Description, DefinitionRegion.CrestSummerdike.Format);
            IniCategory.AddProperty(DefinitionRegion.FlowAreaSummerdike.Key, summerDike.FloodSurface, DefinitionRegion.FlowAreaSummerdike.Description, DefinitionRegion.FlowAreaSummerdike.Format);
            IniCategory.AddProperty(DefinitionRegion.TotalAreaSummerdike.Key, summerDike.TotalSurface, DefinitionRegion.TotalAreaSummerdike.Description, DefinitionRegion.TotalAreaSummerdike.Format);
            IniCategory.AddProperty(DefinitionRegion.BaseLevelSummerdike.Key, summerDike.FloodPlainLevel, DefinitionRegion.BaseLevelSummerdike.Description, DefinitionRegion.BaseLevelSummerdike.Format);

            if (crossSectionDefinitionZw.Sections.Count > 0)
            {
                IniCategory.AddProperty(DefinitionRegion.Main.Key, crossSectionDefinitionZw.GetSectionWidth(CrossSectionDefinitionZW.MainSectionName), DefinitionRegion.Main.Description, DefinitionRegion.Main.Format);
                IniCategory.AddProperty(DefinitionRegion.FloodPlain1.Key, crossSectionDefinitionZw.GetSectionWidth(CrossSectionDefinitionZW.Floodplain1SectionTypeName), DefinitionRegion.FloodPlain1.Description, DefinitionRegion.FloodPlain1.Format);    
                IniCategory.AddProperty(DefinitionRegion.FloodPlain2.Key, crossSectionDefinitionZw.GetSectionWidth(CrossSectionDefinitionZW.Floodplain2SectionTypeName), DefinitionRegion.FloodPlain2.Description, DefinitionRegion.FloodPlain2.Format);    
            }
            else // crossSectionDefinition came from a Culvert or Bridge
            {
                var largestTotalWidth = totalWidths.Max();
                IniCategory.AddProperty(DefinitionRegion.Main.Key, largestTotalWidth, DefinitionRegion.Main.Description, DefinitionRegion.Main.Format);
            }
            
            return IniCategory;
        }

        protected void GenerateTabulatedProfile(CrossSectionDefinitionZW crossSectionDefinitionZw)
        {
            var sortedData = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z).ToArray();

            var levels = sortedData.Select(r => r.Z).ToList();
            IniCategory.AddProperty(DefinitionRegion.NumLevels.Key, levels.Count, DefinitionRegion.NumLevels.Description);
            IniCategory.AddProperty(DefinitionRegion.Levels.Key, levels, DefinitionRegion.Levels.Description, DefinitionRegion.Levels.Format);
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
            IniCategory.AddProperty(DefinitionRegion.FlowWidths.Key, flowWidth, DefinitionRegion.FlowWidths.Description, DefinitionRegion.FlowWidths.Format);
        }

        private static byte[] DoublesToBytes(double[] valuesAsDoubles)
        {
            byte[] valuesAsBytes = new byte[valuesAsDoubles.Length*sizeof (double)];
            Buffer.BlockCopy(valuesAsDoubles, 0, valuesAsBytes, 0, valuesAsBytes.Length);
            return valuesAsBytes;
        }
    }
}