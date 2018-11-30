using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelReader
    {
        private string runTimeConfigFilePath;
        private string dataConfigFilePath;
        private string toolsConfigFilePath;
        private string stateImportConfigFilePath;
        private string timeSeriesFilePath;

        public RealTimeControlModelReader(string directoryPath)
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
            var allElements = importElements.Concat(exportElements).ToList();

            var controlGroups = RealTimeControlDataConfigXmlReader.CreateControlGroupsFromXmlElementIDs(allElements);

            var rules = RealTimeControlDataConfigXmlReader.GetAllRulesFromXmlElementsAndAddToControlGroup(allElements, controlGroups);
            var conditions = RealTimeControlDataConfigXmlReader.GetAllConditionsFromXmlElementsAndAddToControlGroup(allElements, controlGroups);
            var inputs = (IList<Input>) RealTimeControlDataConfigXmlReader.GetConnectionPointsFromXmlElements(exportElements, RtcXmlTag.Input, targetModel);
            var outputs = (IList<Output>)RealTimeControlDataConfigXmlReader.GetConnectionPointsFromXmlElements(exportElements, RtcXmlTag.Output, targetModel);

            RealTimeControlDataConfigXmlReader.AddOutputAsInputForRelativeTimeRule(allElements,
                rules.OfType<RelativeTimeRule>().ToList(), outputs);
          
            // read ToolsConfigFile: couple inputs and outputs etc.

            model.ControlGroups.AddRange(controlGroups);

            return model;
        }
    }
}
