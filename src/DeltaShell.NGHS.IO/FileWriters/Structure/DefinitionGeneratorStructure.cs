using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructure"/> implements a base class for
    /// <see cref="IDefinitionGeneratorStructure"/>.
    /// </summary>
    public abstract class DefinitionGeneratorStructure : IDefinitionGeneratorStructure
    {
        /// <summary>
        /// Gets the <see cref="DelftIniCategory"/> which is being constructed.
        /// </summary>
        protected DelftIniCategory IniCategory { get; }

        /// <summary>
        /// Creates a new <see cref="DefinitionGeneratorStructure"/>
        /// </summary>
        /// <remarks>
        /// This initializes the <see cref="DefinitionGeneratorStructure.IniCategory"/>.
        /// </remarks>
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

            if(hydroObject is IHydroNetworkFeature hydroNetworkFeature) 
                IniCategory.AddProperty(StructureRegion.Name.Key, 
                                        hydroNetworkFeature.LongName, 
                                        StructureRegion.Name.Description);

            IniCategory.AddProperty(StructureRegion.BranchId.Key, 
                                    branchFeature.Branch.Name, 
                                    StructureRegion.BranchId.Description);
            IniCategory.AddProperty(StructureRegion.Chainage.Key, 
                                    branchFeature.Branch.GetBranchSnappedChainage(branchFeature.Chainage), 
                                    StructureRegion.Chainage.Description, 
                                    StructureRegion.Chainage.Format);

            AddDefinitionTypePropertyToIniCategory(definitionType);
        }

        /// <summary>
        /// Add an Id property to this <see cref="DefinitionGeneratorStructure.IniCategory"/>.
        /// </summary>
        /// <param name="hydroObject">The hydro object</param>
        protected void AddIdPropertyToIniCategory(IHydroObject hydroObject)
        {
            string nameWithoutHashSigns = hydroObject.Name.Replace("##", "~~");
            IniCategory.AddProperty(StructureRegion.Id.Key, 
                                    nameWithoutHashSigns, 
                                    StructureRegion.Id.Description);
        }

        /// <summary>
        /// Add an definition type property to this <see cref="DefinitionGeneratorStructure.IniCategory"/>.
        /// </summary>
        /// <param name="definitionType">The definition type to add</param>
        protected void AddDefinitionTypePropertyToIniCategory(string definitionType) => 
            IniCategory.AddProperty(StructureRegion.DefinitionType.Key, 
                                    definitionType, 
                                    StructureRegion.DefinitionType.Description);

        /// <summary>
        /// Add a property with the given <paramref name="value"/> with the given <see cref="setting"/>
        /// to the <see cref="DefinitionGeneratorStructure.IniCategory"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setting"></param>
        protected void AddPropertyToIniCategory(double value, ConfigurationSetting setting) => 
            IniCategory.AddProperty(setting.Key, value, setting.Description, setting.Format);
    }
}