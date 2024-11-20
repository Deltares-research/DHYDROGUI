using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data
{
    /// <summary>
    /// Data access object for the boundary data in a boundary external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BoundaryDTO
    {
        private readonly HashSet<string> forcingFiles;
        private int lineNumber;

        /// <summary>
        /// Initialize a new instance of the <see cref="BoundaryDTO"/> class.
        /// </summary>
        /// <param name="quantity"> The quantity of the boundary.</param>
        /// <param name="locationFile">The location file of the boundary.</param>
        /// <param name="forcingFiles">The forcing files of the boundary.</param>
        /// <param name="returnTime">The Thatcher-Harleman return time.</param>
        public BoundaryDTO(string quantity, string locationFile, IEnumerable<string> forcingFiles, double? returnTime)
        {
            Quantity = quantity;
            LocationFile = locationFile;
            ReturnTime = returnTime;

            this.forcingFiles = new HashSet<string>(forcingFiles);
        }

        /// <summary>
        /// The boundary quantity.
        /// </summary>
        public string Quantity { get; }

        /// <summary>
        /// The relative path to the location file of the boundary.
        /// </summary>
        public string LocationFile { get; }

        /// <summary>
        /// The forcing files for this boundary.
        /// </summary>
        public IEnumerable<string> ForcingFiles => forcingFiles;

        /// <summary>
        /// The Thatcher-Harleman return time.
        /// </summary>
        public double? ReturnTime { get; set; }

        /// <summary>
        /// The line number of the corresponding section in the file.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="value"/> is a negative number.
        /// </exception>
        public int LineNumber
        {
            get => lineNumber;
            set
            {
                Ensure.NotNegative(value, nameof(value));
                lineNumber = value;
            }
        }

        /// <summary>
        /// Add a forcing file to this instance.
        /// </summary>
        /// <param name="forcingFile"> The forcing file to add. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="forcingFile"/> is <c>null</c>.
        /// </exception>
        public void AddForcingFile(string forcingFile)
        {
            Ensure.NotNull(forcingFile, nameof(forcingFile));
            forcingFiles.Add(forcingFile);
        }

        /// <summary>
        /// Remove a forcing file from this instance.
        /// </summary>
        /// <param name="forcingFile"> The forcing file to remove. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="forcingFile"/> is <c>null</c>.
        /// </exception>
        public void RemoveForcingFile(string forcingFile)
        {
            Ensure.NotNull(forcingFile, nameof(forcingFile));
            forcingFiles.Remove(forcingFile);
        }
    }
}