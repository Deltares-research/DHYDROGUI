using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy
{
    /// <summary>
    /// Legacy loader for <see cref="RealTimeControlApplicationPlugin"/> version 3.6.0.
    /// </summary>
    public class RtcLegacyLoader36 : LegacyLoader
    {
        private readonly Regex timeRegex = new Regex(@"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}");
        private const string restartFileName = "state_import.xml";
        private const string metaDataFileName = "metadata.xml";

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

            GetModels(project).ForEach(MigrateModel);

            base.OnAfterProjectMigrated(project);
        }

        private void MigrateModel(RealTimeControlModel model)
        {
            string rootPath = Path.GetDirectoryName(((IFileBased) model.Owner).Path);

            model.RestartOutput = RetrieveRestartOutput(rootPath, model.Name);

            RemoveExplicitWorkingDir(rootPath, model.Name);
        }

        private EventedList<RealTimeControlRestartFile> RetrieveRestartOutput(string rootPath, string modelName)
        {
            IList<RealTimeControlRestartFile> restartFiles = new List<RealTimeControlRestartFile>();

            foreach (string stateFilePath in SearchStateFiles(rootPath, modelName))
            {
                ZipFileUtils.Extract(stateFilePath, rootPath);

                string restartFilePath = Path.Combine(rootPath, restartFileName);
                string newFileName = GetNewFileName(stateFilePath);

                var restartFile = new RealTimeControlRestartFile(newFileName, File.ReadAllText(restartFilePath));

                File.Delete(Path.Combine(rootPath, metaDataFileName));
                File.Delete(stateFilePath);
                File.Delete(restartFilePath);

                restartFiles.Add(restartFile);
            }

            return new EventedList<RealTimeControlRestartFile>(restartFiles);
        }

        private static void RemoveExplicitWorkingDir(string rootPath, string modelName)
        {
            string rtcExplicitWorkingDir = Path.Combine(rootPath, modelName.Replace(" ", "_") + "_output");
            FileUtils.DeleteIfExists(rtcExplicitWorkingDir);
        }

        private string GetNewFileName(string stateFile)
        {
            string timeStr = timeRegex.Match(stateFile).Value.Replace("-", "");

            return $"rtc_{timeStr}.xml";
        }

        private static IEnumerable<string> SearchStateFiles(string dir, string modelName)
        {
            var reg = new Regex($"state_{modelName}_" + @"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}.*.zip$");

            return Directory.EnumerateFiles(dir).Where(f => reg.IsMatch(Path.GetFileName(f)));
        }

        private static IEnumerable<RealTimeControlModel> GetModels(Project project) => project.RootFolder.GetAllItemsRecursive().OfType<RealTimeControlModel>();
    }
}