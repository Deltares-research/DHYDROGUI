using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// Represents a standard bridge structure definition generator.
    /// </summary>
    public class DefinitionGeneratorStructureBridgeStandard : DefinitionGeneratorStructureBridge
    {
        /// <summary>
        /// Creates the structure region for a standard bridge based on the provided hydro object.
        /// </summary>
        /// <param name="hydroObject">The hydro object (<see cref="IHydroObject"/> representing the bridge.</param>
        /// <returns>The INI section containing the structure region information.</returns>
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

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
