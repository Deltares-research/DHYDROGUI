using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        public const string DiaFileDataItemTag = "DiaFile";
        private string currentOutputDirectoryPath;

        /// <summary>
        /// Saves the output by either moving or copying the source output to the target output directory.
        /// </summary>
        /// <remarks> When a file is locked, we report an error and return. </remarks>
        private void SaveOutput()
        {
            if (string.IsNullOrEmpty(currentOutputDirectoryPath))
            {
                return;
            }

            var sourceOutputDirectory = new DirectoryInfo(currentOutputDirectoryPath);
            if (!sourceOutputDirectory.Exists)
            {
                currentOutputDirectoryPath = GetModelOutputDirectory();
                return;
            }

            var targetOutputDirectory = new DirectoryInfo(GetModelOutputDirectory());
            string sourceOutputDirectoryPath = sourceOutputDirectory.FullName;
            string targetOutputDirectoryPath = targetOutputDirectory.FullName;

            bool sourceIsWorkingDir = fileSystem.ArePathsEqual(sourceOutputDirectoryPath, WorkingOutputDirectoryPath);

            if (OutputIsEmpty && !HasOpenFunctionStores)
            {
                CleanDirectory(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    CleanDirectory(WorkingDirectoryPath);
                }

                currentOutputDirectoryPath = targetOutputDirectoryPath;

                return;
            }

            if (sourceOutputDirectoryPath == targetOutputDirectoryPath)
            {
                return;
            }

            //copy all files and subdirectories from source directory "output" to persistent directory "output"
            if (!FileUtils.IsDirectoryEmpty(sourceOutputDirectoryPath))
            {
                FileUtils.CreateDirectoryIfNotExists(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    List<string> lockedFiles = GetLockedFiles(WorkingDirectoryPath).ToList();

                    if (lockedFiles.Any())
                    {
                        ReportLockedFiles(lockedFiles);
                        return;
                    }

                    CleanDirectory(targetOutputDirectoryPath);
                    MoveAllContentDirectory(sourceOutputDirectory, targetOutputDirectoryPath);
                }
                else
                {
                    CleanDirectory(targetOutputDirectoryPath);
                    FileUtils.CopyAll(sourceOutputDirectory, targetOutputDirectory, string.Empty);
                }
            }
            
            currentOutputDirectoryPath = targetOutputDirectoryPath;
            
            bool originalOutputOutOfSync = OutputOutOfSync;
            ReconnectOutputFiles(currentOutputDirectoryPath, true);
            OutputOutOfSync = originalOutputOutOfSync;

            if (sourceIsWorkingDir)
            {
                CleanDirectory(WorkingDirectoryPath);
            }
        }

        private static void MoveAllContentDirectory(DirectoryInfo sourceDirectory, string targetDirectoryPath)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                MoveFile(file, targetDirectoryPath);
            }

            bool onSameVolume = Directory.GetDirectoryRoot(sourceDirectory.FullName)
                                         .Equals(Directory.GetDirectoryRoot(targetDirectoryPath));

            foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
            {
                MoveDirectory(directory, targetDirectoryPath, onSameVolume);
            }
        }

        private static void ReportLockedFiles(IEnumerable<string> filePaths)
        {
            string separator = Environment.NewLine + "- ";
            string lockedFilesMessage = separator + string.Join(separator, filePaths);
            Log.Error("There are one or more files locked, please close the following file(s) and save again:" +
                      lockedFilesMessage);
        }

        private IEnumerable<string> GetLockedFiles(string sourceDirectoryPath)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

            foreach (FileInfo file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string path = file.FullName;
                string parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(path));

                // Snapped feature files are locked when the map in the GUI is open, so we ignore and copy snapped files instead.
                if (parentDirectoryName != FileConstants.SnappedFeaturesDirectoryName && FileUtils.IsFileLocked(path))
                {
                    yield return path;
                }
            }
        }

        private static void MoveFile(FileInfo file, string targetDirectoryPath)
        {
            string targetPath = Path.Combine(targetDirectoryPath, file.Name);
            file.MoveTo(targetPath);
        }

        private static void MoveDirectory(DirectoryInfo sourceDirectoryInfo, string targetParentDirectoryPath,
                                          bool onSameVolume)
        {
            var targetDirectoryInfo =
                new DirectoryInfo(Path.Combine(targetParentDirectoryPath, sourceDirectoryInfo.Name));

            if (onSameVolume && sourceDirectoryInfo.Name != FileConstants.SnappedFeaturesDirectoryName)
            {
                sourceDirectoryInfo.MoveTo(targetDirectoryInfo.FullName);
            }
            else
            {
                FileUtils.CopyAll(sourceDirectoryInfo, targetDirectoryInfo, string.Empty);
            }
        }

        /// <summary>
        /// Removes all files and directories from the directory.
        /// </summary>
        /// <param name="directoryPath"> The directory path of the directory that needs to be cleaned. </param>
        private static void CleanDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                // Do NOT remove: when File Explorer is opened in the directory, an IO exeption is thrown.
                // There is no way of checking for this case, so we have to catch it. The second time it is called, it works fine.
                // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
                catch (IOException)
                {
                    directory.Delete(true);
                }
            }
        }
    }
}