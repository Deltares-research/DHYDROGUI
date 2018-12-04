using DelftTools.Hydro;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlModelXmlReader
    {
        public static RealTimeControlModel Read(string directoryPath)
        {
            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            var dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            var toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            var stateImportConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);
            var timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);

            var rtcModel = new RealTimeControlModel();

            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(dataConfigFilePath, rtcModel);

            RealTimeControlToolsConfigXmlReader.Read(toolsConfigFilePath, rtcModel.ControlGroups, connectionPoints);

            return rtcModel;
        }
    }
}
