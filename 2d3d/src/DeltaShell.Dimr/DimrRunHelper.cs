using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// DimrRunHelper contains generic logic for running models
    /// by using the DimrRunner or HydroModel run logic.
    /// </summary>
    public static class DimrRunHelper
    {
        /// <summary>
        /// DataItem tag name.
        /// </summary>
        public const string dimrRunLogfileDataItemTag = "DimrRunLog";

        private const string dimrRunLogfileName = "dimr_redirected.log";

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
        public static void ConnectDimrRunLogFile(IModel model, string dimrLogDirectory)
        {
            string completeDimrLogFilename = Path.Combine(dimrLogDirectory, dimrRunLogfileName);
            if (!File.Exists(completeDimrLogFilename))
            {
                return;
            }

            IDataItem logDataItem = GetLogDataItem(model);

            ((TextDocument) logDataItem.Value).Content = ReadLogFile(completeDimrLogFilename);
        }

        private static IDataItem GetLogDataItem(IModel model)
        {
            IDataItem logDataItem = model.DataItems.FirstOrDefault(di => di.Tag == dimrRunLogfileDataItemTag);

            //add an dimr run log output dataitem if needed.
            if (logDataItem == null)
            {
                var textDocument = new TextDocument(true) {Name = "Dimr Run Log"};

                logDataItem = new DataItem(textDocument, DataItemRole.Output, dimrRunLogfileDataItemTag);
                model.DataItems.Add(logDataItem);
            }

            return logDataItem;
        }

        private static string ReadLogFile(string completeDimrLogFilename)
        {
            var stringBuilder = new StringBuilder();

            using (Stream objStream = File.OpenRead(completeDimrLogFilename))
            {
                // Read data from file until read position is not equals to length of file
                while (objStream.Position != objStream.Length)
                {
                    // Read number of remaining bytes to read
                    long lRemainingBytes = objStream.Length - objStream.Position;

                    // If bytes to read greater than 2 mega bytes size create array of 2 mega bytes
                    // Else create array of remaining bytes
                    byte[] arrData = lRemainingBytes > 262144 ? new byte[262144] : new byte[lRemainingBytes];

                    // Read data from file
                    objStream.Read(arrData, 0, arrData.Length);

                    stringBuilder.Append(Encoding.UTF8.GetString(arrData, 0, arrData.Length));
                }
            }

            return stringBuilder.ToString();
        }
    }
}