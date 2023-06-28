using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="IWaveOutputData"/> implements the interface for the wave
    /// output data component. This component is responsible for managing all
    /// Wave output domain concepts.
    /// </summary>
    [Entity]
    public class WaveOutputData : IWaveOutputData
    {
        private readonly IWaveOutputDataHarvester harvester;
        private readonly IWaveOutputDataCopyHandler copyHandler;

        public WaveOutputData(IWaveOutputDataHarvester harvester,
                              IWaveOutputDataCopyHandler copyHandler)
        {
            Ensure.NotNull(harvester, nameof(harvester));
            Ensure.NotNull(copyHandler, nameof(copyHandler));

            this.harvester = harvester;
            this.copyHandler = copyHandler;
        }

        public string DataSourcePath { get; private set; } = null;
        public bool IsConnected => DataSourcePath != null;
        public bool IsStoredInWorkingDirectory { get; private set; } = false;

        public IEventedList<ReadOnlyTextFileData> DiagnosticFiles { get; } = new EventedList<ReadOnlyTextFileData>();
        public IEventedList<ReadOnlyTextFileData> SpectraFiles { get; } = new EventedList<ReadOnlyTextFileData>();
        public IEventedList<ReadOnlyTextFileData> SwanFiles { get; } = new EventedList<ReadOnlyTextFileData>();
        public IEventedList<IWavmFileFunctionStore> WavmFileFunctionStores { get; } = new EventedList<IWavmFileFunctionStore>();
        public IEventedList<IWavhFileFunctionStore> WavhFileFunctionStores { get; } = new EventedList<IWavhFileFunctionStore>();

        public void ConnectTo(string dataSourcePath, 
                              bool isStoredInWorkingDirectory,
                              ILogHandler logHandler = null)
        {
            Ensure.NotNull(dataSourcePath, nameof(dataSourcePath));

            var dataSourceInfo = new DirectoryInfo(dataSourcePath);

            Disconnect();
            if (!dataSourceInfo.Exists)
            {
                logHandler?.ReportErrorFormat(Resources.WaveOutputData_ConnectTo_The_directory_at__0__does_not_exist__disconnecting_output_instead_, 
                                              dataSourceInfo.FullName);
                return;
            }

            DataSourcePath = dataSourcePath;
            IsStoredInWorkingDirectory = isStoredInWorkingDirectory;
           
            ConnectDiagnosticFiles(dataSourceInfo, logHandler);
            ConnectSpectraFiles(dataSourceInfo, logHandler);
            ConnectSwanFiles(dataSourceInfo, logHandler);
            ConnectWavmFileFunctionStores(dataSourceInfo, logHandler);
            ConnectWavhFileFunctionStores(dataSourceInfo, logHandler);
        }

        public void SwitchTo(string dataTargetPath, 
                             ILogHandler logHandler = null)
        {
            Ensure.NotNull(dataTargetPath, nameof(dataTargetPath));
            var dataTargetDirectoryInfo = new DirectoryInfo(dataTargetPath);

            if (!dataTargetDirectoryInfo.Exists)
            {
                Disconnect();
                logHandler?.ReportErrorFormat(Resources.WaveOutputData_ConnectTo_The_directory_at__0__does_not_exist__disconnecting_output_instead_, 
                                              dataTargetDirectoryInfo.FullName);
                return;
            }

            var dataSourceDirectoryInfo = new DirectoryInfo(DataSourcePath);
            if (IsStoredInWorkingDirectory)
            {
                copyHandler.CopyRunDataTo(dataSourceDirectoryInfo, 
                                          dataTargetDirectoryInfo,
                                          logHandler);
            }
            else
            {
                copyHandler.CopyOutputDataTo(dataSourceDirectoryInfo, 
                                             dataTargetDirectoryInfo,
                                             logHandler);
            }

            DataSourcePath = dataTargetPath;
            IsStoredInWorkingDirectory = false;
            UpdateOutputDataAfterSwitch();
        }

        private void ConnectDiagnosticFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            DiagnosticFiles.AddRange(harvester.HarvestDiagnosticFiles(dataSourceInfo, logHandler));

        private void ConnectSpectraFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            SpectraFiles.AddRange(harvester.HarvestSpectraFiles(dataSourceInfo, logHandler));
        
        private void ConnectSwanFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            SwanFiles.AddRange(harvester.HarvestSwanFiles(dataSourceInfo, logHandler));

        private void ConnectWavmFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavmFileFunctionStores.AddRange(harvester.HarvestWavmFileFunctionStores(dataSourceInfo, logHandler));

        private void ConnectWavhFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavhFileFunctionStores.AddRange(harvester.HarvestWavhFileFunctionStores(dataSourceInfo, logHandler));

        private void UpdateOutputDataAfterSwitch()
        {
            // Validate text files
            RemoveTextFileDataWithoutFiles(DiagnosticFiles);
            RemoveTextFileDataWithoutFiles(SpectraFiles);
            RemoveTextFileDataWithoutFiles(SwanFiles);

            // Validate function stores
            WavmFileFunctionStores.RemoveAllWhere(fs => !File.Exists(Path.Combine(DataSourcePath, Path.GetFileName(fs.Path))));
            foreach (IWavmFileFunctionStore wavmFileFunctionStore in WavmFileFunctionStores)
            {
                wavmFileFunctionStore.Path = Path.Combine(DataSourcePath, Path.GetFileName(wavmFileFunctionStore.Path));
            }

            WavhFileFunctionStores.RemoveAllWhere(fs => !File.Exists(Path.Combine(DataSourcePath, Path.GetFileName(fs.Path))));
            foreach (IWavhFileFunctionStore wavhFileFunctionStore in WavhFileFunctionStores)
            {
                wavhFileFunctionStore.Path = Path.Combine(DataSourcePath, Path.GetFileName(wavhFileFunctionStore.Path));
            }
        }

        private void RemoveTextFileDataWithoutFiles(IEventedList<ReadOnlyTextFileData> textFiles) =>
            textFiles.RemoveAllWhere(tfd => !File.Exists(Path.Combine(DataSourcePath, tfd.DocumentName)));

        public void Disconnect()
        {
            DataSourcePath = null;
            IsStoredInWorkingDirectory = false;

            DiagnosticFiles.Clear();
            SpectraFiles.Clear();
            SwanFiles.Clear();

            foreach (IWavmFileFunctionStore store in WavmFileFunctionStores)
            {
                store.Close();
            }
            WavmFileFunctionStores.Clear();

            foreach (IWavhFileFunctionStore store in WavhFileFunctionStores)
            {
                store.Close();
            }
            WavhFileFunctionStores.Clear();
        }
    }
}