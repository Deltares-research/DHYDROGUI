using DeltaShell.NGHS.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// Interface for classes that can update legacy properties to their up to date version.
    /// </summary>
    public interface IPropertyUpdater
    {
        /// <summary>
        /// Updates legacy properties to their up-to-date version.
        /// </summary>
        /// <remarks>
        /// Sometimes other properties from the same section as the legacy property are
        /// required to correctly update the legacy property's value to the latest version.
        /// </remarks>
        /// <param name="oldPropertyKey">The key of the property to update.</param>
        /// <param name="newPropertyKey">The new key to use for the legacy property.</param>
        /// <param name="section">
        /// The <see cref="IniSection"/> to which the property to update belongs to that may contain
        /// data required for updating the legacy property's value.
        /// </param>
        /// <param name="logHandler">The log handler to which log messages can be added.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when any <see cref="IniProperty"/>, that is required for updating
        /// the value, is missing from the provided <paramref name="section"/>.
        /// </exception>
        void UpdateProperty(string oldPropertyKey,
                            string newPropertyKey,
                            IniSection section,
                            ILogHandler logHandler);
    }
}