using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlStateImportXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlStateImportXmlReader));

        public static void Read(string stateImportFilePath, IList<Output> outputs)
        {
            var stateImportObject = (TreeVectorFileXML)DelftConfigXmlFileParser.Read(stateImportFilePath);

            var outputItems = stateImportObject.treeVector.Items.OfType<TreeVectorLeafXML>();

            foreach (var outputItem in outputItems)
            {
                var outputName = outputItem.id;
                var outputValue = double.Parse(outputItem.vector, System.Globalization.CultureInfo.InvariantCulture);

                var correspondingOutput = outputs.FirstOrDefault(o => o.Name == outputName);

                if (correspondingOutput == null)
                {
                    Log.Warn($"Could not find output with name '{outputName}' that is referenced in file '{RealTimeControlXMLFiles.XmlImportState}'. " +
                             $"Please check file '{RealTimeControlXMLFiles.XmlData}'");
                    continue;
                }

                correspondingOutput.Value = outputValue;
            }
        }
    }
}
