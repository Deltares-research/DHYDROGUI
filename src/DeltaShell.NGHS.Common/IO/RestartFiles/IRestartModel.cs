using System.Collections.Generic;

namespace DeltaShell.NGHS.Common.IO.RestartFiles
{
    /// <summary>
    /// <see cref="IRestartModel"/> provides the interface for models supporting restart files.
    /// </summary>
    public interface IRestartModel
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
        /// Gets the restart input file.
        /// </summary>
        RestartFile RestartInput { get; }

        /// <summary>
        /// Gets the restart output files.
        /// </summary>
        IEnumerable<RestartFile> RestartOutput { get; }
    }
}