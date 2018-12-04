using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlModelXmlReader
    {
        public static RealTimeControlModel Read(string directoryPath)
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroups = rtcModel.ControlGroups;

            var dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(dataConfigFilePath, controlGroups);

            var toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            RealTimeControlToolsConfigXmlReader.Read(toolsConfigFilePath, controlGroups, connectionPoints);

            var timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
            RealTimeControlTimeSeriesXmlReader.Read(timeSeriesFilePath, controlGroups);

            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            var stateImportConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);

            return rtcModel;
        }
    }
}
