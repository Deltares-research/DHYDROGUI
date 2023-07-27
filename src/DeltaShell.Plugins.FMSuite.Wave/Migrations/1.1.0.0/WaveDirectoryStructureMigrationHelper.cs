using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="WaveDirectoryStructureMigrationHelper"/> provides the methods
    /// for the Directory Structure migration associated with file format version
    /// 1.2.0.0.
    /// </summary>
    public static class WaveDirectoryStructureMigrationHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveDirectoryStructureMigrationHelper));

        /// <summary>
        /// Gets an unique temporary migration directory name for the provided
        /// <paramref name="srcDirectory"/>.
        /// </summary>
        /// <param name="srcDirectory">The source directory.</param>
        /// <returns>
        /// An unique temporary directory name formatted as <c>srcDirectory.Name_tmp.{0}</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="srcDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="srcDirectory"/> parent is <c>null</c>.
        /// </exception>
        public static string GetTemporaryMigrationDirectoryName(DirectoryInfo srcDirectory)
        {
            Ensure.NotNull(srcDirectory, nameof(srcDirectory));

            if (srcDirectory.Parent == null)
            {
                throw new ArgumentException("Cannot create a temporary directory name if the parent folder is null.");
            }

            IEnumerable<string> folderNames =
                srcDirectory.Parent.GetDirectories()
                            .Select(x => x.Name);

            string nameFormat = srcDirectory.Name + "_tmp.{0}";
            return NamingHelper.GenerateUniqueNameFromList(nameFormat, true, folderNames);
        }

        /// <summary>
        /// Migrates the wave model associated with the specified
        /// <paramref name="mdwPath"/> to the directory structure associated
        /// with file format version 1.2.0.0 defined as:
        /// <code>
        /// └───waveModelName
        ///     ├───input
        ///     └───output
        /// </code>
        /// </summary>
        /// <param name="mdwPath">Path to the mdw file of the waves model to migrate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="mdwPath"/> is <c>null</c>.
        /// </exception>
        public static void MigrateFileStructure(string mdwPath)
        {
            Ensure.NotNull(mdwPath, nameof(mdwPath));

            var expectedMdwInfo = new FileInfo(mdwPath);

            if (!expectedMdwInfo.Exists)
            {
                log.Error(Resources.WaveDirectoryStructureMigrationHelper_MigrateFileStructure_ErrorMigrateFileStructure);
                return;
            }

            DirectoryInfo origModelDirectoryInfo =
                expectedMdwInfo.Directory;
            DirectoryInfo temporaryDirectory =
                CreateToTemporaryDirectory(origModelDirectoryInfo);

            MigrateMdw(mdwPath, origModelDirectoryInfo, temporaryDirectory);
            MigrateRemainingOutputFiles(mdwPath, origModelDirectoryInfo, temporaryDirectory);

            RemoveOldModelDirectory(origModelDirectoryInfo);
            RenameNewModelDirectory(temporaryDirectory, origModelDirectoryInfo.Name);

            RemoveOldExplicitWorkingDirectory(origModelDirectoryInfo);
        }

        private static DirectoryInfo CreateToTemporaryDirectory(DirectoryInfo oldModelDirectory)
        {
            string temporaryDirectoryName =
                GetTemporaryMigrationDirectoryName(oldModelDirectory);
            string temporaryDirectoryPath =
                Path.Combine(oldModelDirectory.Parent.FullName, temporaryDirectoryName);

            var temporaryDirectoryInfo = new DirectoryInfo(temporaryDirectoryPath);

            temporaryDirectoryInfo.Create();
            temporaryDirectoryInfo.CreateSubdirectory("input");
            temporaryDirectoryInfo.CreateSubdirectory("output");

            return temporaryDirectoryInfo;
        }

        private static void MigrateMdw(string mdwPath,
                                       DirectoryInfo origModelDirectoryInfo,
                                       DirectoryInfo goalDirectory)
        {
            string newInputDirectory = Path.Combine(goalDirectory.FullName, "input");
            string mdwFileName = Path.GetFileName(mdwPath);

            IDelftIniFileOperator migrator =
                MigratorInstanceCreator.CreateMdwMigrator(origModelDirectoryInfo.FullName, newInputDirectory);

            var fileStream = new FileStream(mdwPath, FileMode.Open);

            string logMessage = string.Format(Resources.WaveDirectoryStructureMigrationHelper_MigrateMdw_Migrating___0___to_1_2_0_0, mdwFileName);
            var logHandler = new LogHandler(logMessage, log);
            migrator.Invoke(fileStream, mdwPath, logHandler);
            logHandler.LogReport();
        }

        private static void MigrateRemainingOutputFiles(string mdwPath,
                                                        DirectoryInfo origModelDirectoryInfo,
                                                        DirectoryInfo goalDirectory)
        {
            FileInfo[] outputFiles = origModelDirectoryInfo.GetFiles("*", SearchOption.AllDirectories);

            if (outputFiles.Length <= 0)
            {
                return;
            }

            string mdwFileName = Path.GetFileName(mdwPath);
            string warningMessage = string.Format(Resources.WaveDirectoryStructureMigrationHelper_MigrateRemainingOutputFiles_Migrating_remaining_files_of___0___to_1_2_0_0,
                                                  mdwFileName);
            var logHandler = new LogHandler(warningMessage);

            string outputFilesString = string.Join(", ", outputFiles.Select(x => x.Name));
            logHandler.ReportWarningFormat(Resources.WaveDirectoryStructureMigrationHelper_MigrateRemainingOutputFiles_The_following_files_are_assumed_to_be_output_and_moved_to_the_new_output_folder___0_, outputFilesString,
                                           outputFilesString);

            string newOutputDirectory = Path.Combine(goalDirectory.FullName, "output");
            foreach (FileInfo outputFile in outputFiles)
            {
                string destFileName = Path.Combine(newOutputDirectory, outputFile.Name);
                outputFile.MoveTo(destFileName);
            }

            logHandler.LogReport();
        }

        private static void RemoveOldModelDirectory(DirectoryInfo origModelDirectoryInfo) =>
            FileUtils.DeleteIfExists(origModelDirectoryInfo.FullName);

        private static void RenameNewModelDirectory(DirectoryInfo temporDirectoryInfo, string originalModelName)
        {
            string modelFolderPath = Path.Combine(temporDirectoryInfo.Parent.FullName, originalModelName);
            temporDirectoryInfo.MoveTo(modelFolderPath);
        }

        private static void RemoveOldExplicitWorkingDirectory(DirectoryInfo originalModelDirectoryInfo)
        {
            string outputDirectoryName = originalModelDirectoryInfo.Name.Replace(' ', '_') +
                                         "_output";
            string explicitWorkingDirectoryPath =
                Path.Combine(originalModelDirectoryInfo.Parent.FullName, outputDirectoryName);

            FileUtils.DeleteIfExists(explicitWorkingDirectoryPath);
        }
    }
}