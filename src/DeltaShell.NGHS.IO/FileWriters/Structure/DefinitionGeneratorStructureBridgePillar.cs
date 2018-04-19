using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureBridgePillar : DefinitionGeneratorStructureBridge
    {
        public DefinitionGeneratorStructureBridgePillar(KeyValuePair<int, string> compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.BridgePillar);

            var bridge = structure as IBridge;
            if(bridge == null) return IniCategory;

            AddCommonBridgeElements(bridge);

            IniCategory.AddProperty(StructureRegion.PillarWidth.Key, bridge.PillarWidth, StructureRegion.PillarWidth.Description, StructureRegion.PillarWidth.Format);
            IniCategory.AddProperty(StructureRegion.FormFactor.Key, bridge.ShapeFactor, StructureRegion.FormFactor.Description, StructureRegion.FormFactor.Format);
            
            return IniCategory;
        }
    }
}
