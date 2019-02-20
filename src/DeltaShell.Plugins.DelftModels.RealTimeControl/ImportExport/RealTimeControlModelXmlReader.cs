using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>Responsible for reading the XML files for RTC and building a RealTimeControlModel</summary>
    public static class RealTimeControlModelXmlReader
    {
        /// <summary>Reads the XML files in the specified directory path.</summary>
        /// <param name="directoryPath">The directory path of RTC</param>
        /// <returns>A RealTimeControl Model</returns>
        /// <remarks>If the path directory path does not exist, the method logs a message and returns null.</remarks>
        public static RealTimeControlModel Read(string directoryPath)
        {
            var logHandler = new LogHandler("Import of the Real-Time Control Model");

            if (!Directory.Exists(directoryPath))
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_,
                    directoryPath);
                logHandler.LogReport();
                return null;
            }

            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            var dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            var toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            var timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
            var stateImportFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);

            var rtcModel = new RealTimeControlModel();

            var runtimeConfigReader = new RealTimeControlRuntimeConfigXmlReader(logHandler);
            runtimeConfigReader.Read(runTimeConfigFilePath, rtcModel);

            var dataAndToolsConfigReader = new RealTimeControlDataAndToolsConfigXmlReader(logHandler);
            var controlGroups = dataAndToolsConfigReader.Read(dataConfigFilePath, toolsConfigFilePath, rtcModel.TimeStep);

            var timeSeriesReader = new RealTimeControlTimeSeriesXmlReader(logHandler);
            timeSeriesReader.Read(timeSeriesFilePath, controlGroups);

            var connectionPoints = GetAllConnectionPointsFromControlGroups(controlGroups);

            var stateImportReader = new RealTimeControlStateImportXmlReader(logHandler);
            stateImportReader.Read(stateImportFilePath, connectionPoints);

            AddControlGroupsToRtcModel(controlGroups, rtcModel);

            logHandler.LogReport();

            return rtcModel;
        }

        private static IList<ConnectionPoint> GetAllConnectionPointsFromControlGroups(IList<IControlGroup> controlGroups)
        {
            var connectionPoints = controlGroups
                .SelectMany(g => g.Inputs)
                .Concat<ConnectionPoint>(controlGroups.SelectMany(g => g.Outputs))
                .ToList();

            return connectionPoints;
        }

        private static void AddControlGroupsToRtcModel(IList<IControlGroup> controlGroups, RealTimeControlModel rtcModel)
        {
            rtcModel.ControlGroups.AddRange(controlGroups.Cast<ControlGroup>());
        }
    }
}