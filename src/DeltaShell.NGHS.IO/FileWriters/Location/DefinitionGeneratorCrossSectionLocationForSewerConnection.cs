using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocationForSewerConnection : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocationForSewerConnection(string iniSectionName) : base(iniSectionName)
        {
        }

        public override IEnumerable<IniSection> CreateIniRegion(IBranchFeature branchFeature)
        {
            if (!(branchFeature.Branch is ISewerConnection sewerConnection)) yield break;

            yield return CreateSourceIniSection(sewerConnection);
            yield return CreateTargetIniSection(sewerConnection);
        }

        private static IniSection CreateSourceIniSection(ISewerConnection sewerConnection)
        {
            var sourceIniSection = new IniSection(CrossSectionRegion.IniHeader);
            sourceIniSection.AddProperty(new IniProperty(LocationRegion.PipeId.Key, sewerConnection.Name + "_begin", string.Empty));
            sourceIniSection.AddProperty(new IniProperty(LocationRegion.BranchId.Key, sewerConnection.Name, string.Empty));
            sourceIniSection.AddPropertyWithOptionalCommentAndFormat(LocationRegion.PipeChainage.Key, 0.0d, format: LocationRegion.PipeChainage.Format);
            sourceIniSection.AddProperty(new IniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", sewerConnection.LevelSource), string.Empty));
            sourceIniSection.AddProperty(new IniProperty(LocationRegion.Definition.Key, sewerConnection.CrossSection.Definition.Name, string.Empty));
            return sourceIniSection;
        }

        private static IniSection CreateTargetIniSection(ISewerConnection sewerConnection)
        {
            var targetIniSection = new IniSection(CrossSectionRegion.IniHeader);
            targetIniSection.AddProperty(new IniProperty(LocationRegion.PipeId.Key, sewerConnection.Name + "_end", string.Empty));
            targetIniSection.AddProperty(new IniProperty(LocationRegion.BranchId.Key, sewerConnection.Name, string.Empty));
            targetIniSection.AddPropertyWithOptionalCommentAndFormat(LocationRegion.PipeChainage.Key, sewerConnection.Length.TruncateByDigits(), format:LocationRegion.PipeChainage.Format);
            targetIniSection.AddProperty(new IniProperty(LocationRegion.Shift.Key, string.Format(CultureInfo.InvariantCulture, "{0:0.00}", sewerConnection.LevelTarget), string.Empty));
            targetIniSection.AddProperty(new IniProperty(LocationRegion.Definition.Key, sewerConnection.CrossSection.Definition.Name, string.Empty));
            return targetIniSection;
        }
    }
}
