using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="DelftIniPostOperationBehaviour"/> provides a base-class with
    /// the common parameter validation as specified in the
    /// <see cref="IDelftIniPostOperationBehaviour"/> interface.
    /// </summary>
    /// <seealso cref="IDelftIniPostOperationBehaviour" />
    public abstract class DelftIniPostOperationBehaviour : IDelftIniPostOperationBehaviour
    {
        public virtual void Invoke(Stream sourceFileStream, string sourceFilePath, IniData iniData, ILogHandler logHandler)
        {
            Ensure.NotNull(sourceFileStream, nameof(sourceFileStream));
            Ensure.NotNull(sourceFilePath, nameof(sourceFilePath));
            Ensure.NotNull(iniData, nameof(iniData));
            Ensure.NotNullOrEmpty(Path.GetFileName(sourceFilePath), nameof(sourceFilePath), "Cannot determine the file name.");
        }
    }
}