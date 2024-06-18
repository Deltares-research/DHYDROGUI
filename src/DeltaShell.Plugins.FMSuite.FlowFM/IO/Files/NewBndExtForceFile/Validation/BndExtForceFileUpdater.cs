using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation
{
    /// <summary>
    /// Class to ensure that <see cref="BndExtForceFileDTO"/> object are valid.
    /// </summary>
    public sealed class BndExtForceFileUpdater
    {
        private readonly BoundaryValidator boundaryValidator;
        private readonly LateralValidator lateralValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryValidator"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or
        /// MDU file dependent on the PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="bndExtForceFilePath"> The external forcing file path. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="bndExtForceFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public BndExtForceFileUpdater(string referencePath, string bndExtForceFilePath, ILogHandler logHandler)
            : this(referencePath, bndExtForceFilePath, logHandler, new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryValidator"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="fileSystem"> Provides access to the file system. </param>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or
        /// MDU file dependent on the PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="bndExtForceFilePath"> The external forcing file path. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="bndExtForceFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public BndExtForceFileUpdater(string referencePath, string bndExtForceFilePath, ILogHandler logHandler, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(bndExtForceFilePath, nameof(bndExtForceFilePath));
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            lateralValidator = new LateralValidator(logHandler, fileSystem, referencePath, bndExtForceFilePath);
            boundaryValidator = new BoundaryValidator(logHandler, fileSystem, referencePath, bndExtForceFilePath);
        }

        /// <summary>
        /// Update the provided data.
        /// The provided <paramref name="bndExtForceFile"/> instance is updated such that the object is valid.
        /// </summary>
        /// <param name="bndExtForceFile"> The boundary external forcing file data. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="bndExtForceFile"/> is <c>null</c>.
        /// </exception>
        public void Update(BndExtForceFileDTO bndExtForceFile)
        {
            Ensure.NotNull(bndExtForceFile, nameof(bndExtForceFile));

            RemoveInvalidBoundaries(bndExtForceFile);
            RemoveInvalidLaterals(bndExtForceFile);
        }

        private void RemoveInvalidBoundaries(BndExtForceFileDTO bndExtForceFile)
        {
            foreach (BoundaryDTO boundary in bndExtForceFile.Boundaries.ToArray())
            {
                if (!boundaryValidator.Validate(boundary))
                {
                    bndExtForceFile.RemoveBoundary(boundary);
                }
            }
        }

        private void RemoveInvalidLaterals(BndExtForceFileDTO bndExtForceFile)
        {
            foreach (LateralDTO lateral in bndExtForceFile.Laterals.ToArray())
            {
                if (!lateralValidator.Validate(lateral))
                {
                    bndExtForceFile.RemoveLateral(lateral);
                }
            }
        }
    }
}