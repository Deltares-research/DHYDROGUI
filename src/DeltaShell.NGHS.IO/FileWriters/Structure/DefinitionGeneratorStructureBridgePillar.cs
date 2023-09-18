using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureBridgePillar : DefinitionGeneratorStructureBridge
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.BridgePillar);

            var bridge = hydroObject as IBridge;
            if(bridge == null) return IniSection;

            AddCommonBridgeElements(bridge);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PillarWidth.Key, bridge.PillarWidth, StructureRegion.PillarWidth.Description, StructureRegion.PillarWidth.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.FormFactor.Key, bridge.ShapeFactor, StructureRegion.FormFactor.Description, StructureRegion.FormFactor.Format);
            
            return IniSection;
        }
    }
}
