using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>Responsible for reading the XML files for RTC and building a RealTimeControlModel</summary>
    public sealed class RealTimeControlModelXmlReader : IRealTimeControlXmlReader
    {
        /// <inheritdoc/>
        public void ReadFromXml(RealTimeControlModel model, string directory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrEmpty(directory, nameof(directory));
            
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format(Resources.RealTimeControlModelXmlReader_ReadFromXml_Directory___0___does_not_exist, directory));
            }

            string runTimeConfigFilePath = Path.Combine(directory, RealTimeControlXmlFiles.XmlRuntime);
            string dataConfigFilePath = Path.Combine(directory, RealTimeControlXmlFiles.XmlData);
            string toolsConfigFilePath = Path.Combine(directory, RealTimeControlXmlFiles.XmlTools);
            string timeSeriesFilePath = Path.Combine(directory, RealTimeControlXmlFiles.XmlTimeSeries);

            var logHandler = new LogHandler(Resources.RealTimeControlModelXmlReader_ReadFromXml_Import_of_the_Real_Time_Control_Model);

            logHandler.ReportInfo(Resources.RealTimeControlModelXmlReader_Please_note_that_Use_Restart_option_in_D_RTC_is_set_to_False);

            var runtimeConfigReader = new RealTimeControlRuntimeConfigXmlReader(logHandler);
            runtimeConfigReader.Read(runTimeConfigFilePath, model);

            var dataAndToolsConfigReader = new RealTimeControlDataAndToolsConfigXmlReader(logHandler);
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(dataConfigFilePath, toolsConfigFilePath, model.TimeStep);

            var timeSeriesReader = new RealTimeControlTimeSeriesXmlReader(logHandler);
            timeSeriesReader.Read(timeSeriesFilePath, controlGroups);

            model.ControlGroups.AddRange(controlGroups.Cast<ControlGroup>());

            ThrowOnReportedError(logHandler);
        }

        private static void ThrowOnReportedError(ILogHandler logHandler)
        {
            bool hasErrors = logHandler.LogMessages.ErrorMessages.Any();
            logHandler.LogReport();

            if (hasErrors)
            {
                throw new InvalidOperationException(Resources.RealTimeControlModelXmlReader_ThrowOnReportedError_Reading_the_Real_Time_Control_Model_XML_files_failed_);
            }
        }
    }
}