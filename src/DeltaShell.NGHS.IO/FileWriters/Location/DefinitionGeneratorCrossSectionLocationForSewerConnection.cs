using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocationForSewerConnection : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocationForSewerConnection(string iniCategoryName) : base(iniCategoryName)
        {
        }

        public override IEnumerable<DelftIniCategory> CreateIniRegion(IBranchFeature branchFeature)
        {
            if (!(branchFeature.Branch is ISewerConnection sewerConnection)) yield break;

            yield return CreateSourceIniCategory(sewerConnection);
            yield return CreateTargetIniCategory(sewerConnection);
        }

        private static DelftIniCategory CreateSourceIniCategory(ISewerConnection sewerConnection)
        {
            var sourceiniCategory = new DelftIniCategory(CrossSectionRegion.IniHeader);
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeId.Key, sewerConnection.Name + "_begin", string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.BranchId.Key, sewerConnection.Name, string.Empty));
            sourceiniCategory.AddProperty(LocationRegion.PipeChainage.Key, 0.0d, format: LocationRegion.PipeChainage.Format);
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", sewerConnection.LevelSource), string.Empty));
            sourceiniCategory.AddProperty(new DelftIniProperty(LocationRegion.Definition.Key, sewerConnection.CrossSection.Definition.Name, string.Empty));
            return sourceiniCategory;
        }

        private static DelftIniCategory CreateTargetIniCategory(ISewerConnection sewerConnection)
        {
            var targetIniCategory = new DelftIniCategory(CrossSectionRegion.IniHeader);
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.PipeId.Key, sewerConnection.Name + "_end", string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.BranchId.Key, sewerConnection.Name, string.Empty));
            targetIniCategory.AddProperty(LocationRegion.PipeChainage.Key, sewerConnection.Length.TruncateByDigits(), format:LocationRegion.PipeChainage.Format);
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", sewerConnection.LevelTarget), string.Empty));
            targetIniCategory.AddProperty(new DelftIniProperty(LocationRegion.Definition.Key, sewerConnection.CrossSection.Definition.Name, string.Empty));
            return targetIniCategory;
        }
    }
}
