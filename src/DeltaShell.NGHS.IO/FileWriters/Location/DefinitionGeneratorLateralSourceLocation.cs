using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorLateralSourceLocation : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorLateralSourceLocation(string iniCategoryName)
            : base(iniCategoryName)
        {
        }

        public override IEnumerable<DelftIniCategory> CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            var lateralSource = branchFeature as ILateralSource;
            if (lateralSource == null || Math.Abs(lateralSource.Length) < double.Epsilon)
            {
                yield return IniCategory;
                yield break;
            }
            IniCategory.AddProperty(LateralSourceLocationRegion.Length.Key, lateralSource.Length, LateralSourceLocationRegion.Length.Description, LateralSourceLocationRegion.Length.Format);

            yield return IniCategory;
        }
    }
}