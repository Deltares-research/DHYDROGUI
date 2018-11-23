using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlXmlModelImporter
    {
        private string runTimeConfigFilePath;
        private string dataConfigFilePath;
        private string toolsConfigFilePath;
        private string stateImportConfigFilePath;
        private string timeSeriesFilePath;

        public RealTimeControlXmlModelImporter(string directoryPath)
        {
            runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            stateImportConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);
            timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
        }

        public RealTimeControlModel ImportOn(IHydroModel targetModel)
        {
            var model = new RealTimeControlModel();
            
            var dataConfigObject = (RTCDataConfigXML)DelftConfigXmlFileParser.Read(dataConfigFilePath);

            var importElements = dataConfigObject.importSeries.timeSeries;
            var exportElements = dataConfigObject.exportSeries.timeSeries;

            // import
            var inputs = (IList<Input>)RealTimeControlDataConfigXmlReader.GetConnectionPointsFromXmlElements(importElements, RtcDataConfigTag.Input, targetModel);
            var timeRules = RealTimeControlDataConfigXmlReader.GetTimeRulesFromXmlElements(importElements);
            var timeConditions = RealTimeControlDataConfigXmlReader.GetTimeConditionsFromXmlElements(importElements);
            var outputsAsInputs = RealTimeControlDataConfigXmlReader.GetOutputsAsInputsFromXmlElements(importElements, targetModel);

            // export
            var outputs = (IList<Output>)RealTimeControlDataConfigXmlReader.GetConnectionPointsFromXmlElements(exportElements, RtcDataConfigTag.Output, targetModel);
            var standardConditionsExport = RealTimeControlDataConfigXmlReader.GetStandardConditionsFromXmlElements(exportElements);
            var relativeTimeRules = RealTimeControlDataConfigXmlReader.GetRelativeTimeRulesFromXmlElements(exportElements);

            return model;
        }
    }
}
