using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.Common
{
    public static class FileBasedUtils
    {
        private const string InputDirectoryName = "input";
        private const string OutputDirectoryName = "output";

        /// <summary>
        /// Method for retrieving the non recursive paths of files and directories
        /// inside a directory.
        /// </summary>
        /// <param name="directory">
        /// The folder in which this method is collecting paths.
        /// </param>
        /// <returns> All absolute paths.</returns>
        /// <remarks> File paths inside the collected directories are not added to
        /// the array (non recursive).</remarks>
        public static string[] CollectNonRecursivePaths(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] directories = Directory.GetDirectories(directory);

            return files.Concat(directories).ToArray();
        }

        public static void CleanPersistentDirectories(DirectoryInfo persistedDataDirectory, ICompositeActivity compositeActivity)
        {
            if (!persistedDataDirectory.Exists)
            {
                return;
            }
            
            string hydroModelDirectory = Path.Combine(persistedDataDirectory.FullName, compositeActivity.Name);
            List<string> childModelNames = compositeActivity.Activities.OfType<IModel>().Select(a => a.Name).ToList();

            CleanPersistentDirectoryForCompositeModel(new DirectoryInfo(hydroModelDirectory), childModelNames);

            foreach (string childModelName in childModelNames)
            {
                var modelDirectoryInfo = new DirectoryInfo(Path.Combine(hydroModelDirectory, childModelName));
                CleanPersistentDirectoryForStandAloneModel(modelDirectoryInfo);
            }
        }
        
        public static void CleanPersistentDirectories(DirectoryInfo persistedDataDirectory, IModel model)
        {
            if (!persistedDataDirectory.Exists)
            {
                return;
            }
            
            var modelDirectoryInfo = new DirectoryInfo(Path.Combine(persistedDataDirectory.FullName, model.Name));
            CleanPersistentDirectoryForStandAloneModel(modelDirectoryInfo);
        }

        private static void CleanPersistentDirectoryForCompositeModel(DirectoryInfo compositeModelDirectoryInfo, IEnumerable<string> childModelNames)
        {
            if (!compositeModelDirectoryInfo.Exists)
            {
                return;
            }

            compositeModelDirectoryInfo.GetDirectories()
                                       .Where(d => !childModelNames.Contains(d.Name))
                                       .ForEach(d => FileUtils.DeleteIfExists(d.FullName));

            compositeModelDirectoryInfo.GetFiles().ForEach(f => FileUtils.DeleteIfExists(f.FullName));
        }

        private static void CleanPersistentDirectoryForStandAloneModel(DirectoryInfo modelDirectoryInfo)
        {
            if (!modelDirectoryInfo.Exists)
            {
                return;
            }

            modelDirectoryInfo.GetDirectories()
                              .Where(d => d.Name != InputDirectoryName && d.Name != OutputDirectoryName)
                              .ForEach(d => FileUtils.DeleteIfExists(d.FullName));

            modelDirectoryInfo.GetFiles().ForEach(f => FileUtils.DeleteIfExists(f.FullName));
        }
    }
}