using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="DeleteSourcePostOperationBehaviour"/> implements the
    /// post-behaviour to delete the file at the provided invoke source file
    /// path.
    /// </summary>
    /// <seealso cref="IDelftIniPostOperationBehaviour" />
    public sealed class DeleteSourcePostOperationBehaviour : DelftIniPostOperationBehaviour
    {
        public override void Invoke(Stream sourceFileStream,
                                    string sourceFilePath,
                                    IList<DelftIniCategory> categories,
                                    ILogHandler logHandler)
        {
            base.Invoke(sourceFileStream, sourceFilePath, categories, logHandler);
            File.Delete(sourceFilePath);
        }
    }
}