using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
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
        /// Gets the <see cref="DHYDRO.Common.IO.Ini.IniSection"/> which is being constructed.
        /// </summary>
        protected IniSection IniSection { get; }

        /// <summary>
        /// Creates a new <see cref="DefinitionGeneratorStructure"/>
        /// </summary>
        /// <remarks>
        /// This initializes the <see cref="IniSection"/>.
        /// </remarks>
        protected DefinitionGeneratorStructure()
        {
            IniSection = new IniSection(StructureRegion.Header);
        }

        public abstract IniSection CreateStructureRegion(IHydroObject hydroObject);

        protected virtual void AddCommonRegionElements(IHydroObject hydroObject, string definitionType)
        {
            var branchFeature = hydroObject as IBranchFeature;
            if (branchFeature?.Branch == null) return;

            AddIdPropertyToIniSection(hydroObject);

            if(hydroObject is IHydroNetworkFeature hydroNetworkFeature) 
                IniSection.AddPropertyWithOptionalComment(StructureRegion.Name.Key, 
                                        hydroNetworkFeature.LongName, 
                                        StructureRegion.Name.Description);

            IniSection.AddPropertyWithOptionalComment(StructureRegion.BranchId.Key, 
                                    branchFeature.Branch.Name, 
                                    StructureRegion.BranchId.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Chainage.Key, 
                                    branchFeature.Branch.GetBranchSnappedChainage(branchFeature.Chainage), 
                                    StructureRegion.Chainage.Description, 
                                    StructureRegion.Chainage.Format);

            AddDefinitionTypePropertyToIniSection(definitionType);
        }

        /// <summary>
        /// Add an Id property to this <see cref="IniSection"/>.
        /// </summary>
        /// <param name="hydroObject">The hydro object</param>
        protected void AddIdPropertyToIniSection(IHydroObject hydroObject)
        {
            string nameWithoutHashSigns = hydroObject.Name.Replace("##", "~~");
            IniSection.AddPropertyWithOptionalComment(StructureRegion.Id.Key, 
                                    nameWithoutHashSigns, 
                                    StructureRegion.Id.Description);
        }

        /// <summary>
        /// Add an definition type property to this <see cref="IniSection"/>.
        /// </summary>
        /// <param name="definitionType">The definition type to add</param>
        protected void AddDefinitionTypePropertyToIniSection(string definitionType) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.DefinitionType.Key, 
                                    definitionType, 
                                    StructureRegion.DefinitionType.Description);

        /// <summary>
        /// Add a property with the given <paramref name="value"/> with the given <see cref="setting"/>
        /// to the <see cref="IniSection"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setting"></param>
        protected void AddPropertyToIniSection(double value, ConfigurationSetting setting) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(setting.Key, value, setting.Description, setting.Format);
    }
}