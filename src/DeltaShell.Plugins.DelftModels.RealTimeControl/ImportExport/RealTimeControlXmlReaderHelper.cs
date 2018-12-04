using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Linq;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static public class RealTimeControlXmlReaderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlConverter));

        public static string RemoveTagFromElementName(string taggedName, string tag)
        {
            return taggedName.Replace(tag, string.Empty);
        }

        public static string GetRuleOrConditionNameFromElementId(string id)
        {
            var name = id.Split('/').LastOrDefault();
            return name;
        }

        public static string GetControlGroupNameFromElementId(string id, string tag = null)
        {
            if (tag == RtcXmlTag.Input || tag == RtcXmlTag.Output) return null;

            var untaggedName = tag != null && !id.Contains(RtcXmlTag.Status)
                ? RemoveTagFromElementName(id,tag)
                : id.Split(']').LastOrDefault();

            var groupName = untaggedName?.Split('/').FirstOrDefault();

            return groupName;
        }

        public static ControlGroup GetControlGroupByElementId(string id, IList<ControlGroup> controlGroups)
        {
            var groupName = GetControlGroupNameFromElementId(id);
            var controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                Log.Warn($"Could not find the controlgroup '{groupName}' that is referenced in id '{id}'. The group needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}'.");
                return null;
            }
            return controlGroup;
        }

        public static ConnectionPoint GetConnectionPointByName(string name, IList<ConnectionPoint> connectionPoints)
        {
            var correspondingConnectionPoint = connectionPoints.FirstOrDefault(o => o.Name == name);
            if (correspondingConnectionPoint == null)
            {
                Log.Warn($"Could not find the input/output '{name}'. The input/output needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}'.");
            }

            return correspondingConnectionPoint;
        }

        public static RuleBase GetRuleByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            var ruleName = GetRuleOrConditionNameFromElementId(id);

            var correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
            {
                Log.Warn($"Could not find the input/output '{ruleName}' that is referenced in id '{id}'. The input/output needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}'.");
                return null;
            }

            return correspondingRule;
        }

        public static ConditionBase GetConditionByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            var conditionName = GetRuleOrConditionNameFromElementId(id);

            var correspondingCondition = controlGroup.Conditions.FirstOrDefault(r => r.Name == conditionName);
            if (correspondingCondition == null)
            {
                Log.Warn($"Could not find the condition '{conditionName}'. The condition needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}'.");
                return null;
            }

            return correspondingCondition;
        }
    }
}
