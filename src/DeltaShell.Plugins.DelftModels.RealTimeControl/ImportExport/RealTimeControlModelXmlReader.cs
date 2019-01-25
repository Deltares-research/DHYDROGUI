using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            var runTimeConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlRuntime);
            RealTimeControlRuntimeConfigXmlReader.Read(runTimeConfigFilePath, rtcModel);

            var dataConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlData);
            var toolsConfigFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTools);
            var controlGroups = RealTimeControlDataAndToolsConfigXmlReader.Read(dataConfigFilePath, toolsConfigFilePath);

            var timeSeriesFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlTimeSeries);
            RealTimeControlTimeSeriesXmlReader.Read(timeSeriesFilePath, controlGroups);

            var stateImportFilePath = Path.Combine(directoryPath, RealTimeControlXMLFiles.XmlImportState);

            var outputs = controlGroups.SelectMany(cg => cg.Outputs);
            RealTimeControlStateImportXmlReader.Read(stateImportFilePath, outputs.ToList());

            AddCreatedControlGroupsToRtcModel(controlGroups, rtcModel);

            return rtcModel;
        }

        private static void AddCreatedControlGroupsToRtcModel(IList<IControlGroup> controlGroups, RealTimeControlModel rtcModel)
        {
            controlGroups.ForEach(cg => rtcModel.ControlGroups.Add((ControlGroup) cg));
        }
    }
}
