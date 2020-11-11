using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
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

        public WaveOutputData(IWaveOutputDataHarvester harvester)
        {
            Ensure.NotNull(harvester, nameof(harvester));

            this.harvester = harvester;
        }

        public string DataSourcePath { get; private set; } = null;
        public bool IsConnected => DataSourcePath != null;
        public bool IsStoredInWorkingDirectory { get; private set; } = false;

        public IEventedList<ReadOnlyTextFileData> DiagnosticFiles { get; } = new EventedList<ReadOnlyTextFileData>();
        public IEventedList<ReadOnlyTextFileData> SpectraFiles { get; } = new EventedList<ReadOnlyTextFileData>();
        public IEventedList<WavmFileFunctionStore> WavmFileFunctionStores { get; } = new EventedList<WavmFileFunctionStore>();
        public IEventedList<WavhFileFunctionStore> WavhFileFunctionStores { get; } = new EventedList<WavhFileFunctionStore>();

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
            ConnectWavmFileFunctionStores(dataSourceInfo, logHandler);
            ConnectWavhFileFunctionStores(dataSourceInfo, logHandler);
        }

        private void ConnectDiagnosticFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            DiagnosticFiles.AddRange(harvester.HarvestDiagnosticFiles(dataSourceInfo, logHandler));

        private void ConnectSpectraFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            SpectraFiles.AddRange(harvester.HarvestSpectraFiles(dataSourceInfo, logHandler));

        private void ConnectWavmFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavmFileFunctionStores.AddRange(harvester.HarvestWavmFileFunctionStores(dataSourceInfo, logHandler));

        private void ConnectWavhFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavhFileFunctionStores.AddRange(harvester.HarvestWavhFileFunctionStores(dataSourceInfo, logHandler));

        public void Disconnect()
        {
            DataSourcePath = null;
            IsStoredInWorkingDirectory = false;

            DiagnosticFiles.Clear();
            SpectraFiles.Clear();
            WavmFileFunctionStores.Clear();
            WavhFileFunctionStores.Clear();
        }
    }
}