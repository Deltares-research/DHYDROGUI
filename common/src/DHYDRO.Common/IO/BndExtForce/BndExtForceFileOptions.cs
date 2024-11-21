using Deltares.Infrastructure.API.Guards;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides options for reading and writing the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceFileOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceFileOptions"/> class.
        /// </summary>
        /// <param name="extForceFilePath">The path to the external forcings file.</param>
        public BndExtForceFileOptions(string extForceFilePath)
            : this(extForceFilePath, extForceFilePath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceFileOptions"/> class.
        /// </summary>
        /// <param name="extForceFilePath">The path to the external forcings file.</param>
        /// <param name="referenceFilePath">The path to the reference file, relative to which data files are written.</param>
        public BndExtForceFileOptions(string extForceFilePath, string referenceFilePath)
        {
            Ensure.NotNullOrWhiteSpace(extForceFilePath, nameof(extForceFilePath));
            Ensure.NotNullOrWhiteSpace(referenceFilePath, nameof(referenceFilePath));

            ExtForceFilePath = extForceFilePath;
            ReferenceFilePath = referenceFilePath;
        }

        /// <summary>
        /// Path to the external forcings file.
        /// </summary>
        public string ExtForceFilePath { get; }

        /// <summary>
        /// Path to which the data file references in the external forcings file are relative to.
        /// In practice, can be either the MDU file or the external forcings file.
        /// </summary>
        public string ReferenceFilePath { get; }

        /// <summary>
        /// Whether the path of the referenced files should be switched to the new file location.
        /// </summary>
        public bool SwitchTo { get; set; }
    }
}