using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;

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
        /// Migrates the specified wave model to the directory structure
        /// associated with file format version 1.2.0.0.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static void Migrate(WaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));

            if (waveModel.MdwFilePath == null)
            {
                return;
            }

            DirectoryInfo origModelDirectoryInfo = 
                (new FileInfo(waveModel.MdwFilePath)).Directory;

            DirectoryInfo temporaryDirectory = 
                MoveToTemporaryDirectory(origModelDirectoryInfo);

            CreateExpectedDirectoryStructure(origModelDirectoryInfo.Parent, 
                                             origModelDirectoryInfo.Name);


        }

        private static DirectoryInfo MoveToTemporaryDirectory(DirectoryInfo oldModelDirectory)
        {
            string temporaryDirectoryName = 
                GetTemporaryMigrationDirectoryName(oldModelDirectory);
            string temporaryDirectoryPath = 
                Path.Combine(oldModelDirectory.Parent.FullName, temporaryDirectoryName);

            oldModelDirectory.MoveTo(temporaryDirectoryPath);

            return new DirectoryInfo(temporaryDirectoryPath);
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
        /// Creates the expected 1.2.0.0 directory structure in the
        /// <paramref name=" parentDirectoryInfo"/>. The directory structure is
        /// defined as:
        ///
        /// <code>
        /// └───waveModelName
        ///     ├───input
        ///     └───output
        /// </code>
        /// </summary>
        /// <param name="parentDirectoryInfo">The parent directory information.</param>
        /// <param name="waveModelName">Name of the wave model.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void CreateExpectedDirectoryStructure(DirectoryInfo parentDirectoryInfo, 
                                                            string waveModelName)
        {
            Ensure.NotNull(parentDirectoryInfo, nameof(parentDirectoryInfo));
            Ensure.NotNull(waveModelName, nameof(waveModelName));

            DirectoryInfo modelDirectoryInfo = parentDirectoryInfo.CreateSubdirectory(waveModelName);
            modelDirectoryInfo.CreateSubdirectory("input");
            modelDirectoryInfo.CreateSubdirectory("output");
        }
    }
}