using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlToolsConfigXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlToolsConfigXmlReader));

        public static void Read(string toolsConfigFilePath, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (!File.Exists(toolsConfigFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlToolsConfigXmlReader_Read_File___0___does_not_exist_, toolsConfigFilePath);
                return;
            }
                
            if (controlGroups == null || connectionPoints == null) return;

            var toolsConfigObject = (RtcToolsConfigXML)DelftConfigXmlFileParser.Read(toolsConfigFilePath);

            var ruleElements = toolsConfigObject.rules;
            var conditionElements = toolsConfigObject.triggers;
    
            var relativeTimeRuleElements = ruleElements.Where(r => r.Item is TimeRelativeXML).Select(r => r.Item as TimeRelativeXML).ToList();
            var timeRuleElements = ruleElements.Where(r => r.Item is TimeAbsoluteXML).Select(r => r.Item as TimeAbsoluteXML).ToList();
            var standardConditionElements = conditionElements.Where(t => t.Item is StandardTriggerXML).Select(r => r.Item as StandardTriggerXML).ToList();

            RealTimeControlToolsConfigComponentConnector.ConnectTimeRules(timeRuleElements, controlGroups, connectionPoints);
            RealTimeControlToolsConfigComponentConnector.ConnectRelativeTimeRules(relativeTimeRuleElements,controlGroups,connectionPoints);
            RealTimeControlToolsConfigComponentConnector.ConnectStandardConditions(standardConditionElements, controlGroups, connectionPoints);
        }
    }
}
