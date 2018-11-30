using System.Collections.Generic;
using DelftTools.Shell.Core;

namespace DeltaShell.Dimr
{
    public interface IDimrModelFileImporter : IFileImporter
    {
        /// <summary>
        /// Extension (without dot) of the master definition file for this importer
        /// </summary>
        string MasterFileExtension { get; }

        /// <summary>
        /// Sub folder relative to the Dimr root folder.
        /// </summary>
        IEnumerable<string> SubFolders { get; }
    }
}
