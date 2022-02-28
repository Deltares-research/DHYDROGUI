using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.LogFileReading;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// DimrRunHelper contains generic logic for running models
    /// by using the DimrRunner or HydroModel run logic.
    /// </summary>
    public class DimrRunHelper
    {
        private readonly ILogFileReader logFileReader;

        /// <summary>
        /// DataItem tag name.
        /// </summary>
        public const string DimrRunLogfileDataItemTag = "DimrRunLog";

        private const string dimrRunLogfileName = "dimr_redirected.log";

        public DimrRunHelper(ILogFileReader logFileReader)
        {
            Ensure.NotNull(logFileReader, nameof(logFileReader));
            
            this.logFileReader = logFileReader;
        }
        /// <summary>
        /// Method for reading the DimrRunLog file and storing the content
        /// in a TextDocument.
        /// </summary>
        /// <param name="model"> The model of the run</param>
        /// <param name="dimrLogDirectory">
        /// The directory where the log file should be after a run.
        /// </param>
        /// <remarks>
        /// If the log file does not exist, this method will do nothing.
        /// </remarks>
        public void ConnectDimrRunLogFile(IModel model, string dimrLogDirectory)
        {
            string completeDimrLogFilename = Path.Combine(dimrLogDirectory, dimrRunLogfileName);
            if (!File.Exists(completeDimrLogFilename))
            {
                return;
            }

            IDataItem logDataItem = GetLogDataItem(model);

            using (var fileStream = new FileStream(completeDimrLogFilename, FileMode.Open, FileAccess.Read))
            {
                ((TextDocument) logDataItem.Value).Content = logFileReader.ReadCompleteStream(fileStream);
            }
        }

        private static IDataItem GetLogDataItem(IModel model)
        {
            IDataItem logDataItem = model.DataItems.FirstOrDefault(di => di.Tag == DimrRunLogfileDataItemTag);

            //add an dimr run log output dataitem if needed.
            if (logDataItem == null)
            {
                var textDocument = new TextDocument(true) {Name = "Dimr Run Log"};

                logDataItem = new DataItem(textDocument, DataItemRole.Output, DimrRunLogfileDataItemTag);
                model.DataItems.Add(logDataItem);
            }

            return logDataItem;
        }
    }
}