using System.IO;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="DeleteSourcePostOperationBehaviour"/> implements the
    /// post-behaviour to delete the file at the provided invoke source file
    /// path.
    /// </summary>
    /// <seealso cref="IIniPostOperationBehaviour" />
    public sealed class DeleteSourcePostOperationBehaviour : IniPostOperationBehaviour
    {
        public override void Invoke(Stream sourceFileStream,
                                    string sourceFilePath,
                                    IniData iniData,
                                    ILogHandler logHandler)
        {
            base.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);
            File.Delete(sourceFilePath);
        }
    }
}