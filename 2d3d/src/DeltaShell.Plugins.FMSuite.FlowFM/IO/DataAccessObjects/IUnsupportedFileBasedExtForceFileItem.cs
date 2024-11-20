using DelftTools.Utils.IO;
using DHYDRO.Common.IO.ExtForce;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public interface IUnsupportedFileBasedExtForceFileItem : IFileBased
    {
        ExtForceData UnsupportedExtForceFileItem { get; set; }
    }
}