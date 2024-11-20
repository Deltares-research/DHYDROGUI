namespace DeltaShell.NGHS.Common.Restart
{
    /// <summary>
    /// <see cref="IRestartFile"/> provides the interface for a restart file
    /// </summary>
    public interface IRestartFile
    {
        /// <summary>
        /// Gets the name of the restart file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        bool IsEmpty { get; }
    }
}