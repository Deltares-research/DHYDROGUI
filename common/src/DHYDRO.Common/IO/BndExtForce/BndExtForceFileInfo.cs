namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// General information of the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceFileInfo
    {
        /// <summary>
        /// Gets or sets the external forcings file version.
        /// </summary>
        public string FileVersion { get; set; } = BndExtForceFileConstants.DefaultFileVersion;

        /// <summary>
        /// Gets or sets the external forcings file type.
        /// </summary>
        public string FileType { get; set; } = BndExtForceFileConstants.DefaultFileType;
    }
}