
using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="IWaveOutputDataHarvester"/> defines the interface
    /// with which to obtain the relevant wave output files from a
    /// given directory.
    /// </summary>
    public interface IWaveOutputDataHarvester
    {
        /// <summary>
        /// Harvests the diagnostic files from the folder specified with
        /// <paramref name="outputDataDirectory"/>.
        /// </summary>
        /// <param name="outputDataDirectory">The output data directory.</param>
        /// <param name="logHandler">Log handler to note any mistakes.</param>
        /// <returns>
        /// A collection of diagnostic files obtained from the specified folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outputDataDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when the specified <paramref name="outputDataDirectory"/>
        /// does not exists.
        /// </exception>
        IReadOnlyList<ReadOnlyTextFileData> HarvestDiagnosticFiles(DirectoryInfo outputDataDirectory, 
                                                                   ILogHandler logHandler = null);
    }
}