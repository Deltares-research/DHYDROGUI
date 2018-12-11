using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static public class RealTimeControlXmlReaderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlXmlReaderHelper));

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

            string untaggedName;
            if (tag != null && !id.Contains(RtcXmlTag.Status))
            {
                untaggedName = RemoveTagFromElementName(id, tag);
            }
            else
            {
                var splitId = id.Split(']');
                untaggedName = splitId.LastOrDefault();
            }
            
            var splitName = untaggedName?.Split('/');
            var groupName = splitName?.FirstOrDefault();

            return groupName;
        }

        public static ControlGroup GetControlGroupByElementId(string id, IList<ControlGroup> controlGroups)
        {
            if (controlGroups == null) return null;

            var groupName = GetControlGroupNameFromElementId(id);
            var controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetControlGroupByElementId_Could_not_find_the_controlgroup___0___that_is_referenced_in_id___1____The_group_needs_to_be_referenced_in_file___2___, groupName, id, RealTimeControlXMLFiles.XmlData);
                return null;
            }
            return controlGroup;
        }

        public static ConnectionPoint GetConnectionPointByName(string name, IList<ConnectionPoint> connectionPoints)
        {
            if (connectionPoints == null) return null;

            var correspondingConnectionPoint = connectionPoints.FirstOrDefault(o => o.Name == name);
            if (correspondingConnectionPoint == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetConnectionPointByName_Could_not_find_the_input_output___0____The_input_output_needs_to_be_referenced_in_file___1___, name, RealTimeControlXMLFiles.XmlData);

            return correspondingConnectionPoint;
        }

        public static RuleBase GetRuleByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            if (controlGroup == null) return null;

            var ruleName = GetRuleOrConditionNameFromElementId(id);

            var correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_input_output___0___that_is_referenced_in_id___1____The_input_output_needs_to_be_referenced_in_file___2___, ruleName, id, RealTimeControlXMLFiles.XmlData);

            return correspondingRule;
        }

        public static ConditionBase GetConditionByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            if (controlGroup == null) return null;

            var conditionName = GetRuleOrConditionNameFromElementId(id);

            var correspondingCondition = controlGroup.Conditions.FirstOrDefault(r => r.Name == conditionName);
            if (correspondingCondition == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetConditionByElementIdInControlGroup_Could_not_find_the_condition___0____The_condition_needs_to_be_referenced_in_file___1___, conditionName, RealTimeControlXMLFiles.XmlData);

            return correspondingCondition;
        }
    }
}
