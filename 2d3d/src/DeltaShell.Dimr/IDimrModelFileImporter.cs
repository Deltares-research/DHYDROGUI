using DelftTools.Shell.Core;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// Interface for importing DIMR model data from external formats.
    /// </summary>
    public interface IDimrModelFileImporter : IFileImporter
    {
        /// <summary>
        /// Returns whether the specified file in the DIMR configuration can be imported.
        /// </summary>
        /// <param name="path">The file path specified in the DIMR configuration.</param>
        /// <returns><c>true</c> if the specified file can be imported; <c>false</c> otherwise.</returns>
        bool CanImportDimrFile(string path);
    }
}