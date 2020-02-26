using System;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// <see cref="IEnvironmentSetter"/> provides the interface to set Environment variables.
    /// </summary>
    public interface IEnvironmentSetter
    {
        /// <summary>
        /// Sets environment variable with key <paramref name="key"/> and value
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key of the Environment variable.</param>
        /// <param name="value">The value of the Environment variable.</param>
        /// <param name="target"> </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="key"/> contains a zero-length string,
        /// an initial hexadecimal zero character (0x00), or an equal sign ("=").
        /// or an error occurred during the execution of this operation.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission to perform this operation. 
        /// </exception>
        void SetVariable(string key, 
                         string value, 
                         EnvironmentVariableTarget target = EnvironmentVariableTarget.Process);
    }
}