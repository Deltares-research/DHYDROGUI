using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccess
{
    public interface IUnsupportedFileBasedExtForceFileItem : IFileBased

    {
        ExtForceFileItem UnsupportedExtForceFileItem { get; set; }
    }
}