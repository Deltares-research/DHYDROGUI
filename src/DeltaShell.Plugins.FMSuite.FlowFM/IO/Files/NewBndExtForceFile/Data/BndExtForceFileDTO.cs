using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data
{
    /// <summary>
    /// Data access object for a boundary external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceFileDTO
    {
        private readonly IList<BoundaryDTO> boundaries = new List<BoundaryDTO>();
        private readonly IList<LateralDTO> laterals = new List<LateralDTO>();
        private readonly HashSet<string> locationFiles = new HashSet<string>();
        private readonly HashSet<string> forcingFiles = new HashSet<string>();

        /// <summary>
        /// The boundary data access objects.
        /// </summary>
        public IEnumerable<BoundaryDTO> Boundaries => boundaries;

        /// <summary>
        /// The lateral data access objects.
        /// </summary>
        public IEnumerable<LateralDTO> Laterals => laterals;

        /// <summary>
        /// A set of unique location files collected from all the boundaries.
        /// </summary>
        public IEnumerable<string> LocationFiles => locationFiles;

        /// <summary>
        /// A set of unique forcing files collected from all the boundaries.
        /// </summary>
        public IEnumerable<string> ForcingFiles => forcingFiles;

        /// <summary>
        /// Add a boundary data access data object to this instance.
        /// If a forcing file or location file has no value, the file will not be added to this instance.
        /// </summary>
        /// <param name="boundaryDTO">The boundary data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        public void AddBoundary(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));

            boundaries.Add(boundaryDTO);

            if (IsEmbankment(boundaryDTO) && HasValue(boundaryDTO.LocationFile))
            {
                locationFiles.Add(boundaryDTO.LocationFile);
            }

            foreach (string forcingFile in boundaryDTO.ForcingFiles.Where(HasValue))
            {
                forcingFiles.Add(forcingFile);
            }
        }

        /// <summary>
        /// Add a lateral data access data object to this instance.
        /// If a forcing file has no value, the file will not be added to this instance.
        /// </summary>
        /// <param name="lateralDTO">The lateral data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralDTO"/> is <c>null</c>.
        /// </exception>
        public void AddLateral(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));

            laterals.Add(lateralDTO);

            if (lateralDTO.Discharge?.Mode == SteerableMode.TimeSeries)
            {
                forcingFiles.Add(lateralDTO.Discharge.TimeSeriesFilename);
            }
        }

        private static bool IsEmbankment(BoundaryDTO boundaryDTO) =>
            boundaryDTO.Quantity != ExtForceQuantNames.EmbankmentBnd;

        private static bool HasValue(string value) => !string.IsNullOrWhiteSpace(value);
    }
}