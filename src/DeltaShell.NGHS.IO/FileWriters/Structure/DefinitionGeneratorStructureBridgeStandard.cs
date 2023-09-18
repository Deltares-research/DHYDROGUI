using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureBridgeStandard : DefinitionGeneratorStructureBridge
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Bridge);

            var bridge = hydroObject as IBridge;
            if (bridge == null) return IniSection;

            AddCommonBridgeElements(bridge);
            IniSection.AddPropertyWithOptionalComment(StructureRegion.CsDefId.Key, bridge.CrossSectionDefinition.Name, StructureRegion.CsDefId.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Length.Key, bridge.Length, StructureRegion.Length.Description, StructureRegion.Length.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.InletLossCoeff.Key, bridge.InletLossCoefficient, StructureRegion.InletLossCoeff.Description, StructureRegion.InletLossCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.OutletLossCoeff.Key, bridge.OutletLossCoefficient, StructureRegion.OutletLossCoeff.Description, StructureRegion.OutletLossCoeff.Format);
            
            return IniSection;
        }
    }
}
