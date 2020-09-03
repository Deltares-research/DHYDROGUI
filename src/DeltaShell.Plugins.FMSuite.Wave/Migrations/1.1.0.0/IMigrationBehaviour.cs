using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="IMigrationBehaviour"/> defines the migration of a single
    /// property.
    /// </summary>
    public interface IMigrationBehaviour
    {
        /// <summary>
        /// Migrates the property and returns the migrated result.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="logHandler">Optional log handler.</param>
        /// <returns>
        /// The migrated property, if this <see cref="IMigrationBehaviour"/>
        /// acts upon the specified <paramref name="property"/> else
        /// property will be returned.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// While not a technical requirement, it is expected that each
        /// <see cref="IMigrationBehaviour"/> acts upon one and only one
        /// property and not multiple.
        /// </remarks>
        DelftIniProperty MigrateProperty(DelftIniProperty property, ILogHandler logHandler);
    }
}