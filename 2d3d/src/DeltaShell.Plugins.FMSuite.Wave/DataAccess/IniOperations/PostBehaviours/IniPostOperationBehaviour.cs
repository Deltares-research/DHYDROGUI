using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="IniPostOperationBehaviour"/> provides a base-class with
    /// the common parameter validation as specified in the
    /// <see cref="IIniPostOperationBehaviour"/> interface.
    /// </summary>
    /// <seealso cref="IIniPostOperationBehaviour" />
    public abstract class IniPostOperationBehaviour : IIniPostOperationBehaviour
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