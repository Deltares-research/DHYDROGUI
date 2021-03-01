using DelftTools.Shell.Core;

namespace DeltaShell.Dimr
{
    public interface IDimrModelFileImporter : IFileImporter
    {
        /// <summary>
        /// Extension (without dot) of the master definition file for this importer
        /// </summary>
        string MasterFileExtension { get; }
    }
}