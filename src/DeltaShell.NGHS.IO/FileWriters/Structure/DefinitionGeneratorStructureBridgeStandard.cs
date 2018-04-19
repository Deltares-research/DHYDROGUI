using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureBridgeStandard : DefinitionGeneratorStructureBridge
    {
        public DefinitionGeneratorStructureBridgeStandard(KeyValuePair<int, string> compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.Bridge);

            var bridge = structure as IBridge;
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
