using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMModel110LegacyLoader : LegacyLoader
    {
        private const string NewOutputDirectoryName = "output";
        private const string OldWorkingDirectoryPostfix = "_output";
        private const string SnappedDirectoryName = "snapped";
        private const string OldOutputDirectoryNamePrefix = "DFM_OUTPUT_";
        private const string dflowfmDirectoryName = "dflowfm";

        private readonly LegacyLoader nextLegacyLoader = new WaterFlowFMModel120LegacyLoader();

        /// <summary>
        /// Called when [after project migrated]. Performs directory restructuring on old WaterFlowFM models.
        /// </summary>
        /// <param name="project"> The project. </param>
        public override void OnAfterProjectMigrated(Project project)
        {
            IEnumerable<WaterFlowFMModel> existingFMModels =
                project.RootFolder.GetAllItemsRecursive().OfType<WaterFlowFMModel>();

            foreach (WaterFlowFMModel waterFlowFmModel in existingFMModels)
            {
                DirectoryInfo projectDataDirectoryInfo =
                    RecursivelyGetDsProjDataDirectoryFromMduPath(project, waterFlowFmModel);
                string oldWorkingDirPath = Path.Combine(projectDataDirectoryInfo.FullName,
                                                        waterFlowFmModel.Name + OldWorkingDirectoryPostfix);

                PerformDirectoryRestructuring(waterFlowFmModel, oldWorkingDirPath);

                waterFlowFmModel.ConnectOutput(waterFlowFmModel.GetModelOutputDirectory());

                if (projectDataDirectoryInfo != null)
                {
                    CleanUpDirectories(projectDataDirectoryInfo, waterFlowFmModel, oldWorkingDirPath);
                }
            }

            nextLegacyLoader.OnAfterProjectMigrated(project);
        }

        private static void PerformDirectoryRestructuring(WaterFlowFMModel waterFlowFMModel, string oldWorkingDirPath)
        {
            string mduSavePath = waterFlowFMModel.GetMduSavePath();
            string currentOutputDirName = GetOldOutputDirectoryName(waterFlowFMModel.ModelDefinition);
            var currentWaqOutputDirName = $"DFM_DELWAQ_{waterFlowFMModel.Name}";

            if (waterFlowFMModel.MduFilePath != mduSavePath)
            {
                waterFlowFMModel.ExportTo(mduSavePath);
            }

            var modelDirectoryInfo = new DirectoryInfo(waterFlowFMModel.MduFilePath);
            while (modelDirectoryInfo != null && modelDirectoryInfo.Name != waterFlowFMModel.Name)
            {
                modelDirectoryInfo = modelDirectoryInfo.Parent;
            }

            string modelDirPath = modelDirectoryInfo?.FullName ?? waterFlowFMModel.GetMduDirectory();

            string currentOutputDirPath = Path.Combine(modelDirPath, currentOutputDirName);
            string targetOutputDirPath = Path.Combine(modelDirPath, NewOutputDirectoryName);

            FileUtils.CreateDirectoryIfNotExists(targetOutputDirPath, true);

            MoveOutputFromPreviousOutputDirectoryToNewLocation(currentOutputDirPath, modelDirPath, targetOutputDirPath);

            MoveWaqOutputDirectoryToNewLocation(modelDirPath, currentWaqOutputDirName, targetOutputDirPath);

            MoveSnappedOutputDirectoryToNewLocation(oldWorkingDirPath, currentOutputDirName, targetOutputDirPath);
        }

        private static void MoveOutputFromPreviousOutputDirectoryToNewLocation(
            string currentOutputDirPath, string modelDirPath,
            string targetOutputDirPath)
        {
            if (currentOutputDirPath != modelDirPath)
            {
                MoveAllContentOfOldOutputDirectoryToNewLocation(currentOutputDirPath, targetOutputDirPath);
                FileUtils.DeleteIfExists(currentOutputDirPath);
            }
            else
            {
                MoveOutputFilesAndDirectoriesInModelDirectoryToNewLocation(modelDirPath, targetOutputDirPath);
            }
        }

        private static void MoveOutputFilesAndDirectoriesInModelDirectoryToNewLocation(
            string modelDirPath, string targetOutputDirPath)
        {
            Directory.GetFiles(modelDirPath).ForEach(filePath =>
            {
                if (IsOutputFile(filePath))
                {
                    File.Move(filePath, Path.Combine(targetOutputDirPath, Path.GetFileName(filePath)));
                }
            });
            string snappedDirectory = Directory.GetDirectories(modelDirPath)
                                               .FirstOrDefault(d => Path.GetFileName(d) == SnappedDirectoryName);
            if (snappedDirectory != null)
            {
                Directory.Move(snappedDirectory, Path.Combine(targetOutputDirPath, SnappedDirectoryName));
            }
        }

        private static void MoveWaqOutputDirectoryToNewLocation(string modelDirPath, string outputWAQDirName,
                                                                string targetOutputDirPath)
        {
            string currentOutputWAQDirPath = Path.Combine(modelDirPath, outputWAQDirName);
            string targetOutputWaqDirPath = Path.Combine(targetOutputDirPath, outputWAQDirName);

            if (Directory.Exists(currentOutputWAQDirPath))
            {
                Directory.Move(currentOutputWAQDirPath, targetOutputWaqDirPath);
            }
        }

        private static void MoveSnappedOutputDirectoryToNewLocation(string oldWorkingDirPath,
                                                                    string outputDirectoryName,
                                                                    string targetOutputDirPath)
        {
            string sourceSnappedDirectoryPath =
                Path.Combine(oldWorkingDirPath, dflowfmDirectoryName, outputDirectoryName, SnappedDirectoryName);
            string targetDirectoryPath = Path.Combine(targetOutputDirPath, SnappedDirectoryName);

            if (!Directory.Exists(sourceSnappedDirectoryPath) || Directory.Exists(targetDirectoryPath))
            {
                return;
            }

            Directory.Move(sourceSnappedDirectoryPath, targetDirectoryPath);
        }

        private static void MoveAllContentOfOldOutputDirectoryToNewLocation(
            string currentOutputFMDirPath, string targetOutputDirPath)
        {
            if (!Directory.Exists(currentOutputFMDirPath))
            {
                return;
            }

            Directory.GetFiles(currentOutputFMDirPath).ForEach(sourceFilePath =>
            {
                string targetFilePath = Path.Combine(targetOutputDirPath, Path.GetFileName(sourceFilePath));
                File.Move(sourceFilePath, targetFilePath);
            });

            Directory.GetDirectories(currentOutputFMDirPath).ForEach(directoryPath =>
            {
                string targetDirectoryPath = Path.Combine(targetOutputDirPath, Path.GetFileName(directoryPath));
                Directory.Move(directoryPath, targetDirectoryPath);
            });
        }

        private static void CleanUpDirectories(DirectoryInfo projectDataDirectoryInfo,
                                               WaterFlowFMModel waterFlowFmModel, string oldWorkingDir)
        {
            FileBasedUtils.CleanPersistentDirectories(projectDataDirectoryInfo, waterFlowFmModel);
            FileUtils.DeleteIfExists(oldWorkingDir);
            FileUtils.CreateDirectoryIfNotExists(oldWorkingDir);
        }

        private static DirectoryInfo RecursivelyGetDsProjDataDirectoryFromMduPath(
            Project project, WaterFlowFMModel waterFlowFmModel)
        {
            var dsprojDataDirName = $"{project.Name}.dsproj_data";
            var projectDataDirectoryInfo = new DirectoryInfo(waterFlowFmModel.MduFilePath);
            while (projectDataDirectoryInfo != null && projectDataDirectoryInfo.Name != dsprojDataDirName)
            {
                projectDataDirectoryInfo = projectDataDirectoryInfo.Parent;
            }

            return projectDataDirectoryInfo;
        }

        private static bool IsOutputFile(string filePath)
        {
            var outputExtensions = new[]
            {
                ".out",
                ".dia",
                "_numlimdt.xyz",
                "_rst.nc",
                "_his.nc",
                "_map.nc",
                "_clm.nc",
                ".tek",
                "_timings.txt"
            };
            return outputExtensions.Any(ext => filePath.EndsWith(ext));
        }

        private static string GetOldOutputDirectoryName(WaterFlowFMModelDefinition modelDefinition)
        {
            string defaultName = OldOutputDirectoryNamePrefix + modelDefinition.ModelName;

            if (!modelDefinition.ContainsProperty(KnownProperties.OutputDir))
            {
                return defaultName;
            }

            string mduOutputDir =
                modelDefinition.GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

            if (string.IsNullOrEmpty(mduOutputDir))
            {
                return defaultName;
            }

            if (string.Equals(mduOutputDir, "."))
            {
                return "";
            }

            return mduOutputDir;
        }
    }
}