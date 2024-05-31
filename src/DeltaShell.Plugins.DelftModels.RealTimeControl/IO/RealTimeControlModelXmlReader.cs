using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DHYDRO.Common.Logging;
using Newtonsoft.Json;

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
            
            string xmlDir = GetXmlDirectory(directoryPath, logHandler);

            if (xmlDir is null)
            {
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
        
        /// <summary>
        /// Composes the XML directory of the RTC plugin. The location of the XML directory is provided by the settings.json file
        /// </summary>
        /// <param name="workDir">The path of the directory where the settings.json and XML files are located.</param>
        /// <param name="logHandler">The log handler that is used in this class</param>
        /// <returns>The directory of the XML files</returns>
        /// <remarks>Note that in most cases, settings.json and the XML files are located in the same directory. However,
        /// It may be possible that the settings.json specifies a different location for the XML files</remarks>
        private static string GetXmlDirectory(string workDir, ILogHandler logHandler)
        {
            string settingsJsonPath = Path.Combine(workDir, "settings.json");

            if (!File.Exists(settingsJsonPath))
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_, 
                                             settingsJsonPath);
                return workDir; // no settings.json? return the workDir, as this will be the location of the XML files 
            }
            
            string settingsJsonFile = File.ReadAllText(settingsJsonPath);
            var deserializedFile = JsonConvert.DeserializeObject<RealTimeControlXmlDirectoryLookup>(settingsJsonFile);
            string xmlLocation = deserializedFile?.XmlDirectory;
            
            if (xmlLocation is null) // check if the settings.json contains a "xmlDir" key
            {
                logHandler.ReportError(Resources.RealTimeControlModelXmlReader_GetXmlDirectory_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory);
                return null;
            }
            
            string xmlDirPath = Path.Combine(workDir, xmlLocation);
            return xmlDirPath;
        }

        private static void AddControlGroupsToRtcModel(IList<IControlGroup> controlGroups, RealTimeControlModel rtcModel)
        {
            rtcModel.ControlGroups.AddRange(controlGroups.Cast<ControlGroup>());
        }
    }
}