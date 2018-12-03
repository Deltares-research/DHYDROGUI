using DelftTools.Hydro;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelXmlReader
    {
        private string runTimeConfigFilePath;
        private string dataConfigFilePath;
        private string toolsConfigFilePath;
        private string stateImportConfigFilePath;
        private string timeSeriesFilePath;

        public RealTimeControlModelXmlReader(string directoryPath)
        {
            runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            stateImportConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);
            timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
        }

        public RealTimeControlModel ImportOn(IHydroModel targetModel)
        {
            var rtcModel = new RealTimeControlModel();

            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(dataConfigFilePath, targetModel, rtcModel);

            RealTimeControlToolsConfigXmlReader.Read(toolsConfigFilePath, targetModel, rtcModel.ControlGroups, connectionPoints);

            return rtcModel;
        }
    }
}
