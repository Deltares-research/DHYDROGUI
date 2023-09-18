using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructureBridge : DefinitionGeneratorStructure
    {
        protected void AddCommonBridgeElements(IBridge bridge)
        {
            IniSection.AddPropertyWithOptionalComment(StructureRegion.AllowedFlowDir.Key, bridge.FlowDirection.ToString().ToLower(), StructureRegion.AllowedFlowDir.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Shift.Key, bridge.Shift, StructureRegion.Shift.Description, StructureRegion.Shift.Format);
        }
    }
}
