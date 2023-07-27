using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Provides helper methods to assist in reading the RTC XML files.
    /// </summary>
    public static class RealTimeControlXmlReaderHelper
    {
        /// <summary>
        /// Gets the rule, condition or signal name from the xml element id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The rule, condition or signal name.</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static string GetComponentNameFromElementId(string id)
        {
            List<string> tags = GetAllTagsFromId(id).ToList();

            if (tags.Any(t => RtcXmlTag.ConnectionPointTags.Contains(t)))
            {
                return null;
            }

            tags.ForEach(t => id = id.Replace(t, string.Empty));

            string name = id.Split('/').LastOrDefault();

            return name;
        }

        /// <summary>
        /// Gets the control group name from the xml element id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The control group name</returns>
        /// <remarks>id is expected to not be <c>null</c>.</remarks>
        public static string GetControlGroupNameFromElementId(string id)
        {
            string controlGroupName = id.Split('/').FirstOrDefault();

            string tag = GetTag(controlGroupName);
            if (tag != null)
            {
                controlGroupName = controlGroupName.Substring(tag.Length);
            }

            return controlGroupName;
        }

        /// <summary>
        /// Gets the control group name from the xml element id, provided that it does not belong to a connection point.
        /// </summary>
        /// <param name="controlGroups">The control groups.</param>
        /// <param name="id">The id.</param>
        /// <param name="logHandler">The log handler to which log messages can be added.</param>
        /// <returns>The corresponding control group.</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static IControlGroup GetControlGroupByElementId(this IEnumerable<IControlGroup> controlGroups, string id, ILogHandler logHandler)
        {
            if (controlGroups == null)
            {
                return null;
            }

            string groupName = GetControlGroupNameFromElementId(id);
            IControlGroup controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                logHandler?.ReportWarningFormat(Resources.RealTimeControlXmlReaderHelper_GetControlGroupByElementId_Could_not_find_the_controlgroup___0___that_is_referenced_in_id___1____The_group_needs_to_be_referenced_in_file___2___, groupName, id, RealTimeControlXmlFiles.XmlTools);
                return null;
            }

            return controlGroup;
        }

        /// <summary>
        /// Gets the connection point by name and type in a collection from connection points.
        /// </summary>
        /// <typeparam name="T">The type of connection point</typeparam>
        /// <param name="connectionPoints">The connection points.</param>
        /// <param name="name">The name of the connection point.</param>
        /// <param name="logHandler">The log handler to which log messages can be added.</param>
        /// <returns>The corresponding connection point.</returns>
        /// <remarks>Parameter name is expected to not be <c>null</c>.</remarks>
        public static T GetByName<T>(this IEnumerable<ConnectionPoint> connectionPoints, string name, ILogHandler logHandler) where T : ConnectionPoint
        {
            if (connectionPoints == null)
            {
                return null;
            }

            T correspondingConnectionPoint = connectionPoints.OfType<T>().FirstOrDefault(o => o.Name == name);
            if (correspondingConnectionPoint == null)
            {
                logHandler?.ReportWarningFormat(Resources.RealTimeControlXmlReaderHelper_GetConnectionPointByName_Could_not_find_the_input_output___0____The_input_output_needs_to_be_referenced_in_file___1___, name, RealTimeControlXmlFiles.XmlData);
            }

            return correspondingConnectionPoint;
        }

        /// <summary>
        /// Gets the corresponding rule by element id and type in a control group.
        /// </summary>
        /// <typeparam name="T">The type of rule</typeparam>
        /// <param name="controlGroup">The controlgroup.</param>
        /// <param name="id">The id of the rule element.</param>
        /// <param name="logHandler">The log handler to which log messages can be added. </param>
        /// <returns>The corresponding rule.</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static RuleBase GetRuleByElementId<T>(this IControlGroup controlGroup, string id, ILogHandler logHandler) where T : RuleBase
        {
            if (controlGroup == null)
            {
                return null;
            }

            string ruleName = GetComponentNameFromElementId(id);

            T correspondingRule = controlGroup.Rules.OfType<T>().FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
            {
                logHandler?.ReportWarningFormat(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___, ruleName, id, RealTimeControlXmlFiles.XmlData);
            }

            return correspondingRule;
        }

        /// <summary>
        /// Gets the corresponding rule by element id in a control group.
        /// </summary>
        /// <param name="controlGroup">The controlgroup.</param>
        /// <param name="id">The id of the rule element.</param>
        /// <param name="logHandler">The log handler to which log messages can be added.</param>
        /// <returns>The corresponding rule.</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static RuleBase GetRuleByElementId(this IControlGroup controlGroup, string id, ILogHandler logHandler)
        {
            if (controlGroup == null)
            {
                return null;
            }

            string ruleName = GetComponentNameFromElementId(id);

            RuleBase correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
            {
                logHandler?.ReportWarningFormat(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___, ruleName, id, RealTimeControlXmlFiles.XmlData);
            }

            return correspondingRule;
        }

        /// <summary>
        /// Gets the corresponding signal by element id and type in a control group.
        /// </summary>
        /// <typeparam name="T"> The type of signal. </typeparam>
        /// <param name="controlGroup"> The control group from which the signal is retrieved. </param>
        /// <param name="id"> The id of the read signal element. </param>
        /// <param name="logHandler"> The log handler.</param>
        /// <returns> The corresponding signal. </returns>
        public static SignalBase GetSignalByElementId<T>(this IControlGroup controlGroup, string id,
                                                         ILogHandler logHandler) where T : SignalBase
        {
            if (controlGroup == null)
            {
                return null;
            }

            string signalName = GetComponentNameFromElementId(id);

            T correspondingSignal = controlGroup.Signals.OfType<T>().FirstOrDefault(r => r.Name == signalName);
            if (correspondingSignal == null)
            {
                logHandler?.ReportWarningFormat(
                    Resources
                        .RealTimeControlXmlReaderHelper_GetSignalByElementIdInControlGroup_Could_not_find_the_signal___0___that_is_referenced_in_id___1___The_signal_needs_to_be_referenced_in_file___2___,
                    signalName, id, RealTimeControlXmlFiles.XmlData);
            }

            return correspondingSignal;
        }

        /// <summary>
        /// Gets the corresponding rule by element id and type in a control group.
        /// </summary>
        /// <typeparam name="T">The type of rule</typeparam>
        /// <param name="controlGroup">The controlgroup.</param>
        /// <param name="id">The id of the rule element.</param>
        /// <param name="logHandler">The log handler to which log messages can be added/</param>
        /// <returns>The corresponding rule.</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static T GetConditionByElementId<T>(this IControlGroup controlGroup, string id, ILogHandler logHandler) where T : ConditionBase
        {
            if (controlGroup == null)
            {
                return null;
            }

            string conditionName = GetComponentNameFromElementId(id);

            ConditionBase condition = controlGroup.Conditions
                                                  .Where(c => c.GetType() == typeof(T))
                                                  .FirstOrDefault(r => r.Name == conditionName);

            if (condition == null)
            {
                logHandler?.ReportWarningFormat(Resources.RealTimeControlXmlReaderHelper_GetConditionByElementIdInControlGroup_Could_not_find_the_condition___0____The_condition_needs_to_be_referenced_in_file___1___, conditionName, RealTimeControlXmlFiles.XmlData);
            }

            return (T) condition;
        }

        /// <summary>
        /// Gets the tag of interest (connection points, rules, conditions) from the element identifier.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The tag</returns>
        /// <remarks>Parameter id is expected to not be <c>null</c>.</remarks>
        public static string GetTagFromElementId(string id)
        {
            IEnumerable<string> tags = GetAllTagsFromId(id);

            string tag = tags.FirstOrDefault(t =>
                                                 RtcXmlTag.ComponentTags.Contains(t)
                                                 || RtcXmlTag.ConnectionPointTags.Contains(t));

            return tag;
        }

        private static string GetTag(string str)
        {
            var regex = new Regex(@"^\[.*?\]");
            return regex.IsMatch(str)
                       ? regex.Match(str).Value
                       : null;
        }

        private static IEnumerable<string> GetAllTagsFromId(string id)
        {
            var regex = new Regex(@"\[.*?\]");
            MatchCollection matches = regex.Matches(id);

            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
    }
}