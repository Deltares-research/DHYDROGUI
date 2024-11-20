using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    /// <summary>
    /// Migrates projects with file format version 3.5.2 or lower to a newer format.
    /// </summary>
    public class WaterQualityModel352LegacyLoader : LegacyLoader
    {
        /// <summary>
        /// Called after project is migrated.
        /// Sets the correct OutputFolder on every <see cref="WaterQualityModel"/>
        /// in the specified <paramref name="project"/>.
        /// </summary>
        /// <param name="project"> The project. </param>
        public override void OnAfterProjectMigrated(Project project)
        {
            IEnumerable<WaterQualityModel> waqModels = project.RootFolder
                                                              .GetAllModelsRecursive()
                                                              .OfType<WaterQualityModel>();

            foreach (WaterQualityModel waterQualityModel in waqModels)
            {
                RemoveDataItemsForOldOutputTextDocumentsType(waterQualityModel.DataItems);
                MoveFilesFromOldWorkingDirectoryToPersistentOutputFolder(waterQualityModel);
            }
        }

        private static void MoveFilesFromOldWorkingDirectoryToPersistentOutputFolder(WaterQualityModel waterQualityModel)
        {
            string outputDirectoryPath = waterQualityModel.ModelSettings.OutputDirectory;
            string previousExplicitWorkingDirectory = waterQualityModel.ModelDataDirectory + "_output";

            bool workDirDoesNotExistOrIsEmpty = DoesNotExistOrIsEmpty(previousExplicitWorkingDirectory);
            if (DoesNotExistOrIsEmpty(outputDirectoryPath) && workDirDoesNotExistOrIsEmpty)
            {
                return;
            }

            if (!workDirDoesNotExistOrIsEmpty)
            {
                var folder = new FileBasedFolder(previousExplicitWorkingDirectory);
                folder.MoveTo(outputDirectoryPath, false);
                FileUtils.CreateDirectoryIfNotExists(previousExplicitWorkingDirectory);
            }

            waterQualityModel.OutputFolder = new FileBasedFolder(outputDirectoryPath);
        }

        private static void RemoveDataItemsForOldOutputTextDocumentsType(IEventedList<IDataItem> dataItems)
        {
            dataItems.Where(di => di.Tag == "BalanceOutputTag" || di.Tag == "MonitoringFileTag" ||
                                  di.Tag == "ListFileTag" || di.Tag == "ProcessFileTag")
                     .ToList().ForEach(di => dataItems.Remove(di));
        }

        private static bool DoesNotExistOrIsEmpty(string outputDirectoryPath)
        {
            return !Directory.Exists(outputDirectoryPath) ||
                   !Directory.EnumerateFileSystemEntries(outputDirectoryPath).Any();
        }
    }
}