using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.TextData;
using DHYDRO.Common.Logging;

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
        IEventedList<ReadOnlyTextFileData> DiagnosticFiles { get; }

        /// <summary>
        /// Gets the spectra files.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be null. If no spectra files
        /// currently exist, or the <see cref="IWaveOutputData"/> is
        /// disconnected, then an empty list is returned.
        /// </remarks>
        IEventedList<ReadOnlyTextFileData> SpectraFiles { get; }
        
        /// <summary>
        /// Gets the SWAN input files.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be null. If no SWAN files
        /// currently exist, or the <see cref="IWaveOutputData"/> is
        /// disconnected, then an empty list is returned.
        /// </remarks>
        IEventedList<ReadOnlyTextFileData> SwanFiles { get; }

        /// <summary>
        /// Gets the collection of <see cref="WavmFileFunctionStore"/> objects.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be null. If no Wavm File Function Stores
        /// currently exist, or the <see cref="IWaveOutputData"/> is
        /// disconnected, then an empty list is returned.
        /// </remarks>
        IEventedList<IWavmFileFunctionStore> WavmFileFunctionStores { get; }

        /// <summary>
        /// Gets the collection of <see cref="WavhFileFunctionStore"/> objects.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be null. If no Wavh File Function Stores
        /// currently exist, or the <see cref="IWaveOutputData"/> is
        /// disconnected, then an empty list is returned.
        /// </remarks>
        IEventedList<IWavhFileFunctionStore> WavhFileFunctionStores { get; }

        /// <summary>
        /// Connects this <see cref="IWaveOutputData"/> to the specified path.
        /// This will read all supported files from the specified folder.
        /// </summary>
        /// <param name="dataSourcePath">The new path for the data source.</param>
        /// <param name="isStoredInWorkingDirectory">
        /// Whether the provided <paramref name="dataSourcePath"/> is in the working directory.
        /// </param>
        /// <param name="logHandler">optional log handler to report any problems with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataSourcePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission to open <paramref name="dataSourcePath"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="dataSourcePath"/> contains invalid characters such as ", <, >, or |.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified <paramref name="dataSourcePath"/>, exceeds the system-defined maximum length.
        /// </exception>
        /// <remarks>
        /// If the path at <paramref name="dataSourcePath"/> does not exist, an error is logged
        /// and the output data is disconnected instead.
        /// </remarks>
        void ConnectTo(string dataSourcePath, bool isStoredInWorkingDirectory, ILogHandler logHandler = null);

        /// <summary>
        /// Switches this <see cref="IWaveOutputData"/> to the specified path.
        /// This will copy the underlying current output data but keep the same data in
        /// this <see cref="IWaveOutputData"/>.
        /// </summary>
        /// <param name="dataTargetPath">The data target path.</param>
        /// <param name="logHandler">Optional log handler to report any problems with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataTargetPath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when <see cref="IsConnected"/> is <c>false</c>.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission to open <paramref name="dataTargetPath"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="dataTargetPath"/> contains invalid characters such as ", <, >, or |.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified <paramref name="dataTargetPath"/>, exceeds the system-defined maximum length.
        /// </exception>
        /// <remarks>
        /// If the path at <paramref name="dataTargetPath"/> does not exist, an error is logged
        /// and the output data is disconnected instead.
        /// </remarks>
        /// <remarks>
        /// If the output data should be read from the provided folder, then <see cref="ConnectTo"/> should
        /// be used.
        /// </remarks>
        void SwitchTo(string dataTargetPath, ILogHandler logHandler = null);

        /// <summary>
        /// Disconnects the output data from the current <see cref="DataSourcePath"/>.
        /// </summary>
        void Disconnect();
    }
}