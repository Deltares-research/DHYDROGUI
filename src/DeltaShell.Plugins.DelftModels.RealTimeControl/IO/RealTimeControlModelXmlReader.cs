using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>Responsible for reading the XML files for RTC and building a RealTimeControlModel</summary>
    public static class RealTimeControlModelXmlReader
    {
        /// <summary>Reads the XML files in the specified directory path.</summary>
        /// <param name="directoryPath">The directory path of RTC</param>
        /// <returns>A RealTimeControl Model</returns>
        /// <remarks>If the path directory path does not exist, the method logs a message and returns null.</remarks>
        /// <remarks>
        /// Import for restart/state files is not yet supported.
        /// The use restart option is automatically set to false after importing.
        /// for more information please review issue SOBEK3-1704
        /// </remarks>
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

            string runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXmlFiles.XmlRuntime);
            string dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXmlFiles.XmlData);
            string toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXmlFiles.XmlTools);
            string timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXmlFiles.XmlTimeSeries);

            var rtcModel = new RealTimeControlModel();

            logHandler.ReportInfo(Resources.RealTimeControlModelXmlReader_Please_note_that_Use_Restart_option_in_D_RTC_is_set_to_False);

            var runtimeConfigReader = new RealTimeControlRuntimeConfigXmlReader(logHandler);
            runtimeConfigReader.Read(runTimeConfigFilePath, rtcModel);

            var dataAndToolsConfigReader = new RealTimeControlDataAndToolsConfigXmlReader(logHandler);
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(dataConfigFilePath, toolsConfigFilePath, rtcModel.TimeStep);

            var timeSeriesReader = new RealTimeControlTimeSeriesXmlReader(logHandler);
            timeSeriesReader.Read(timeSeriesFilePath, controlGroups);

            AddControlGroupsToRtcModel(controlGroups, rtcModel);

            logHandler.LogReport();

            return rtcModel;
        }

        private static void AddControlGroupsToRtcModel(IList<IControlGroup> controlGroups, RealTimeControlModel rtcModel)
        {
            rtcModel.ControlGroups.AddRange(controlGroups.Cast<ControlGroup>());
        }
    }
}