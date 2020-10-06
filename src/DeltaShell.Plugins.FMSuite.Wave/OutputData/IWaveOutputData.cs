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
        string DataSourcePath { get; }

        /// <summary>
        /// Connects this <see cref="IWaveOutputData"/> to the specified path,
        /// this will read all supported files from the specified folder.
        /// </summary>
        /// <param name="dataSourcePath">The new path for the data source.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataSourcePath"/> is <c>null</c>.
        /// </exception>
        // TODO: add relevant exceptions
        void ConnectTo(string dataSourcePath);
    }
}