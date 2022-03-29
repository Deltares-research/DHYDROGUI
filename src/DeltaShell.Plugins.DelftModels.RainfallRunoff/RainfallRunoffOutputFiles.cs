using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Manages the output files of a <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public sealed class RainfallRunoffOutputFiles
    {
        private static ILog log = LogManager.GetLogger(typeof(RainfallRunoffOutputFiles));
        
        private static readonly StringComparer stringComparer = StringComparer.InvariantCultureIgnoreCase;

        private static readonly IEnumerable<string> outputFileExtensions = new[]
        {
            ".hia",
            ".his",
            ".nc",
            ".out",
            ".txt",
            ".dbg",
            ".log",
            ".rtn",
        };

        private static readonly IEnumerable<string> outputFileInclusions = new[]
        {
            "RR-ready",
            "RSRR_OUT"
        };

        private static readonly IEnumerable<string> outputFileExclusions = new[]
        {
            "runoff.out",
            "sobek_3b.dbg"
        };

        private FileInfo[] outputFiles = Array.Empty<FileInfo>();
        private DirectoryInfo directory;

        /// <summary>
        /// Gets the name of the Log file produced after running the <see cref="IRainfallRunoffModel"/>
        /// </summary>
        public const string LogFileName = "sobek_3b.log";

        /// <summary>
        /// Gets the name of the run report file produced after running the <see cref="IRainfallRunoffModel"/>
        /// </summary>
        public const string RunReportFilename = "3b_bal.out";

        /// <summary>
        /// Sets the directory to the provided path.
        /// </summary>
        /// <param name="directoryPath"> The path to the directory. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="directoryPath"/> is <c>null</c> or empty, or contains invalid characters such as ", &, or
        /// |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Thrown when the specified <paramref name="directoryPath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when <paramref name="directoryPath"/> does not exist.
        /// </exception>
        public void SetDirectory(string directoryPath)
        {
            Ensure.NotNullOrEmpty(directoryPath, nameof(directoryPath));

            directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException(directory.FullName);
            }

            outputFiles = FindOutputFiles(directory);
        }

        /// <summary>
        /// Clears the output files.
        /// </summary>
        public void Clear() => outputFiles = Array.Empty<FileInfo>();

        /// <summary>
        /// Copies the files into the specified <paramref name="directoryPath"/>,
        /// overwriting files that already exist.
        /// </summary>
        /// <param name="directoryPath"> The destination directory. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="directoryPath"/> is <c>null</c> or empty, or contains invalid characters such as ", &, or
        /// |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Thrown when the specified <paramref name="directoryPath"/> exceeds the system-defined maximum length.
        /// </exception>
        public void CopyTo(string directoryPath)
        {
            Ensure.NotNullOrEmpty(directoryPath, nameof(directoryPath));

            var targetDirectory = new DirectoryInfo(directoryPath);
            if (targetDirectory.EqualsDirectory(directory))
            {
                return;
            }

            for (var i = 0; i < outputFiles.Length; i++)
            {
                outputFiles[i] = outputFiles[i].CopyToDirectory(targetDirectory, overwrite: true);
            }
        }

        /// <summary>
        /// Deletes any output files present in the given <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath">The directory path where the output should be deleted from.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="directoryPath"/> is <c>null</c> or empty, or contains invalid characters such as ", &, or
        /// |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Thrown when the specified <paramref name="directoryPath"/> exceeds the system-defined maximum length.
        /// </exception>
        public void DeleteOutputFiles(string directoryPath)
        {
            Ensure.NotNullOrEmpty(directoryPath, nameof(directoryPath));
            
            SetDirectory(directoryPath);
            
            foreach (FileInfo outputFile in FindOutputFiles(directory))
            {
                TryDeleteFile(outputFile.FullName);
            }

            Clear();
        }

        private static FileInfo[] FindOutputFiles(DirectoryInfo directory) => directory.GetFiles().Where(ShouldBeIncluded).ToArray();

        private static bool ShouldBeIncluded(FileSystemInfo file) => !ShouldBeExcluded(file) &&
                                                                     (outputFileInclusions.Contains(file.Name, stringComparer) ||
                                                                      outputFileExtensions.Contains(file.Extension, stringComparer));

        private static bool ShouldBeExcluded(FileSystemInfo file) => outputFileExclusions.Contains(file.Name, stringComparer);

        private static void TryDeleteFile(string filepath)
        {
            if (FileUtils.IsFileLocked(filepath))
            {
                log.Error(string.Format(Resources.RainfallRunoffOutputFiles_Could_not_delete_file, filepath));
            }
            else
            {
                File.Delete(filepath);
            }
        }
    }
}