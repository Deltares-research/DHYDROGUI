using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Aop;
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
        public IReadOnlyList<ReadOnlyTextFileData> DiagnosticFiles { get; private set; } = new List<ReadOnlyTextFileData>();
        public IReadOnlyList<ReadOnlyTextFileData> SpectraFiles { get; private set; } = new List<ReadOnlyTextFileData>();
        public IReadOnlyList<WavmFileFunctionStore> WavmFileFunctionStores { get; private set; } = new List<WavmFileFunctionStore>();
        public IReadOnlyList<WavhFileFunctionStore> WavhFileFunctionStores { get; private set; } = new List<WavhFileFunctionStore>();

        public void ConnectTo(string dataSourcePath, 
                              bool isStoredInWorkingDirectory,
                              ILogHandler logHandler = null)
        {
            Ensure.NotNull(dataSourcePath, nameof(dataSourcePath));

            var dataSourceInfo = new DirectoryInfo(dataSourcePath);

            if (!dataSourceInfo.Exists)
            {
                logHandler?.ReportErrorFormat(Resources.WaveOutputData_ConnectTo_The_directory_at__0__does_not_exist__disconnecting_output_instead_, 
                                              dataSourceInfo.FullName);
                Disconnect();
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
            DiagnosticFiles = harvester.HarvestDiagnosticFiles(dataSourceInfo, logHandler);

        private void ConnectSpectraFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            SpectraFiles = harvester.HarvestSpectraFiles(dataSourceInfo, logHandler);

        private void ConnectWavmFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavmFileFunctionStores = harvester.HarvestWavmFileFunctionStores(dataSourceInfo, logHandler);

        private void ConnectWavhFileFunctionStores(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            WavhFileFunctionStores = harvester.HarvestWavhFileFunctionStores(dataSourceInfo, logHandler);

        public void Disconnect()
        {
            DataSourcePath = null;
            IsStoredInWorkingDirectory = false;

            DiagnosticFiles = new List<ReadOnlyTextFileData>();
            SpectraFiles = new List<ReadOnlyTextFileData>();
            WavmFileFunctionStores = new List<WavmFileFunctionStore>();
            WavhFileFunctionStores = new List<WavhFileFunctionStore>();
        }
    }
}