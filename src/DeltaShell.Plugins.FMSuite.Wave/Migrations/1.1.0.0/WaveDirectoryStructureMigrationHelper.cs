using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.FMSuite.Wave.IO;

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
        /// Updates the paths of the <see cref="WavmFileFunctionStore"/> of the
        /// provided <paramref name="model"/> to <paramref name="modelPath"/> output.
        /// </summary>
        /// <param name="modelPath">The model path.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void UpdateWavmFileFunctionStorePaths(string modelPath, WaveModel model)
        {
            Ensure.NotNull(modelPath, nameof(modelPath));
            Ensure.NotNull(model, nameof(model));

            IEnumerable<WavmFileFunctionStore> functionStores = 
                GetWavmFunctionStoreDataItems(model).Select(x => (WavmFileFunctionStore) x.Value);

            // We assume at this point the wavm file function store exists at this path
            // If it does not exist after migration, we will remove the function store all
            // together.
            foreach (WavmFileFunctionStore functionStore in functionStores)
            {
                string wavmFileName = Path.GetFileName(functionStore.Path);
                string expectedMigratedWavmPath = Path.Combine(modelPath, "output", wavmFileName);
                functionStore.Path = expectedMigratedWavmPath;
            }
        }

        /// <summary>
        /// Try and parse the database path from the provided <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databasePath">The database path.</param>
        /// <returns>
        /// <c>True</c> if a database path could be retrieved and stored in
        /// <paramref name="databasePath"/>; <c>False</c> if no database path
        /// could be retrieved and null is stored in <paramref name="databasePath"/>.
        /// </returns>
        public static bool TryParseDatabasePath(string connectionString, out string databasePath)
        {
            databasePath = null;
            string[] keyValuePairs = connectionString.Split(';');

            foreach (string keyValueString in keyValuePairs)
            {
                if (TryParseDataSource(keyValueString, out string parsedPath))
                {
                    databasePath = parsedPath;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseDataSource(string keyValueString, out string parsedDataSourcePath)
        {
            parsedDataSourcePath = null;

            string[] keyValuePair = keyValueString.Split('=');
            string key = keyValuePair[0].Trim();
            string value = keyValuePair[1].Trim();

            if (key != "Data Source")
            {
                return false;
            }

            parsedDataSourcePath = value;
            return true;
        }

        /// <summary>
        /// Removes the invalid wavm function stores from the provided
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static void RemoveInvalidWavmFunctionStores(WaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            IEnumerable<IDataItem> wavmDataItems = GetWavmFunctionStoreDataItems(waveModel);
            foreach (IDataItem dataItem in wavmDataItems)
            {
                var wavmFunctionStore = (WavmFileFunctionStore) dataItem.Value;

                if (File.Exists(wavmFunctionStore.Path))
                {
                    continue;
                }

                wavmFunctionStore?.Close();
                waveModel.DataItems.Remove(dataItem);
            }
        }

        private static IEnumerable<IDataItem> GetWavmFunctionStoreDataItems(WaveModel model) =>
            WaveDomainHelper.GetAllDomains(model.OuterDomain)
                            .Select(domain => model.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + domain.Name))
                            .Where(di => di != null);
    }
}