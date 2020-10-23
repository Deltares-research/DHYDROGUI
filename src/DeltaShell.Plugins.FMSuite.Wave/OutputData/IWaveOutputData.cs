using System.Collections.Generic;
using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="IWaveOutputData"/> defines the interface for the wave
    /// output data component. This component is responsible for managing all
    /// Wave output domain concepts.
    /// </summary>
    public interface IWaveOutputData
    {
        /// <summary>
        /// Gets the path to the data source on disk where the output data
        /// is read from.
        /// </summary>
        /// <remarks>
        /// If the data is currently disconnected, then null is returned.
        /// </remarks>
        string DataSourcePath { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IWaveOutputData"/>
        /// is currently connected to an output folder.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets a value indicating whether the data of this <see cref="IWaveOutputData"/>
        /// is stored in working directory.
        /// </summary>
        bool IsStoredInWorkingDirectory { get; }

        /// <summary>
        /// Gets the diagnostic files.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be null. If no diagnostic files
        /// currently exist, or the <see cref="IWaveOutputData"/> is
        /// disconnected, then an empty list is returned.
        /// </remarks>
        IReadOnlyList<ReadOnlyTextFileData> DiagnosticFiles { get; }

        /// <summary>
        /// Connects this <see cref="IWaveOutputData"/> to the specified path,
        /// this will read all supported files from the specified folder.
        /// </summary>
        /// <param name="dataSourcePath">The new path for the data source.</param>
        /// <param name="isStoredInWorkingDirectory">Whether the provided dataSourcePath is in the working directory.</param>
        /// <param name="logHandler">optional log handler to report any problems with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataSourcePath"/> is <c>null</c>.
        /// </exception>
        void ConnectTo(string dataSourcePath, bool isStoredInWorkingDirectory, ILogHandler logHandler = null);

        /// <summary>
        /// Disconnects the output data from the current <see cref="DataSourcePath"/>.
        /// </summary>
        void Disconnect();
    }
}