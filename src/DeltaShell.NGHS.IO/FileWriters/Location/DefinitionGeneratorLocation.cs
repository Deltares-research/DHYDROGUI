using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorLocation : IDefinitionGeneratorLocation
    {
        protected IniSection IniSection { get; private set; }

        public DefinitionGeneratorLocation(string iniSectionName)
        {
            IniSection = new IniSection(iniSectionName);
        }

        protected void AddCommonRegionElements(IBranchFeature branchFeature)
        {
            if(branchFeature.Branch == null) throw new FileWritingException("BranchFeature does not have a valid Branch property");
            if(branchFeature is IObservationPoint)
                IniSection.AddPropertyWithOptionalComment(LocationRegion.ObsId.Key, branchFeature.Name, LocationRegion.ObsId.Description);
            else
                IniSection.AddPropertyWithOptionalComment(LocationRegion.Id.Key, branchFeature.Name, LocationRegion.Id.Description);
            IniSection.AddPropertyWithOptionalComment(LocationRegion.BranchId.Key, branchFeature.Branch.Name, LocationRegion.BranchId.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(LocationRegion.Chainage.Key, branchFeature.Branch.GetBranchSnappedChainage(branchFeature.Chainage), LocationRegion.Chainage.Description, LocationRegion.Chainage.Format);

            var networkFeature = branchFeature as IHydroNetworkFeature;
            if (networkFeature != null && !(branchFeature is IObservationPoint))
                IniSection.AddPropertyWithOptionalComment(LocationRegion.Name.Key, networkFeature.LongName, LocationRegion.Name.Description);
        }

        public virtual IEnumerable<IniSection> CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            yield return IniSection;
        }
    }

    public interface IDefinitionGeneratorLocation
    {
        IEnumerable<IniSection> CreateIniRegion(IBranchFeature branchFeature);
    }
}