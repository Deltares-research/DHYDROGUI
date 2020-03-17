using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructure : IDefinitionGeneratorStructure
    {
        protected DelftIniCategory IniCategory { get; private set; }

        protected DefinitionGeneratorStructure()
        {
            IniCategory = new DelftIniCategory(StructureRegion.Header);
        }

        public abstract DelftIniCategory CreateStructureRegion(IHydroObject hydroObject);

        protected virtual void AddCommonRegionElements(IHydroObject hydroObject, string definitionType)
        {
            var branchFeature = hydroObject as IBranchFeature;
            if (branchFeature?.Branch == null) return;

            AddIdPropertyToIniCategory(hydroObject);

            var hydroNetworkFeature = hydroObject as IHydroNetworkFeature;
            if(hydroNetworkFeature != null) IniCategory.AddProperty(StructureRegion.Name.Key, hydroNetworkFeature.LongName, StructureRegion.Name.Description);

            IniCategory.AddProperty(StructureRegion.BranchId.Key, branchFeature.Branch.Name, StructureRegion.BranchId.Description);
            IniCategory.AddProperty(StructureRegion.Chainage.Key, branchFeature.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(branchFeature.Chainage), StructureRegion.Chainage.Description, StructureRegion.Chainage.Format);

            /*var compoundStructureId = compoundStructureInfo.Id;
            IniCategory.AddProperty(StructureRegion.Compound.Key, compoundStructureId, StructureRegion.Compound.Description);
            if (compoundStructureId > 0) IniCategory.AddProperty(StructureRegion.CompoundName.Key, compoundStructureInfo.Name, StructureRegion.CompoundName.Description);*/

            AddDefinitionTypePropertyToIniCategory(definitionType);
        }

        protected void AddIdPropertyToIniCategory(IHydroObject hydroObject)
        {
            var nameWithoutHashSigns = hydroObject.Name.Replace("##", "~~");
            IniCategory.AddProperty(StructureRegion.Id.Key, nameWithoutHashSigns, StructureRegion.Id.Description);
        }

        protected void AddDefinitionTypePropertyToIniCategory(string definitionType)
        {
            IniCategory.AddProperty(StructureRegion.DefinitionType.Key, definitionType, StructureRegion.DefinitionType.Description);
        }

        protected void AddPropertyToIniCategory(double value, ConfigurationSetting setting)
        {
            IniCategory.AddProperty(setting.Key, value, setting.Description, setting.Format);
        }
    }
}