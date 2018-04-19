using System.Collections.Generic;
using DelftTools.Hydro.Structures;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructureBridge : DefinitionGeneratorStructure
    {
        protected DefinitionGeneratorStructureBridge(KeyValuePair<int, string> compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        protected void AddCommonBridgeElements(IBridge bridge)
        {
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, (int)bridge.FlowDirection, StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.BedLevel.Key, bridge.BottomLevel, StructureRegion.BedLevel.Description, StructureRegion.BedLevel.Format);
        }
    }
}
