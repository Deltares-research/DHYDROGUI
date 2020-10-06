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
        /// <summary>
        /// Creates a new <see cref="WaveOutputData"/>.
        /// </summary>
        /// <param name="dataSourcePath">The data source path.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public WaveOutputData(string dataSourcePath)
        {
            ConnectTo(dataSourcePath);
        }

        public string DataSourcePath { get; private set; }

        public void ConnectTo(string dataSourcePath)
        {
            Ensure.NotNull(dataSourcePath, nameof(dataSourcePath));

            DataSourcePath = dataSourcePath;
        }
    }
}