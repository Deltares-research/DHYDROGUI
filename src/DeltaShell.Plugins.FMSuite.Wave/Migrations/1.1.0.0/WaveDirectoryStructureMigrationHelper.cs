using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="WaveDirectoryStructureMigrationHelper"/> acts as a facade
    /// to the Directory Structure migration associated with file format version
    /// 1.2.0.0.
    /// </summary>
    public static class WaveDirectoryStructureMigrationHelper
    {
        /// <summary>
        /// Migrates the wave model associated with the specified
        /// <paramref name="mdwPath"/> to the directory structure associated
        /// with file format version 1.2.0.0.
        /// </summary>
        /// <param name="mdwPath">Path to the mdw file of the waves model to migrate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="mdwPath"/> is <c>null</c>.
        /// </exception>
        // TODO: add more validation for the mdw path?
        public static void Migrate(string mdwPath)
        {
            Ensure.NotNull(mdwPath, nameof(mdwPath));

            DirectoryInfo origModelDirectoryInfo = 
                (new FileInfo(mdwPath)).Directory;

            DirectoryInfo temporaryDirectory = 
                CreateToTemporaryDirectory(origModelDirectoryInfo);

            MigrateMdw(mdwPath, origModelDirectoryInfo, temporaryDirectory);
            MigrateRemainingOutputFiles(mdwPath, origModelDirectoryInfo, temporaryDirectory);
            
            RemoveOldModelDirectory(origModelDirectoryInfo);
            RenameNewModelDirectory(temporaryDirectory, origModelDirectoryInfo.Name);

            RemoveOldExplicitWorkingDirectory(origModelDirectoryInfo);
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
            string explicitWorkingDirectoryPath =
                Path.Combine(originalModelDirectoryInfo.Parent.FullName, 
                             $"{originalModelDirectoryInfo.Name}_output");

            FileUtils.DeleteIfExists(explicitWorkingDirectoryPath);
        }

        private static void MigrateMdw(string mdwPath, 
                                       DirectoryInfo origModelDirectoryInfo,
                                       DirectoryInfo goalDirectory)
        {
            string newInputDirectory = Path.Combine(goalDirectory.FullName, "input");
            string mdwFileName = Path.GetFileName(mdwPath);

            IDelftIniMigrator migrator = 
                MigratorFactory.CreateMdwMigrator(origModelDirectoryInfo.FullName, newInputDirectory);

            var fileStream = new FileStream(mdwPath, FileMode.Open);
            string targetFilePath = Path.Combine(newInputDirectory, mdwFileName);

            var logHandler = new LogHandler($"Migrating '{mdwFileName}' to 1.2.0.0");
            migrator.MigrateFile(fileStream, mdwPath, targetFilePath, logHandler);
            logHandler.LogReport();
        }

        private static void MigrateRemainingOutputFiles(string mdwPath, 
                                                        DirectoryInfo origModelDirectoryInfo, 
                                                        DirectoryInfo goalDirectory)
        {
            string mdwFileName = Path.GetFileName(mdwPath);
            var logHandler = new LogHandler($"Migrating remaining files of '{mdwFileName}' to 1.2.0.0");

            FileInfo[] outputFiles = origModelDirectoryInfo.GetFiles("*", SearchOption.AllDirectories);

            string outputFilesString = string.Join(", ", outputFiles.Select(x => x.Name));
            logHandler.ReportWarning($"The following files are assumed to be output and moved to the new output folder: {outputFilesString}");

            string newOutputDirectory = Path.Combine(goalDirectory.FullName, "output");
            foreach (FileInfo outputFile in outputFiles)
            {
                string destFileName = Path.Combine(newOutputDirectory, outputFile.Name);
                outputFile.MoveTo(destFileName);
            }

            logHandler.LogReport();
        }

        /// <summary>
        /// Creates a temporary directory according the expected 1.2.0.0
        /// directory structure in the. The directory structure is defined as:
        /// 
        /// <code>
        /// └───waveModelName
        ///     ├───input
        ///     └───output
        /// </code>
        ///
        /// </summary>
        /// <param name="oldModelDirectory">The old model directory.</param>
        /// <returns>
        /// The directory info describing the new temporary directory that is
        /// made according to the 1.2.0.0 directory structure.
        /// </returns>
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
    }
}