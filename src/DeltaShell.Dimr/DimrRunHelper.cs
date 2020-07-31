using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;

namespace DeltaShell.Dimr
{
    public static class DimrRunHelper
    {
        private const string DIMR_RUN_LOGFILE_NAME = "dimr_redirected.log";
        private const string DimrRunLogfileDataItemTag = "DimrRunLog";

        public static void ConnectDimrRunLogFile(IModel model, string dimrLogDirectory)
        {
            string completeDimrLogFilename = Path.Combine(dimrLogDirectory, DIMR_RUN_LOGFILE_NAME);
            if (!File.Exists(completeDimrLogFilename))
            {
                return;
            }

            //add an dimr run log output dataitem with the log...
            IDataItem logDataItem = model.DataItems.FirstOrDefault(di => di.Tag == DimrRunLogfileDataItemTag);
            if (logDataItem == null)
            {
                var textDocument = new TextDocument(true) {Name = "Dimr Run Log"};

                logDataItem = new DataItem(textDocument, DataItemRole.Output, DimrRunLogfileDataItemTag);
                model.DataItems.Add(logDataItem);
            }

            using (Stream objStream = File.OpenRead(completeDimrLogFilename))
            {
                // Read data from file
                byte[] arrData = {};
                var stringBuilder = new StringBuilder();
                // Read data from file until read position is not equals to length of file
                while (objStream.Position != objStream.Length)
                {
                    // Read number of remaining bytes to read
                    long lRemainingBytes = objStream.Length - objStream.Position;

                    // If bytes to read greater than 2 mega bytes size create array of 2 mega bytes
                    // Else create array of remaining bytes
                    if (lRemainingBytes > 262144)
                    {
                        arrData = new byte[262144];
                    }
                    else
                    {
                        arrData = new byte[lRemainingBytes];
                    }

                    // Read data from file
                    objStream.Read(arrData, 0, arrData.Length);

                    stringBuilder.Append(Encoding.UTF8.GetString(arrData, 0, arrData.Length));
                }

                ((TextDocument) logDataItem.Value).Content = stringBuilder.ToString();
            }
        }
    }
}