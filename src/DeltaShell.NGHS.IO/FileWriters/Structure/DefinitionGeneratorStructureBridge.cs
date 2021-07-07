using DelftTools.Hydro.Structures;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructureBridge : DefinitionGeneratorStructure
    {
        protected void AddCommonBridgeElements(IBridge bridge)
        {
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, bridge.FlowDirection.ToString().ToLower(), StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.Shift.Key, bridge.Shift, StructureRegion.Shift.Description, StructureRegion.Shift.Format);
        }
    }
}
