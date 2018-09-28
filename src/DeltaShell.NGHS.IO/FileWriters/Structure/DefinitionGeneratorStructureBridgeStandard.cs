using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureBridgeStandard : DefinitionGeneratorStructureBridge
    {
        public DefinitionGeneratorStructureBridgeStandard(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Bridge);

            var bridge = hydroObject as IBridge;
            if (bridge == null) return IniCategory;

            AddCommonBridgeElements(bridge);
            IniCategory.AddProperty(StructureRegion.CsDefId.Key, bridge.CrossSectionDefinition.Name, StructureRegion.CsDefId.Description);
            IniCategory.AddProperty(StructureRegion.Length.Key, bridge.Length, StructureRegion.Length.Description, StructureRegion.Length.Format);
            IniCategory.AddProperty(StructureRegion.InletLossCoeff.Key, bridge.InletLossCoefficient, StructureRegion.InletLossCoeff.Description, StructureRegion.InletLossCoeff.Format);
            IniCategory.AddProperty(StructureRegion.OutletLossCoeff.Key, bridge.OutletLossCoefficient, StructureRegion.OutletLossCoeff.Description, StructureRegion.OutletLossCoeff.Format);
            
            return IniCategory;
        }
    }
}
