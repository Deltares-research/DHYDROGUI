using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy
{
    /// <summary>
    /// Legacy loader for <see cref="RealTimeControlApplicationPlugin"/> version 3.7.0.
    /// Upon opening the project, the Path property of the <see cref="RealTimeControlModel"/> will be set,
    /// and the output restart files of the <see cref="RealTimeControlModel"/> will be retrieved from the
    /// database to be written to the output folder of the model.
    /// </summary>
    public sealed class RtcLegacyLoader37 : LegacyLoader
    {
        private static readonly ILogHandler logHandler = new LogHandler(Resources.RtcLegacyLoader37_migration_of_the_rtc_model, typeof(RtcLegacyLoader37));
        private readonly IDictionary<RealTimeControlModel, DbFile[]> restoreRestartData = new Dictionary<RealTimeControlModel, DbFile[]>();

        /// <summary>
        /// Called after initializing the migration.
        /// </summary>
        /// <param name="entity">The Real-Time Control model. </param>
        /// <param name="dbConnection">The database connection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dbConnection"/> or <paramref name="entity"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            Ensure.NotNull(entity, nameof(entity));
            Ensure.NotNull(dbConnection, nameof(dbConnection));

            var model = (RealTimeControlModel) entity;

            try
            {
                Execute(dbConnection, model);
            }
            catch (Exception e)
            {
                logHandler.ReportError(e.Message);
            }

            base.OnAfterInitialize(entity, dbConnection);
        }

        /// <summary>
        /// Called after the project migrated.
        /// Set the Path property of <see cref="RealTimeControlModel"/> and switched
        /// to this path, since it was missing in database.
        /// Restores the output restart files from the database in the output folders
        /// of each <see cref="RealTimeControlModel"/> in the specified <paramref name="project"/>.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="project"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterProjectMigrated(Project project)
        {
            Ensure.NotNull(project, nameof(project));

            foreach (RealTimeControlModel model in GetModels(project))
            {
                MigrateModel(model);
            }

            logHandler.LogReport();

            base.OnAfterProjectMigrated(project);
        }

        private void Execute(IDbConnection dbConnection, RealTimeControlModel model)
        {
            restoreRestartData[model] = new DataContext(dbConnection)
                                        .ExecuteQuery<DbFile>(RetrieveRestartFileDataQueryForModel(model.Name))
                                        .ToArray();
        }

        /// <summary>
        /// Retrieves the names and content of the output restart files
        /// for the model with the specified <paramref name="modelName"/>.
        /// </summary>
        private static string RetrieveRestartFileDataQueryForModel(string modelName) =>
            "SELECT RealTimeControlRestartFile.name, RealTimeControlRestartFile.content " +
            "FROM ((RealTimeControlRestartFile " +
            "INNER JOIN activities ON RealTimeControlRestartFile.rtc_restart_output_id = activities.project_item_id) " +
            "INNER JOIN rtc_models ON RealTimeControlRestartFile.rtc_restart_output_id = rtc_models.project_item_id) " +
            $"WHERE activities.name = '{modelName}';";

        private void MigrateModel(RealTimeControlModel model)
        {
            string rootPath = Path.GetDirectoryName(((IFileBased) model.Owner).Path);

            Ensure.NotNull(rootPath, nameof(rootPath));

            RestoreRestartFiles(rootPath, model);

            string className = Path.GetFileName(model.GetType().Name);
            string newPath = Path.Combine(rootPath, className + "-" + Guid.NewGuid());

            ((IFileBased) model).Path = newPath;
            ((IFileBased) model).SwitchTo(newPath);
        }

        private void RestoreRestartFiles(string rootPath, RealTimeControlModel model)
        {
            if (!restoreRestartData.TryGetValue(model, out DbFile[] restartFiles) ||
                !restartFiles.Any())
            {
                return;
            }

            string targetDirPath = Path.Combine(rootPath, model.Name, DirectoryNameConstants.OutputDirectoryName);
            Directory.CreateDirectory(targetDirPath);

            foreach (DbFile file in restartFiles)
            {
                file.WriteTo(targetDirPath);
            }
        }

        private static IEnumerable<RealTimeControlModel> GetModels(Project project) => project.RootFolder.GetAllItemsRecursive().OfType<RealTimeControlModel>();

        private class DbFile
        {
            private string name;
            private string content;

            public void WriteTo(string targetDirPath)
            {
                TryWriteFile(Path.Combine(targetDirPath, name), content);
            }

            private static void TryWriteFile(string filePath, string fileContent)
            {
                try
                {
                    File.WriteAllText(filePath, fileContent);
                }
                catch (IOException e)
                {
                    LogException(filePath, e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    LogException(filePath, e.Message);
                }
                catch (SecurityException e)
                {
                    LogException(filePath, e.Message);
                }
            }

            private static void LogException(string filePath, string exceptionMessage)
            {
                logHandler.ReportError(string.Format(Resources.RtcLegacyLoader37_An_error_occurred_while_writing_file, filePath, exceptionMessage));
            }
        }
    }
}