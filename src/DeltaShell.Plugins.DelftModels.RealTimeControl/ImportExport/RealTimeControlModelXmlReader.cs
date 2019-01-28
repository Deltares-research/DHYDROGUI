using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlModelXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlModelXmlReader));

        public static RealTimeControlModel Read(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.ErrorFormat(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_, directoryPath);
                return null;
            }

            var rtcModel = new RealTimeControlModel();
            var controlGroups = rtcModel.ControlGroups;

            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            RealTimeControlRuntimeConfigXmlReader.Read(runTimeConfigFilePath, rtcModel);

            var dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(dataConfigFilePath, controlGroups);
            
            var toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            RealTimeControlToolsConfigXmlReader.Read(toolsConfigFilePath, controlGroups, connectionPoints);

            var timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
            RealTimeControlTimeSeriesXmlReader.Read(timeSeriesFilePath, controlGroups);

            var stateImportFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);
            RealTimeControlStateImportXmlReader.Read(stateImportFilePath, connectionPoints.OfType<Output>().ToList());

            return rtcModel;
        }
    }
}
