using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
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
        public IEnumerable<string> LocationFiles => GetLocationFilesFromBoundaries().Distinct();

        /// <summary>
        /// A set of unique forcing files collected from all the boundaries and laterals.
        /// </summary>
        public IEnumerable<string> ForcingFiles => GetForcingFilesFromBoundaries().Concat(GetForcingFilesFromLaterals()).Distinct();

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
        }

        /// <summary>
        /// Remove a boundary data access data object from this instance.
        /// </summary>
        /// <param name="boundaryDTO">The boundary data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        public void RemoveBoundary(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));
            boundaries.Remove(boundaryDTO);
        }

        /// <summary>
        /// Remove a lateral data access data object from this instance.
        /// </summary>
        /// <param name="lateralDTO">The lateral data access object.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralDTO"/> is <c>null</c>.
        /// </exception>
        public void RemoveLateral(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));
            laterals.Remove(lateralDTO);
        }

        private IEnumerable<string> GetLocationFilesFromBoundaries()
        {
            return boundaries.Where(b => IsEmbankment(b) && HasValue(b.LocationFile)).Select(b => b.LocationFile);
        }

        private IEnumerable<string> GetForcingFilesFromBoundaries()
        {
            return boundaries.SelectMany(b => b.ForcingFiles.Where(HasValue));
        }

        private IEnumerable<string> GetForcingFilesFromLaterals()
        {
            return laterals.Where(HasTimeSeriesDischarge).Select(l => l.Discharge.TimeSeriesFilename);
        }

        private static bool HasTimeSeriesDischarge(LateralDTO lateralDTO)
        {
            return lateralDTO.Discharge?.Mode == SteerableMode.TimeSeries;
        }

        private static bool IsEmbankment(BoundaryDTO boundaryDTO)
        {
            return boundaryDTO.Quantity != ExtForceQuantNames.EmbankmentBnd;
        }

        private static bool HasValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}