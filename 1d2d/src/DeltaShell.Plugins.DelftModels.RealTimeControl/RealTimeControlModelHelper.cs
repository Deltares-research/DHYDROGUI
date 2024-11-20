using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public static class RealTimeControlModelHelper
    {
        private const string EmptyGroup = "Empty group";
        private const string PidRuleGroup = "PID Rule with condition";
        private const string HydraulicRuleGroup = "Lookup table Rule with condition";
        private const string IntervalRuleGroup = "Interval Rule with condition";
        private const string TimeRuleGroup = "Time Rule with condition";
        private const string RelativeTimeRuleGroup = "Relative from time/value rule with condition";
        private const string InvertorRuleGroup = "InvertorRule";

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> StandardControlGroups
        {
            get
            {
                yield return EmptyGroup;
                yield return PidRuleGroup;
                yield return HydraulicRuleGroup;
                yield return IntervalRuleGroup;
                yield return TimeRuleGroup;
                yield return RelativeTimeRuleGroup;
                yield return InvertorRuleGroup;
            }
        }

        public static ControlGroup CreateStandardControlGroup(string group)
        {
            switch (group)
            {
                case EmptyGroup:
                    return new ControlGroup();
                case PidRuleGroup:
                    return CreateGroupPidRule(true);
                case HydraulicRuleGroup:
                    return CreateGroupHydraulicRule(true);
                case IntervalRuleGroup:
                    return CreateGroupIntervalRule();
                case TimeRuleGroup:
                    return CreateGroupTimeRuleWithCondition();
                case RelativeTimeRuleGroup:
                    return CreateGroupRelativeTimeRule();
                case InvertorRuleGroup:
                    return CreateGroupInvertorRule();
            }

            return null;
        }

        public static ControlGroup CreateGroupHydraulicRule(bool addCondition)
        {
            return addCondition
                       ? CreateGroupRuleWithOneInputOneConditionInput(new HydraulicRule())
                       : CreateGroupRuleWithoutCondition(new HydraulicRule());
        }

        public static ControlGroup CreateGroupInvertorRule()
        {
            return CreateGroupRuleWithoutCondition(new FactorRule {Factor = -1.0});
        }

        public static ControlGroup CreateGroupIntervalRule()
        {
            return CreateGroupRuleWithOneInputOneConditionInput(new IntervalRule());
        }

        public static ControlGroup CreateGroupPidRule(bool addCondition)
        {
            return addCondition
                       ? CreateGroupRuleWithOneInputOneConditionInput(new PIDRule())
                       : CreateGroupRuleWithoutCondition(new PIDRule());
        }

        public static ControlGroup CreateGroupRuleWithOneInputOneConditionInput(RuleBase ruleBase, ConditionBase conditionBase)
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            var ruleInput = new Input();
            var ruleOutput = new Output();
            controlGroup.Inputs.Add(ruleInput);
            controlGroup.Outputs.Add(ruleOutput);

            controlGroup.Rules.Add(ruleBase);
            ruleBase.Inputs.Add(ruleInput);
            ruleBase.Outputs.Add(ruleOutput);

            if (!(conditionBase is TimeCondition))
            {
                var conditionInput = new Input();
                controlGroup.Inputs.Add(conditionInput);
                conditionBase.Input = conditionInput;
            }

            conditionBase.TrueOutputs.Add(ruleBase);
            controlGroup.Conditions.Add(conditionBase);

            return controlGroup;
        }

        public static ControlGroup CreateGroupTimeRuleWithCondition()
        {
            return CreateGroupRuleWithoutInput(new TimeRule());
        }

        public static ControlGroup CreateGroupRelativeTimeRule()
        {
            return CreateGroupRuleWithoutInput(new RelativeTimeRule {FromValue = false});
        }

        public static string GetUniqueName(string filter, IEnumerable items, string prefix)
        {
            if (null != filter)
            {
                if (filter.Length == 0)
                {
                    // to do test if filter has format code
                    throw new ArgumentException("Can not create an unique name when filter is empty.");
                }
            }
            else
            {
                filter = prefix + "{0}";
            }

            var names = new Dictionary<string, int>();

            foreach (INameable o in items.OfType<INameable>())
            {
                if (o.Name == null)
                {
                    o.Name = string.Empty;
                }

                names[o.Name] = 0;
            }

            string unique;
            var id = 1;

            do
            {
                unique = string.Format(filter, id++);
            } while (names.ContainsKey(unique));

            return unique;
        }

        private static ControlGroup CreateGroupRuleWithOneInputOneConditionInput(RuleBase ruleBase)
        {
            return CreateGroupRuleWithOneInputOneConditionInput(ruleBase,
                                                                new StandardCondition());
        }

        private static ControlGroup CreateGroupRuleWithoutInput(RuleBase ruleBase)
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            var conditionInput = new Input();
            controlGroup.Inputs.Add(conditionInput);
            var condition = new StandardCondition {Input = conditionInput};
            condition.Input = conditionInput;
            controlGroup.Conditions.Add(condition);
            controlGroup.Rules.Add(ruleBase);
            var ruleOutput = new Output();
            ruleBase.Outputs.Add(ruleOutput);
            condition.TrueOutputs.Add(ruleBase);
            controlGroup.Outputs.Add(ruleOutput);
            return controlGroup;
        }

        private static ControlGroup CreateGroupRuleWithoutCondition(RuleBase ruleBase)
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            var ruleInput = new Input();
            var ruleOutput = new Output();

            controlGroup.Inputs.Add(ruleInput);
            controlGroup.Outputs.Add(ruleOutput);

            controlGroup.Rules.Add(ruleBase);
            ruleBase.Inputs.Add(ruleInput);
            ruleBase.Outputs.Add(ruleOutput);

            return controlGroup;
        }
    }
}