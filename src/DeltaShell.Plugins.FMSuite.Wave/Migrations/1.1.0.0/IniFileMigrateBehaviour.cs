using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="IniFileMigrateBehaviour"/> defines the migration of a
    /// property containing a path to a INI file with dependents
    /// (i.e. containing references to other files).
    /// </summary>
    /// <seealso cref="FileMigrateBehaviour"/>
    public sealed class IniFileMigrateBehaviour : FileMigrateBehaviour
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IniFileMigrateBehaviour));

        private readonly IIniFileOperator migrator;

        /// <summary>
        /// Creates a new <see cref="IniFileMigrateBehaviour"/>.
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
        public IniFileMigrateBehaviour(string expectedKey,
                                            string relativeDirectory,
                                            string goalDirectory,
                                            IIniFileOperator migrator) :
            base(expectedKey, relativeDirectory, goalDirectory)
        {
            Ensure.NotNull(migrator, nameof(migrator));
            this.migrator = migrator;
        }

        protected override void HandleMigration(FileInfo filePathInfo,
                                                IniProperty property)
        {
            string migratingMsg = string.Format(Resources.IniFileMigrateBehaviour_HandleMigration_Migrating__0_, property.Value);
            var logHandler = new LogHandler(migratingMsg, log);

            migrator.Invoke(new FileStream(filePathInfo.FullName, FileMode.Open),
                            filePathInfo.FullName,
                            logHandler);

            logHandler.LogReport();

            property.Value = filePathInfo.Name;
        }
    }
}