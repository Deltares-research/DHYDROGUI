using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public interface IUnsupportedFileBasedExtForceFileItem : IFileBased

    {
        ExtForceFileItem UnsupportedExtForceFileItem { get; set; }
    }
}