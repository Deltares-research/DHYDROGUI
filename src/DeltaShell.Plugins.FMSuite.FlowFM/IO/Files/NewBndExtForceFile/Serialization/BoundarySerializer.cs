using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization
{
    /// <summary>
    /// Serializer for a boundary data access object from the external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BoundarySerializer
    {
        /// <summary>
        /// Serialize the boundary data access object to a Delft INI category for the boundary external forcing file.
        /// </summary>
        /// <param name="boundaryDTO"> The boundary data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A converted <see cref="DelftIniCategory"/> that contains the data for the boundary category.
        /// </returns>
        public DelftIniCategory Serialize(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));

            var category = new DelftIniCategory(BndExtForceFileConstants.BoundaryBlockKey);

            category.AddProperty(BndExtForceFileConstants.QuantityKey, boundaryDTO.Quantity);
            category.AddProperty(BndExtForceFileConstants.LocationFileKey, boundaryDTO.LocationFile);

            foreach (string forcingFile in boundaryDTO.ForcingFiles)
            {
                category.AddProperty(BndExtForceFileConstants.ForcingFileKey, forcingFile);
            }

            if (boundaryDTO.ReturnTime.HasValue)
            {
                category.AddProperty(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, boundaryDTO.ReturnTime.Value);
            }

            return category;
        }
    }
}