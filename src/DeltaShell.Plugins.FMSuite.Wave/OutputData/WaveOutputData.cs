using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="IWaveOutputData"/> implements the interface for the wave
    /// output data component. This component is responsible for managing all
    /// Wave output domain concepts.
    /// </summary>
    public class WaveOutputData : IWaveOutputData
    {
        public string DataSourcePath { get; private set; } = null;
        public bool IsConnected => DataSourcePath != null;
        public bool IsStoredInWorkingDirectory { get; private set; } = false;

        public void ConnectTo(string dataSourcePath, bool isStoredInWorkingDirectory)
        {
            Ensure.NotNull(dataSourcePath, nameof(dataSourcePath));

            DataSourcePath = dataSourcePath;
            IsStoredInWorkingDirectory = isStoredInWorkingDirectory;
        }

        public void Disconnect()
        {
            DataSourcePath = null;
            IsStoredInWorkingDirectory = false;
        }
    }
}