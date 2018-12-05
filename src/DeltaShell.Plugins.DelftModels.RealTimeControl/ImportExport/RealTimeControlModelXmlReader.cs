using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.IO;
using System.Linq;

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

            var stateImportFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);
            RealTimeControlStateImportXmlReader.Read(stateImportFilePath, connectionPoints.OfType<Output>().ToList());

            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            RealTimeControlRuntimeConfigXmlReader.Read(runTimeConfigFilePath, rtcModel);

            return rtcModel;
        }
    }
}
