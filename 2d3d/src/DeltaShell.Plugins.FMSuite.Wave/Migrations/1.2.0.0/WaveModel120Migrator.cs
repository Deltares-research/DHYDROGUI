using System;
using System.Data;
using System.Data.SQLite;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._2._0._0
{
    /// <summary>
    /// Migrator for the <see cref="WaveApplicationPlugin"/> to update the database to plugin version 1.3.0.0.
    /// </summary>
    public static class WaveModel120Migrator
    {
        private const string commandStr =
            // Disable the foreign key constraints.
            "PRAGMA foreign_keys = off; " +
            // Delete the WavmFileFunctionStore objects from the project_item table.
            "DELETE FROM project_item WHERE id IN (SELECT project_item_id FROM IDataItem WHERE value_type2 = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'); " +
            // Delete the WavmFileFunctionStore objects from the DataItem table.
            "DELETE FROM DataItem WHERE value_type = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'; " +
            // Delete the WavmFileFunctionStore objects from the IDataItem table.
            "DELETE FROM IDataItem WHERE value_type2 = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'; " +
            // Reset the model_list_index column of the IDataItem table with ascending integers.
            "UPDATE IDataItem SET model_list_index = (SELECT COUNT(model_list_index) FROM IDataItem AS B WHERE B.model_list_index < IDataItem.model_list_index AND B.model_id = IDataItem.model_id) WHERE model_list_index IS NOT NULL; " +
            // Delete the wavm_function_store table.
            "DROP TABLE IF EXISTS wavm_function_store; " +
            // Enable the foreign key constraints.
            "PRAGMA foreign_keys = on;";

        private static readonly Version postMigrationVersion = new Version(1, 3, 0, 0);

        /// <summary>
        /// Migrates the database for the <see cref="WaveApplicationPlugin"/> to be compatible with version 1.3.0.0.
        /// All projects with D-Waves plugin version lower than 1.3.0.0 will be migrated.
        /// </summary>
        /// <param name="dbConnection"> The database connection. </param>
        /// <param name="projectVersion"> The version of the project. </param>
        /// <param name="logHandler"> The optional log handler. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dbConnection"/> or <paramref name="projectVersion"/> is <c>null</c>.
        /// </exception>
        public static void Migrate(IDbConnection dbConnection, Version projectVersion, ILogHandler logHandler = null)
        {
            Ensure.NotNull(dbConnection, nameof(dbConnection));
            Ensure.NotNull(projectVersion, nameof(projectVersion));

            if (projectVersion >= postMigrationVersion)
            {
                return;
            }

            try
            {
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = commandStr;
                    command.ExecuteNonQuery();
                }
            }
            catch (SQLiteException e)
            {
                logHandler?.ReportError($"Something went wrong while updating the D-Waves plugin from version {projectVersion} to {postMigrationVersion}: '{e.Message}'");
            }
        }
    }
}