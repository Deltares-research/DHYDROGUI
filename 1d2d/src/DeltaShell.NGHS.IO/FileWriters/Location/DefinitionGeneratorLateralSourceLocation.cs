using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorLateralSourceLocation : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorLateralSourceLocation(string iniSectionName)
            : base(iniSectionName)
        {
        }

        public override IEnumerable<IniSection> CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            var lateralSource = branchFeature as ILateralSource;
            if (lateralSource == null || Math.Abs(lateralSource.Length) < double.Epsilon)
            {
                yield return IniSection;
                yield break;
            }
            IniSection.AddPropertyWithOptionalCommentAndFormat(LateralSourceLocationRegion.Length.Key, lateralSource.Length, LateralSourceLocationRegion.Length.Description, LateralSourceLocationRegion.Length.Format);

            yield return IniSection;
        }
    }
}