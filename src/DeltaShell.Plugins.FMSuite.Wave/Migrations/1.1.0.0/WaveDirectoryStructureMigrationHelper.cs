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
        public static void Migrate(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));
        }

        /// <summary>
        /// Gets an unique temporary migration directory name for the provided
        /// <paramref name="srcDirectory"/>.
        /// </summary>
        /// <param name="srcDirectory">The source directory.</param>
        /// <returns>
        /// An unique temporary directory name formatted as <c>srcDirectory.Name_tmp.{0}</c>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="srcDirectory"/> parent is <c>null</c>.
        /// </exception>
        public static string GetTemporaryMigrationDirectoryName(DirectoryInfo srcDirectory)
        {
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