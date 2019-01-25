using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlXmlReaderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlXmlReaderHelper));

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
                untaggedName = id.Replace(tag, string.Empty);
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

        public static IControlGroup GetControlGroupByElementId(this IList<IControlGroup> controlGroups, string id)
        {
            if (controlGroups == null) return null;

            var groupName = GetControlGroupNameFromElementId(id);
            var controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetControlGroupByElementId_Could_not_find_the_controlgroup___0___that_is_referenced_in_id___1____The_group_needs_to_be_referenced_in_file___2___, groupName, id, RealTimeControlXMLFiles.XmlTools);
                return null;
            }

            return controlGroup;
        }

        public static ConnectionPoint GetByName<T>(this IEnumerable<ConnectionPoint> connectionPoints, string name) where T : ConnectionPoint
        {
            if (connectionPoints == null) return null;

            var correspondingConnectionPoint = connectionPoints.OfType<T>().FirstOrDefault(o => o.Name == name);
            if (correspondingConnectionPoint == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetConnectionPointByName_Could_not_find_the_input_output___0____The_input_output_needs_to_be_referenced_in_file___1___, name, RealTimeControlXMLFiles.XmlData);

            return correspondingConnectionPoint;
        }

        public static RuleBase GetRuleByElementId<T>(this IControlGroup controlGroup, string id) where T : RuleBase
        {
            if (controlGroup == null) return null;

            var ruleName = GetRuleOrConditionNameFromElementId(id);

            var correspondingRule = controlGroup.Rules.OfType<T>().FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___, ruleName, id, RealTimeControlXMLFiles.XmlData);

            return correspondingRule;
        }

        public static RuleBase GetRuleByElementId(this IControlGroup controlGroup, string id)
        {
            if (controlGroup == null) return null;

            var ruleName = GetRuleOrConditionNameFromElementId(id);

            var correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___, ruleName, id, RealTimeControlXMLFiles.XmlData);

            return correspondingRule;
        }

        public static ConditionBase GetConditionByElementId<T>(this IControlGroup controlGroup, string id) where T : ConditionBase
        {
            if (controlGroup == null) return null;

            var conditionName = GetRuleOrConditionNameFromElementId(id);

            var condition = controlGroup.Conditions
                .Where(c => c.GetType() == typeof(T))
                .FirstOrDefault(r => r.Name == conditionName);

            if (condition == null)
                Log.WarnFormat(Resources.RealTimeControlXmlReaderHelper_GetConditionByElementIdInControlGroup_Could_not_find_the_condition___0____The_condition_needs_to_be_referenced_in_file___1___, conditionName, RealTimeControlXMLFiles.XmlData);

            return condition;
        }

        public static string GetTagFromElementId(string id)
        {
            var indexForSplit = id.IndexOf(']') + 1;
            var tag = id.Substring(0, indexForSplit);

            return tag;
        }
    }
}
