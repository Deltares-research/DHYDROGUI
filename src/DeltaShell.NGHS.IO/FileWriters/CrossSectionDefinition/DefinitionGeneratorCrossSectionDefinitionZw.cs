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

        public override DelftIniCategory CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);
            
            var crossSectionDefinitionZw = crossSectionDefinition as CrossSectionDefinitionZW;
            if (crossSectionDefinitionZw == null) return IniCategory;

            GenerateTabulatedProfile(crossSectionDefinitionZw);

            var summerDike = crossSectionDefinitionZw.SummerDike;
            IniCategory.AddProperty(DefinitionPropertySettings.CrestLevee, summerDike.CrestLevel);
            IniCategory.AddProperty(DefinitionPropertySettings.FlowAreaLevee, summerDike.FloodSurface);
            IniCategory.AddProperty(DefinitionPropertySettings.TotalAreaLevee, summerDike.TotalSurface);
            IniCategory.AddProperty(DefinitionPropertySettings.BaseLevelLevee, summerDike.FloodPlainLevel);

            AddFrictionData(crossSectionDefinitionZw, writeFrictionFromDefinition, defaultFrictionId);
            
            return IniCategory;
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
                    IniCategory.AddProperty(DefinitionPropertySettings.Main, crossSectionDefinitionZw.GetSectionWidth(crossSectionDefinitionZw.Sections.First().SectionType.Name));
                    IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain1, 0);
                    IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain2, 0);
                    var frictionId = crossSectionDefinitionZw.Sections.First().SectionType.Name;
                    IniCategory.AddProperty(DefinitionPropertySettings.FrictionIds, $"{frictionId};{frictionId};{frictionId}");
                    return;
                }
                
                if (!writeFrictionFromDefinition)
                {
                    IniCategory.AddProperty(DefinitionPropertySettings.FrictionIds, $"{defaultFrictionId};{defaultFrictionId};{defaultFrictionId}");
                    return;
                }
                
                IniCategory.AddProperty(DefinitionPropertySettings.Main, crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.MainSectionTypeName));

                if (crossSectionDefinitionZw.Sections.Count > 1)
                {
                    double fp1 = crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName);
                    IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain1, fp1);
                }

                if (crossSectionDefinitionZw.Sections.Count > 2)
                {
                    double fp2 = crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.Floodplain2SectionTypeName);
                    IniCategory.AddProperty(DefinitionPropertySettings.FloodPlain2, fp2);
                }
            }
            else // crossSectionDefinition came from a Culvert or Bridge
            {
                var largestTotalWidth = crossSectionDefinitionZw.ZWDataTable.OrderBy(hfsw => hfsw.Z).Select(r => r.Width).Max();
                IniCategory.AddProperty(DefinitionPropertySettings.Main, largestTotalWidth);
            }
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

            var totalWidths = sortedData.Select(r => r.Width).ToList();
            IniCategory.AddProperty(DefinitionPropertySettings.TotalWidths, totalWidths);
        }

        private static byte[] DoublesToBytes(double[] valuesAsDoubles)
        {
            byte[] valuesAsBytes = new byte[valuesAsDoubles.Length*sizeof (double)];
            Buffer.BlockCopy(valuesAsDoubles, 0, valuesAsBytes, 0, valuesAsBytes.Length);
            return valuesAsBytes;
        }
    }
}