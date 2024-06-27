using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// Represents a definition generator for bridge pillars. This class extends the standard bridge structure definition generator.
    /// </summary>
    public class DefinitionGeneratorStructureBridgePillar : DefinitionGeneratorStructureBridgeStandard
    {
        /// <summary>
        /// Creates the structure region for a bridge pillar based on the provided hydro object.
        /// </summary>
        /// <param name="hydroObject">The hydro object (<see cref="IHydroObject"/>) representing the pillar bridge.</param>
        /// <returns>The INI section containing the structure region information.</returns>
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));
            base.CreateStructureRegion(hydroObject);
            var bridge = hydroObject as IBridge;
            if (bridge == null) return IniSection;

            var pillarWidthProperty = new IniProperty(StructureRegion.PillarWidth.Key,
                                              bridge.PillarWidth.ToString(StructureRegion.PillarWidth.Format, CultureInfo.InvariantCulture),
                                              StructureRegion.PillarWidth.Description);
            IniSection.AddProperty(pillarWidthProperty);
            
            var formFactorProperty = new IniProperty(StructureRegion.FormFactor.Key,
                                                    bridge.ShapeFactor.ToString(StructureRegion.FormFactor.Format, CultureInfo.InvariantCulture),
                                                    StructureRegion.FormFactor.Description);
            IniSection.AddProperty(formFactorProperty);

            return IniSection;
        }
    }
}
