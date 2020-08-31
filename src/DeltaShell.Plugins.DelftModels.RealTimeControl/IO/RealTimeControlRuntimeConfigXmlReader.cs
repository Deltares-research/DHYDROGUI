using System.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for reading the runtime configuration file path and setting the data on the RealTimeControl Model.
    /// </summary>
    public class RealTimeControlRuntimeConfigXmlReader
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlRuntimeConfigXmlReader(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Reads the specified runtime configuration file path and sets the data on the RealTimeControl Model.
        /// </summary>
        /// <param name="runtimeConfigFilePath">The runtime configuration file path.</param>
        /// <param name="rtcModel">The RealTimeControl Model.</param>
        /// <remarks>
        /// If parameter rtcModel is NULL, the method returns.
        /// If the runtimeConfigFilePath does not exist, a message is logged and methods returns.
        /// </remarks>
        public void Read(string runtimeConfigFilePath, RealTimeControlModel rtcModel)
        {
            if (rtcModel == null)
            {
                return;
            }

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);

            RtcRuntimeConfigXML runtimeConfigObject;
            try
            {
                runtimeConfigObject = delftConfigXmlParser.Read<RtcRuntimeConfigXML>(runtimeConfigFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return;
            }

            var runTimeSettings = runtimeConfigObject.period.Item as UserDefinedRuntimeXML;
            UserDefinedStateExportXML restartSettings = runtimeConfigObject.stateFiles;
            var simulationModeSettings = runtimeConfigObject.Item as ModeXML;

            var runtimeConfigSetter = new RealTimeControlRuntimeConfigSetter(logHandler);
            runtimeConfigSetter.SetRunTimeSettings(rtcModel, runTimeSettings);
            runtimeConfigSetter.SetRestartSettings(rtcModel, restartSettings);
            runtimeConfigSetter.SetSimulationModeSettings(rtcModel, simulationModeSettings);
        }
    }
}