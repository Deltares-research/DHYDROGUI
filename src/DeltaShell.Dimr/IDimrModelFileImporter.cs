using System.Collections.Generic;
using DelftTools.Shell.Core;

namespace DeltaShell.Dimr
{
    public interface IDimrModelFileImporter : IFileImporter
    {
        string LibraryName { get; }
    }
}
