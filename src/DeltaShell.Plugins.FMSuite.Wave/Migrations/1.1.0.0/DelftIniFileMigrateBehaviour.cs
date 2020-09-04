using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="DelftIniFileMigrateBehaviour"/> defines the migration of a
    /// property containing a path to a delft ini file with dependents
    /// (i.e. containing references to other files).
    /// </summary>
    /// <seealso cref="IMigrationBehaviour" />
    public class DelftIniFileMigrateBehaviour : IMigrationBehaviour
    {
        private readonly string expectedKey;
        private readonly string goalDirectory;
        private readonly IDelftIniMigrator migrator;

        /// <summary>
        /// Creates a new <see cref="DelftIniFileMigrateBehaviour"/>.
        /// </summary>
        /// <param name="expectedKey">The expected key.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <param name="migrator">
        /// The migrator with which the property's file properties are migrated.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>
        /// </exception>
        public DelftIniFileMigrateBehaviour(string expectedKey,
                                            string goalDirectory,
                                            IDelftIniMigrator migrator)
        {
            Ensure.NotNull(expectedKey, nameof(expectedKey));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));
            Ensure.NotNull(migrator, nameof(migrator));

            this.expectedKey = expectedKey;
            this.goalDirectory = goalDirectory;
            this.migrator = migrator;
        }

        public void MigrateProperty(DelftIniProperty property, 
                                    ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Name != expectedKey || 
                property.Value.Trim() == string.Empty)
            {
                return;
            }

            if (File.Exists(property.Value))
                HandleMigration(property);
            else
                HandleNotExists(property, logHandler);
        }

        private void HandleNotExists(DelftIniProperty property, ILogHandler logHandler)
        {
            var warningMsg = 
                $"The file associated with property {expectedKey}, {property.Value}, does not exist, the property is set to an empty string.";
            logHandler?.ReportWarning(warningMsg);

            property.Value = string.Empty;
        }

        private void HandleMigration(DelftIniProperty property)
        {
            var logHandler = new LogHandler($"Migrating {property.Value}");
            migrator.MigrateFile(property.Value, goalDirectory, logHandler);
            logHandler.LogReport();

            property.Value = Path.Combine(goalDirectory, Path.GetFileName(property.Value));
        }
    }
}