using System.Collections.Generic;

namespace DeltaShell.NGHS.Common.Restart
{
    /// <summary>
    /// <see cref="IRestartModel{TRestartFile}"/> provides the interface for models supporting restart files.
    /// </summary>
    public interface IRestartModel<TRestartFile> where TRestartFile: class, IRestartFile, new()
    {
        /// <summary>
        /// Gets the value indicating whether or not this model uses a restart file.
        /// </summary>
        bool UseRestart { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this model should write restart files.
        /// </summary>
        bool WriteRestart { get; set; }

        /// <summary>
        /// Gets or sets the restart input file.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">thrown when null is assigned</exception>
        TRestartFile RestartInput { get; set; }

        /// <summary>
        /// Gets the restart output files.
        /// </summary>
        IEnumerable<TRestartFile> RestartOutput { get; }

        /// <summary>
        /// Create a duplicate of the source instance and assign it to RestartInput
        /// </summary>
        /// <param name="source">The instance to clone</param>
        /// <exception cref="System.ArgumentNullException">thrown when null is assigned</exception>
        void SetRestartInputToDuplicateOf(TRestartFile source);
    }
}