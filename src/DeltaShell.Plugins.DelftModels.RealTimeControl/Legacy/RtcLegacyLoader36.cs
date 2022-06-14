using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy
{
    /// <summary>
    /// Legacy loader for <see cref="RealTimeControlApplicationPlugin"/> version 3.6.0.
    /// </summary>
    public class RtcLegacyLoader36 : LegacyLoader
    {
        private const string restartFileName = "state_import.xml";
        private const string metaDataFileName = "metadata.xml";
        private static readonly ILogHandler logHandler = new LogHandler("the migration of the D-RTC model", typeof(RtcLegacyLoader36));
        private static readonly Regex timeRegex = new Regex(@"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}");
        private readonly LegacyLoader nextLegacyLoader = new RtcLegacyLoader37();

        /// <summary>
        /// Called after the project migrated.
        /// Unpacks the state files, loads the restart files unto the <see cref="RealTimeControlModel"/> and cleans up the project
        /// folder.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="System.ArgumentNullException">
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

            nextLegacyLoader.OnAfterProjectMigrated(project);
        }

        private static void MigrateModel(RealTimeControlModel model)
        {
            string rootPath = Path.GetDirectoryName(((IFileBased) model.Owner).Path);

            ReorganizeRestartOutput(rootPath, model.Name);

            RemoveExplicitWorkingDir(rootPath, model.Name);

            logHandler.ReportWarning(string.Format(Resources.RtcLegacyLoader36_MigrateModel_was_migrated_to_the_newest_version_verify_the_restart_file_settings, model.Name));
        }

        private static void ReorganizeRestartOutput(string rootPath, string modelName)
        {
            foreach (string stateFilePath in SearchStateFiles(rootPath, modelName))
            {
                ZipFileUtils.Extract(stateFilePath, rootPath);

                string newFileName = GetNewFileName(stateFilePath);
                MoveRestartFile(rootPath, modelName, newFileName);

                TryDeleteFile(Path.Combine(rootPath, metaDataFileName));
                TryDeleteFile(stateFilePath);
            }
        }

        private static void MoveRestartFile(string rootPath, string modelName, string newFileName)
        {
            string restartFilePath = Path.Combine(rootPath, restartFileName);

            string targetDirPath = Path.Combine(rootPath, modelName, DirectoryNameConstants.OutputDirectoryName);
            string newFilePath = Path.Combine(targetDirPath, newFileName);

            if (File.Exists(newFilePath))
            {
                TryDeleteFile(restartFilePath);
                return;
            }

            FileUtils.CreateDirectoryIfNotExists(targetDirPath);
            File.Move(restartFilePath, newFilePath);
        }

        private static void RemoveExplicitWorkingDir(string rootPath, string modelName)
        {
            string rtcExplicitWorkingDir = Path.Combine(rootPath, modelName.Replace(" ", "_") + "_output");
            FileUtils.DeleteIfExists(rtcExplicitWorkingDir);
        }

        private static string GetNewFileName(string stateFile)
        {
            string timeStr = timeRegex.Match(stateFile).Value.Replace("-", "");

            return $"rtc_{timeStr}.xml";
        }

        private static IEnumerable<string> SearchStateFiles(string dir, string modelName)
        {
            var reg = new Regex($"state_{Regex.Escape(modelName)}_" + @"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}.*.zip$");

            return Directory.EnumerateFiles(dir).Where(f => reg.IsMatch(Path.GetFileName(f)));
        }

        private static IEnumerable<RealTimeControlModel> GetModels(Project project) => project.RootFolder.GetAllItemsRecursive().OfType<RealTimeControlModel>();

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }

            catch (IOException e)
            {
                LogException(filePath, e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                LogException(filePath, e.Message);
            }
        }

        private static void LogException(string filePath, string exceptionMessage)
        {
            logHandler.ReportError(string.Format(Resources.RtcLegacyLoader36_an_error_occurred_while_deleting_file, Path.GetFileName(filePath),
                                                 exceptionMessage));
        }
    }
}