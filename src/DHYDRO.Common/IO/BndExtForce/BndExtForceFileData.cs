using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DHYDRO.Common.IO.Validation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Represents the data contained in the new style external forcings file (*_bnd.ext).
    /// This file is referenced by the MDU file through the <c>ExtForceFileNew</c> property.
    /// </summary>
    public sealed class BndExtForceFileData
    {
        private readonly List<BndExtForceBoundaryData> boundaryForcings;
        private readonly List<BndExtForceLateralData> lateralForcings;
        private readonly List<BndExtForceMeteoData> meteoForcings;

        private BndExtForceFileInfo fileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceFileData"/> class.
        /// </summary>
        public BndExtForceFileData()
        {
            boundaryForcings = new List<BndExtForceBoundaryData>();
            lateralForcings = new List<BndExtForceLateralData>();
            meteoForcings = new List<BndExtForceMeteoData>();
            fileInfo = new BndExtForceFileInfo();

            BoundaryDataValidator = new BndExtForceBoundaryDataValidator();
            LateralDataValidator = new BndExtForceLateralDataValidator();
            MeteoDataValidator = new BndExtForceMeteoDataValidator();
        }

        /// <summary>
        /// Gets or sets the external forcings file information.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public BndExtForceFileInfo FileInfo
        {
            get => fileInfo;
            set
            {
                Ensure.NotNull(value, nameof(value));
                fileInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets the boundary forcing data validator.
        /// </summary>
        internal BndExtForceBoundaryDataValidator BoundaryDataValidator { get; set; }

        /// <summary>
        /// Gets or sets the lateral forcing data validator.
        /// </summary>
        internal BndExtForceLateralDataValidator LateralDataValidator { get; set; }

        /// <summary>
        /// Gets or sets the meteo forcing data validator.
        /// </summary>
        internal BndExtForceMeteoDataValidator MeteoDataValidator { get; set; }

        /// <summary>
        /// Gets the boundary data in the external forcings file.
        /// </summary>
        public IEnumerable<BndExtForceBoundaryData> BoundaryForcings => boundaryForcings.AsEnumerable();

        /// <summary>
        /// Gets the lateral data in the external forcings file.
        /// </summary>
        public IEnumerable<BndExtForceLateralData> LateralForcings => lateralForcings.AsEnumerable();

        /// <summary>
        /// Gets the meteo data in the external forcings file.
        /// </summary>
        public IEnumerable<BndExtForceMeteoData> MeteoForcings => meteoForcings.AsEnumerable();

        /// <summary>
        /// Determines whether the file data contains any forcings.
        /// </summary>
        /// <returns><c>true</c> if the file data contains any forcings; otherwise, <c>false</c>.</returns>
        public bool AnyForcings() => boundaryForcings.Any() || lateralForcings.Any() || meteoForcings.Any();

        /// <summary>
        /// Gets the distinct forcing files referenced by the file data.
        /// </summary>
        /// <returns>A collection of unique forcing files.</returns>
        public IEnumerable<string> GetForcingFiles()
        {
            return BoundaryForcings.SelectMany(x => x.ForcingFiles)
                                   .Concat(MeteoForcings.Select(x => x.ForcingFile))
                                   .Distinct();
        }

        /// <summary>
        /// Gets the distinct location files referenced by the file data.
        /// </summary>
        /// <returns>A collection of unique location files.</returns>
        public IEnumerable<string> GetLocationFiles()
        {
            return BoundaryForcings.Select(x => x.LocationFile)
                                   .Concat(LateralForcings.Select(x => x.LocationFile))
                                   .Distinct();
        }

        /// <summary>
        /// Adds boundary forcing data to the file data.
        /// </summary>
        /// <param name="forcing">The boundary forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcing"/> is <c>null</c>.</exception>
        public void AddBoundaryForcing(BndExtForceBoundaryData forcing)
        {
            Ensure.NotNull(forcing, nameof(forcing));
            boundaryForcings.Add(forcing);
        }

        /// <summary>
        /// Adds a collection of boundary forcing data to the file data.
        /// </summary>
        /// <param name="forcings">The collection of boundary forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcings"/> is <c>null</c>.</exception>
        public void AddBoundaryForcings(IEnumerable<BndExtForceBoundaryData> forcings)
        {
            Ensure.NotNull(forcings, nameof(forcings));
            boundaryForcings.AddRange(forcings);
        }

        /// <summary>
        /// Adds lateral forcing data to the file data.
        /// </summary>
        /// <param name="forcing">The lateral forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcing"/> is <c>null</c>.</exception>
        public void AddLateralForcing(BndExtForceLateralData forcing)
        {
            Ensure.NotNull(forcing, nameof(forcing));
            lateralForcings.Add(forcing);
        }

        /// <summary>
        /// Adds a collection of lateral forcing data to the file data.
        /// </summary>
        /// <param name="forcings">The collection of lateral forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcings"/> is <c>null</c>.</exception>
        public void AddLateralForcings(IEnumerable<BndExtForceLateralData> forcings)
        {
            Ensure.NotNull(forcings, nameof(forcings));
            lateralForcings.AddRange(forcings);
        }

        /// <summary>
        /// Adds meteo forcing data to the file data.
        /// </summary>
        /// <param name="forcing">The meteo forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcing"/> is <c>null</c>.</exception>
        public void AddMeteoForcing(BndExtForceMeteoData forcing)
        {
            Ensure.NotNull(forcing, nameof(forcing));
            meteoForcings.Add(forcing);
        }

        /// <summary>
        /// Adds a collection of meteo forcing data to the file data.
        /// </summary>
        /// <param name="forcings">The collection of meteo forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcings"/> is <c>null</c>.</exception>
        public void AddMeteoForcings(IEnumerable<BndExtForceMeteoData> forcings)
        {
            Ensure.NotNull(forcings, nameof(forcings));
            meteoForcings.AddRange(forcings);
        }

        /// <summary>
        /// Removes invalid forcing data and logs the validation messages to the specified log handler.
        /// </summary>
        /// <param name="logHandler">The log handler to report user messages with.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="logHandler"/> is <c>null</c>.</exception>
        public void RemoveInvalidForcings(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            boundaryForcings.RemoveAll(data => !BoundaryDataValidator.IsValidWithLogging(data, logHandler));
            lateralForcings.RemoveAll(data => !LateralDataValidator.IsValidWithLogging(data, logHandler));
            meteoForcings.RemoveAll(data => !MeteoDataValidator.IsValidWithLogging(data, logHandler));
        }
    }
}