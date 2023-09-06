using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization
{
    /// <summary>
    /// Serializer for a boundary data access object from the external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BoundarySerializer
    {
        /// <summary>
        /// Serialize the boundary data access object to a INI section for the boundary external forcing file.
        /// </summary>
        /// <param name="boundaryDTO"> The boundary data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A converted <see cref="IniSection"/> that contains the data for the boundary section.
        /// </returns>
        public IniSection Serialize(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));

            var section = new IniSection(BndExtForceFileConstants.BoundaryBlockKey);

            section.AddProperty(BndExtForceFileConstants.QuantityKey, boundaryDTO.Quantity);
            section.AddProperty(BndExtForceFileConstants.LocationFileKey, boundaryDTO.LocationFile);

            foreach (string forcingFile in boundaryDTO.ForcingFiles)
            {
                section.AddProperty(BndExtForceFileConstants.ForcingFileKey, forcingFile);
            }

            if (boundaryDTO.ReturnTime.HasValue)
            {
                section.AddProperty(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, boundaryDTO.ReturnTime.Value);
            }

            return section;
        }
    }
}