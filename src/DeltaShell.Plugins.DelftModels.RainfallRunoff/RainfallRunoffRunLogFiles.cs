using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.LogFileReading;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Responsible for reading the log files produced by running the <see cref="IRainfallRunoffModel"/> and creating
    /// <see cref="IDataItem"/> for visualization in the project tree.
    /// </summary>
    public sealed class RainfallRunoffRunLogFiles
    {
        // The name to display in the project tree
        private const string logFileDataItemDisplayName = "RR Log (sobek_3b.log)";

        // The name to display in the project tree
        private const string runReportDataItemDisplayName = "RR Run report (3b_bal.out)";

        // Used to read the log file.
        private readonly ILogFileReader logFileReader;

        private readonly IRainfallRunoffModel rainfallRunoffModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffRunLogFiles"/> type.
        /// </summary>
        /// <param name="logFileReader">The <see cref="ILogFileReader"/> used to read the log file contents. </param>
        /// <param name="rainfallRunoffModel">The <see cref="IRainfallRunoffModel"/> of which the log files must be visualized. </param>
        /// <exception cref="System.ArgumentNullException">is thrown if the <paramref name="logFileReader"/> is null. </exception>
        public RainfallRunoffRunLogFiles(ILogFileReader logFileReader, IRainfallRunoffModel rainfallRunoffModel)
        {
            Ensure.NotNull(logFileReader, nameof(logFileReader));
            Ensure.NotNull(rainfallRunoffModel, nameof(rainfallRunoffModel));

            this.logFileReader = logFileReader;
            this.rainfallRunoffModel = rainfallRunoffModel;
        }

        /// <summary>
        /// Clear the <see cref="IDataItem"/> representing the Run Report and Log file from the <see cref="IRainfallRunoffModel"/>.
        /// </summary>
        public void Clear()
        {
            RemoveDataItem(RainfallRunoffOutputFiles.LogFileName);
            RemoveDataItem(RainfallRunoffOutputFiles.RunReportFilename);
        }

        /// <summary>
        /// Add or update the logfiles <see cref="IDataItem"/> objects with the logfiles created after model has been run.
        /// </summary>
        /// <param name="outputPath">Location where the log files can be found. </param>
        /// <exception cref="System.ArgumentException">is thrown if the <paramref name="outputPath"/> is null or empty. </exception>
        public void ConnectLoggingFiles(string outputPath)
        {
            Ensure.NotNullOrEmpty(outputPath, nameof(outputPath));
            
            ConnectLog(outputPath);
            ConnectRunReport(outputPath);
        }

        private void RemoveDataItem(string name)
        {
            IDataItem dataItem = GetExistingDataItem(name);
            if (dataItem != null)
            {
                rainfallRunoffModel.DataItems.Remove(dataItem);
            }
        }

        private void ConnectLog(string outputPath)
        {
            var logPath = new FileInfo(Path.Combine(outputPath, RainfallRunoffOutputFiles.LogFileName));

            ConnectRainfallRunoffLog(logPath, logFileDataItemDisplayName);
        }
        
        private void ConnectRunReport(string outputPath)
        {
            var runReportPath = new FileInfo(Path.Combine(outputPath, RainfallRunoffOutputFiles.RunReportFilename));

            ConnectRainfallRunoffLog(runReportPath, runReportDataItemDisplayName);
        }

        private void ConnectRainfallRunoffLog(FileSystemInfo fileInfo, string displayName)
        {
            if (!fileInfo.Exists)
            {
                return;
            }

            IDataItem dataItem = GetExistingDataItem(fileInfo.Name);
            if (dataItem == null)
            {
                dataItem = CreateDataItem(fileInfo.Name, displayName);
            }

            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                ((TextDocument)dataItem.Value).Content = logFileReader.ReadCompleteStream(fileStream);
            }
        }

        private IDataItem GetExistingDataItem(string dataItemTagName)
        {
            return rainfallRunoffModel.DataItems.FirstOrDefault(di => string.Equals(di.Tag, dataItemTagName, StringComparison.InvariantCultureIgnoreCase));
        }

        private IDataItem CreateDataItem(string dataItemTagName, string displayName)
        {
            var textDocument = new TextDocument(true) { Name = displayName };

            var dataItem = new DataItem(textDocument, DataItemRole.Output, dataItemTagName);
            rainfallRunoffModel.DataItems.Add(dataItem);

            return dataItem;
        }
    }
}