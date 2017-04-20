using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorLocation : IDefinitionGeneratorLocation
    {
        protected DelftIniCategory IniCategory { get; private set; }

        public DefinitionGeneratorLocation(string iniCategoryName)
        {
            IniCategory = new DelftIniCategory(iniCategoryName);
        }

        protected void AddCommonRegionElements(IBranchFeature branchFeature)
        {
            if(branchFeature.Branch == null) throw new FileWritingException("BranchFeature does not have a valid Branch property");
            IniCategory.AddProperty(LocationRegion.Id.Key, branchFeature.Name, LocationRegion.Id.Description);
            IniCategory.AddProperty(LocationRegion.BranchId.Key, branchFeature.Branch.Name, LocationRegion.BranchId.Description);
            IniCategory.AddProperty(LocationRegion.Chainage.Key, branchFeature.Chainage, LocationRegion.Chainage.Description, LocationRegion.Chainage.Format);

            var networkFeature = branchFeature as IHydroNetworkFeature;
            if (networkFeature != null)
                IniCategory.AddProperty(LocationRegion.Name.Key, networkFeature.LongName, LocationRegion.Name.Description);
        }

        public virtual DelftIniCategory CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            return IniCategory;
        }
    }

    public interface IDefinitionGeneratorLocation
    {
        DelftIniCategory CreateIniRegion(IBranchFeature branchFeature);
    }
}