using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;

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

        public void ConnectTo(string dataSourcePath, 
                              bool isStoredInWorkingDirectory,
                              ILogHandler logHandler = null)
        {
            Ensure.NotNull(dataSourcePath, nameof(dataSourcePath));

            var dataSourceInfo = new DirectoryInfo(dataSourcePath);

            DataSourcePath = dataSourcePath;
            IsStoredInWorkingDirectory = isStoredInWorkingDirectory;
           
            ConnectDiagnosticFiles(dataSourceInfo, logHandler);
        }

        private void ConnectDiagnosticFiles(DirectoryInfo dataSourceInfo, ILogHandler logHandler) =>
            DiagnosticFiles = harvester.HarvestDiagnosticFiles(dataSourceInfo, logHandler);

        public void Disconnect()
        {
            DataSourcePath = null;
            IsStoredInWorkingDirectory = false;

            DiagnosticFiles = new List<ReadOnlyTextFileData>();
        }
    }
}