using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlStateImportXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlStateImportXmlReader));

        public static void Read(string stateImportFilePath, IList<Output> outputs)
        {
            if (!File.Exists(stateImportFilePath))
                Log.ErrorFormat(Resources.RealTimeControlStateImportXmlReader_Read_File___0___does_not_exist_, stateImportFilePath);

            if (outputs == null) return;

            var stateImportObject = (TreeVectorFileXML)DelftConfigXmlFileParser.Read(stateImportFilePath);

            var outputItems = stateImportObject.treeVector.Items.OfType<TreeVectorLeafXML>();

            foreach (var outputItem in outputItems)
            {
                var outputName = outputItem.id;
                var outputValue = double.Parse(outputItem.vector, System.Globalization.CultureInfo.InvariantCulture);

                var correspondingOutput = outputs.FirstOrDefault(o => o.Name == outputName);

                if (correspondingOutput == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlStateImportXmlReader_Read_Could_not_find_output_with_name___0___that_is_referenced_in_file___1____Please_check_file___2__, outputName, RealTimeControlXMLFiles.XmlImportState, RealTimeControlXMLFiles.XmlData);
                    continue;
                }

                correspondingOutput.Value = outputValue;
            }
        }
    }
}
