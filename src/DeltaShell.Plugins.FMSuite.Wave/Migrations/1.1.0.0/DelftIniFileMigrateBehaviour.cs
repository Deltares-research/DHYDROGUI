using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="DelftIniFileMigrateBehaviour"/> defines the migration of a
    /// property containing a path to a delft ini file with dependents
    /// (i.e. containing references to other files).
    /// </summary>
    /// <seealso cref="FileMigrateBehaviour"/>
    public sealed class DelftIniFileMigrateBehaviour : FileMigrateBehaviour
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniFileMigrateBehaviour));

        private readonly IDelftIniFileOperator migrator;

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
                                            IDelftIniFileOperator migrator) :
            base(expectedKey, relativeDirectory, goalDirectory)
        {
            Ensure.NotNull(migrator, nameof(migrator));
            this.migrator = migrator;
        }

        protected override void HandleMigration(FileInfo filePathInfo,
                                                DelftIniProperty property)
        {
            string migratingMsg = string.Format(Resources.DelftIniFileMigrateBehaviour_HandleMigration_Migrating__0_, property.Value);
            var logHandler = new LogHandler(migratingMsg, log);

            migrator.Invoke(new FileStream(filePathInfo.FullName, FileMode.Open),
                            filePathInfo.FullName,
                            logHandler);

            logHandler.LogReport();

            property.Value = filePathInfo.Name;
        }
    }
}