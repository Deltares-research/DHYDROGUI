using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.CommonTools.TextData;

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
        /// does not exist.
        /// </exception>
        IReadOnlyList<ReadOnlyTextFileData> HarvestDiagnosticFiles(DirectoryInfo outputDataDirectory,
                                                                   ILogHandler logHandler = null);

        /// <summary>
        /// Harvests the spectra files.
        /// </summary>
        /// <param name="outputDataDirectory">The output data directory.</param>
        /// <param name="logHandler">Log handler to note any mistakes.</param>
        /// <returns>
        /// A collection of spectra files obtained from the specified folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outputDataDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when the specified <paramref name="outputDataDirectory"/>
        /// does not exist.
        /// </exception>
        IReadOnlyList<ReadOnlyTextFileData> HarvestSpectraFiles(DirectoryInfo outputDataDirectory,
                                                                ILogHandler logHandler = null);
        
        /// <summary>
        /// Harvests the SWAN input files.
        /// </summary>
        /// <param name="outputDataDirectory">The output data directory.</param>
        /// <param name="logHandler">Log handler to note any mistakes.</param>
        /// <returns>
        /// A collection of SWAN input files obtained from the specified folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outputDataDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when the specified <paramref name="outputDataDirectory"/>
        /// does not exist.
        /// </exception>
        IReadOnlyList<ReadOnlyTextFileData> HarvestSwanFiles(DirectoryInfo outputDataDirectory,
                                                             ILogHandler logHandler = null);

        /// <summary>
        /// Harvests the wave map (wavm) file function stores.
        /// </summary>
        /// <param name="outputDataDirectory">The output data directory.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <returns>
        /// A collection of <see cref="IWavmFileFunctionStore"/> files obtained
        /// from the specified folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outputDataDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when the specified <paramref name="outputDataDirectory"/>
        /// does not exist.
        /// </exception>
        IReadOnlyList<IWavmFileFunctionStore> HarvestWavmFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                            ILogHandler logHandler = null);

        /// <summary>
        /// Harvests the wave history (wavh) file function stores.
        /// </summary>
        /// <param name="outputDataDirectory">The output data directory.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <returns>
        /// A collection of <see cref="IWavhFileFunctionStore"/> files obtained
        /// from the specified folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outputDataDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when the specified <paramref name="outputDataDirectory"/>
        /// does not exist.
        /// </exception>
        IReadOnlyList<IWavhFileFunctionStore> HarvestWavhFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                            ILogHandler logHandler = null);
    }
}