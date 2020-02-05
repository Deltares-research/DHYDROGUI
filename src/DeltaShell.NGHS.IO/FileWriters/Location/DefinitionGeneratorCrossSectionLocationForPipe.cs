using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocationForPipe : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocationForPipe(string iniCategoryName) : base(iniCategoryName)
        {
        }

        public override IEnumerable<DelftIniCategory> CreateIniRegion(IBranchFeature branchFeature)
        {
            var pipe = branchFeature.Branch as IPipe;
            if (pipe == null) yield break;

            yield return CreateSourceIniCategory(pipe);
            yield return CreateTargetIniCategory(pipe);
        }

        private static DelftIniCategory CreateSourceIniCategory(IPipe pipe)
        {
            var sourceiniCategory = new DelftIniCategory(CrossSectionRegion.IniHeader);
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeId.Key, pipe.Name + "_source", string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.BranchId.Key, pipe.Name, string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeChainage.Key, "0.00", string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.LevelSource), string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.Definition.Key, pipe.CrossSection.Definition.Name, string.Empty));
            return sourceiniCategory;
        }

        private static DelftIniCategory CreateTargetIniCategory(IPipe pipe)
        {
            var targetIniCategory = new DelftIniCategory(CrossSectionRegion.IniHeader);
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeId.Key, pipe.Name + "_target", string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.BranchId.Key, pipe.Name, string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeChainage.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.Length), string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", pipe.LevelTarget), string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.Definition.Key, pipe.CrossSection.Definition.Name, string.Empty));
            return targetIniCategory;
        }
    }
}
