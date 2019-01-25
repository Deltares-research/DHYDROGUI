using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlDataAndToolsConfigXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataAndToolsConfigXmlReader));

        public static IList<IControlGroup> Read(string dataConfigFilePath, string toolsConfigFilePath)
        {
            if (!File.Exists(dataConfigFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___does_not_exist_, dataConfigFilePath);
                return null;
            }

            if (!File.Exists(toolsConfigFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlToolsConfigXmlReader_Read_File___0___does_not_exist_, toolsConfigFilePath);
                return null;
            }

            var dataConfigObject = DelftConfigXmlFileParser.Read<RTCDataConfigXML>(dataConfigFilePath);
            var allElements = dataConfigObject.importSeries.timeSeries.Concat(dataConfigObject.exportSeries.timeSeries).ToList();

            var toolsConfigObject = DelftConfigXmlFileParser.Read<RtcToolsConfigXML>(toolsConfigFilePath);
            var ruleElements = toolsConfigObject.rules;
            var conditionElements = toolsConfigObject.triggers;

            var connectionPoints = RealTimeControlDataConfigXmlConverter.CreateConnectionPointsFromXmlElements(allElements).ToList();
            if (connectionPoints.Count == 0)
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___, dataConfigFilePath);
                return null;
            }

            var controlGroups = RealTimeControlToolsConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(ruleElements).ToList();
            if (controlGroups.Count == 0)
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, dataConfigFilePath);
                return null;
            }

            RealTimeControlToolsConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(ruleElements, controlGroups);
            RealTimeControlToolsConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(conditionElements, controlGroups);

            RealTimeControlToolsConfigComponentConnector.ConnectRules(ruleElements, controlGroups, connectionPoints);
            RealTimeControlToolsConfigComponentConnector.ConnectConditions(conditionElements, controlGroups, connectionPoints);

            RealTimeControlDataConfigXmlSetter.SetInterpolationAndExtraPolationRtcComponents(allElements, controlGroups);

            return controlGroups;
        }
    }
}
