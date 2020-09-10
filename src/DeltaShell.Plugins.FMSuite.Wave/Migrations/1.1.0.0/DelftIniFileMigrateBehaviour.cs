using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using log4net;

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
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniFileMigrateBehaviour));

        private readonly string expectedKey;
        private readonly string relativeDirectory;
        private readonly string goalDirectory;
        private readonly IDelftIniMigrator migrator;

        /// <summary>
        /// Creates a new <see cref="DelftIniFileMigrateBehaviour"/>.
        /// </summary>
        /// <param name="expectedKey">The expected key.</param>
        /// <param name="relativeDirectory">The path relative to which property values are evaluated.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <param name="migrator">
        /// The migrator with which the property's file properties are migrated.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>
        /// </exception>
        public DelftIniFileMigrateBehaviour(string expectedKey,
                                            string relativeDirectory,
                                            string goalDirectory,
                                            IDelftIniMigrator migrator)
        {
            Ensure.NotNull(expectedKey, nameof(expectedKey));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));
            Ensure.NotNull(migrator, nameof(migrator));

            this.expectedKey = expectedKey;
            this.relativeDirectory = relativeDirectory;
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

            var filePathInfo = new FileInfo(Path.Combine(relativeDirectory, property.Value));

            if (filePathInfo.Exists)
                HandleMigration(filePathInfo, property);
            else
                HandleNotExists(filePathInfo, property, logHandler);
        }

        private void HandleNotExists(FileInfo filePathInfo,
                                     DelftIniProperty property, 
                                     ILogHandler logHandler)
        {
            var warningMsg = 
                $"The file associated with property {expectedKey}, {Path.GetFileName(property.Value)} at {filePathInfo.FullName}, does not exist and thus is not migrated.";
            logHandler?.ReportWarning(warningMsg);
        }

        private void HandleMigration(FileInfo filePathInfo, 
                                     DelftIniProperty property)
        {
            var logHandler = new LogHandler($"Migrating {property.Value}", log);

            string goalPath = Path.Combine(goalDirectory, Path.GetFileName(property.Value));

            migrator.MigrateFile(new FileStream(filePathInfo.FullName, FileMode.Open),
                                 filePathInfo.FullName, 
                                 goalPath, 
                                 logHandler);
            logHandler.LogReport();

            property.Value = filePathInfo.Name;
        }
    }
}