using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataCopyHandler"/> implements the interface used to
    /// copy output data to a new location with.
    /// </summary>
    /// <seealso cref="IWaveOutputDataCopyHandler" />
    public sealed class WaveOutputDataCopyHandler : IWaveOutputDataCopyHandler
    {
        public void CopyRunDataTo(DirectoryInfo sourceDirectoryInfo,
                                  DirectoryInfo targetDirectoryInfo, 
                                  ILogHandler logHandler = null)
        {
            Ensure.NotNull(targetDirectoryInfo, nameof(targetDirectoryInfo));
            Ensure.NotNull(sourceDirectoryInfo, nameof(sourceDirectoryInfo));

            ClearDirectory(targetDirectoryInfo);

            if (!sourceDirectoryInfo.Exists)
            {
                logHandler?.ReportWarningFormat(
                    Resources.WaveModel_CopyRunDataTo_The_output_source_path__0__does_not_exist__skipping_copying_output_data_, 
                    sourceDirectoryInfo.FullName);
                return;
            }

            // Under normal circumstances there should only be one .mdw file in
            // the working directory. As such we take the first. It might happened
            // that the user has messed with the data, in which case we try to 
            // inform the user.
            FileInfo mdwRunPath = sourceDirectoryInfo.EnumerateFiles("*.mdw")
                                                     .FirstOrDefault();

            if (mdwRunPath == null)
            {
                logHandler?.ReportWarningFormat(
                    Resources.WaveModel_CopyRunDataTo_No__mdw_path_could_be_found_in__0___skipping_copying_output_data_, 
                    sourceDirectoryInfo.FullName);
                return;
            }

            HashSet<string> inputFilePaths = 
                WaveOutputFileHelper.CollectInputFileNamesFromWorkingDirectoryMdw(mdwRunPath.FullName);

            foreach (FileInfo file in sourceDirectoryInfo.EnumerateFiles()
                                                         .Where(x => !inputFilePaths.Contains(x.FullName)))
            {
                file.CopyTo(Path.Combine(targetDirectoryInfo.FullName, file.Name));
            }
        }

        public void CopyOutputDataTo(DirectoryInfo sourceDirectoryInfo,
                                     DirectoryInfo targetDirectoryInfo,  
                                     ILogHandler logHandler = null)
        {
            Ensure.NotNull(targetDirectoryInfo, nameof(targetDirectoryInfo));
            Ensure.NotNull(sourceDirectoryInfo, nameof(sourceDirectoryInfo));

            if (!sourceDirectoryInfo.Exists)
            {
                logHandler?.ReportWarningFormat(
                    Resources.WaveModel_CopyRunDataTo_The_output_source_path__0__does_not_exist__skipping_copying_output_data_, 
                    sourceDirectoryInfo.FullName);
                return;
            }

            if (sourceDirectoryInfo.FullName == targetDirectoryInfo.FullName)
            {
                return;
            }

            FileUtils.CopyAll(sourceDirectoryInfo, targetDirectoryInfo, null);
        }

        private static void ClearDirectory(DirectoryInfo directoryInfo) => 
            FileUtils.CreateDirectoryIfNotExists(directoryInfo.FullName, true);
    }
}