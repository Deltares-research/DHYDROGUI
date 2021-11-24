using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using Newtonsoft.Json;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>Responsible for reading the XML files for RTC and building a RealTimeControlModel</summary>
    public static class RealTimeControlModelXmlReader
    {
        /// <summary>
        /// Constructs a <see cref="RealTimeControlModel"/> by reading XML files
        /// </summary>
        /// <param name="directoryPath">The path containing either the settings.json or the XML files</param>
        /// <returns>
        /// - <c>Null</c> if the <paramref name="directoryPath"/> doesn't contain a settings.json, or <br/>
        /// - <c>Null</c> if the settings.json doesn't contain a <c>xmlDir</c> key, or <br/>
        /// - <c><see cref="RealTimeControlModel"/> when succesfully reading the XML files</c>
        /// </returns>
        /// <remarks>
        /// A error is shown to the user if the method returns <c>Null</c>
        /// </remarks>
        public static RealTimeControlModel Read(string directoryPath)
        {
            var logHandler = new LogHandler("Import of the Real-Time Control Model");

            if (Directory.Exists(directoryPath) is false)
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
            
            string runTimeConfigFilePath = Path.Combine(xmlDir, RealTimeControlXmlFiles.XmlRuntime);
            string dataConfigFilePath = Path.Combine(xmlDir, RealTimeControlXmlFiles.XmlData);
            string toolsConfigFilePath = Path.Combine(xmlDir, RealTimeControlXmlFiles.XmlTools);
            string timeSeriesFilePath = Path.Combine(xmlDir, RealTimeControlXmlFiles.XmlTimeSeries);

            var rtcModel = new RealTimeControlModel();

            logHandler.ReportInfo(Resources.RealTimeControlModelXmlReader_Please_note_that_Use_Restart_option_in_D_RTC_is_set_to_False);

            var runtimeConfigReader = new RealTimeControlRuntimeConfigXmlReader(logHandler);
            runtimeConfigReader.Read(runTimeConfigFilePath, rtcModel);

            var dataAndToolsConfigReader = new RealTimeControlDataAndToolsConfigXmlReader(logHandler);
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(dataConfigFilePath, toolsConfigFilePath, rtcModel.TimeStep);

            var timeSeriesReader = new RealTimeControlTimeSeriesXmlReader(logHandler);
            timeSeriesReader.Read(timeSeriesFilePath, controlGroups);

            rtcModel.ControlGroups.AddRange(controlGroups.Cast<ControlGroup>());

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

            if (File.Exists(settingsJsonPath) is false)
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_, 
                                             settingsJsonPath);
                return workDir; // no settings.json? return the workDir, as this will be the location of the XML files 
            }
            
            string settingsJsonFile = File.ReadAllText(settingsJsonPath);
            var deserializedFile = JsonConvert.DeserializeObject<RtcXmlDirectoryLookup>(settingsJsonFile);
            string xmlLocation = deserializedFile?.XmlDirectory;
            
            if (xmlLocation is null) // check if the settings.json contains a "xmlDir" key
            {
                logHandler.ReportError(Resources.RealTimeControlModelXmlReader_GetXmlDirectory_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory);
                return null;
            }
            
            string xmlDirPath = Path.Combine(workDir, xmlLocation);
            return xmlDirPath;
        }
    }
}