using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    public static class RealTimeControlTestHelper
    {
        public static RealTimeControlModel GenerateTestModel(bool allRules)
        {
            var result = new RealTimeControlModel("testModel");
            ControlGroup controlGroup = GenerateControlGroup();
            if (allRules)
            {
                controlGroup.Rules.Add(GenerateTimeRule());
                controlGroup.Rules.Add(GenerateHydraulicRule());
                controlGroup.Rules.Add(GenerateRelativeTimeRule());
            }

            result.ControlGroups.Add(controlGroup);
            return result;
        }

        public static ControlGroup CreateControlGroupWithTwoRulesOnOneOutput()
        {
            var controlGroup = new ControlGroup {Name = "control_group"};
            var output = new Output {Name = "output"};

            RelativeTimeRule rule1 = CreateRelativeTimeRule("rule1", output);
            var condition1 = new StandardCondition {Name = "condition1"};
            condition1.TrueOutputs.Add(rule1);
            var input1 = new Input();
            condition1.Input = input1;

            RelativeTimeRule rule2 = CreateRelativeTimeRule("rule2", output);
            var condition2 = new StandardCondition {Name = "condition2"};
            condition2.TrueOutputs.Add(rule2);
            var input2 = new Input();
            condition2.Input = input2;

            controlGroup.Outputs.Add(output);
            controlGroup.Rules.Add(rule1);
            controlGroup.Rules.Add(rule2);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);

            return controlGroup;
        }

        public static RelativeTimeRule CreateRelativeTimeRule(string name, Output output)
        {
            var rule1 = new RelativeTimeRule
            {
                Name = name,
                FromValue = false,
                Id = 6L,
                Interpolation = InterpolationType.Constant,
                MinimumPeriod = 3,
                LongName = "relative_time_rule_long_name"
            };

            rule1.Outputs.Add(output);
            rule1.Function[0d] = 1d;
            rule1.Function[3d] = 5d;
            rule1.Function[7d] = 11d;

            return rule1;
        }

        public static ControlGroup GenerateControlGroup()
        {
            return RealTimeControlModelHelper.CreateGroupPidRule(true);
        }

        public static Input GenerateInput()
        {
            return new Input(); // { Name = "ControlInput" };
        }

        public static Output GenerateOutput()
        {
            return new Output(); // { Name = "ControlOutput" };
        }

        public static PIDRule GeneratePidRule()
        {
            return new PIDRule("myFirstRule")
            {
                Kd = 1.2,
                Ki = 42.1,
                Setting = new Setting() {Max = 11.1}
            };
        }

        public static LookupSignal GenerateLookupSignal()
        {
            return new LookupSignal() {Name = "myFirstLookupSignal"};
        }

        public static TimeRule GenerateTimeRule()
        {
            return new TimeRule("myFirstTimeRule")
            {
                InterpolationOptionsTime = InterpolationType.None,
                Periodicity = ExtrapolationType.Periodic,
                TimeSeries = GenerateTimeSeries()
            };
        }

        public static HydraulicRule GenerateHydraulicRule()
        {
            return new HydraulicRule();
        }

        public static FactorRule GenerateFactorRule()
        {
            return new FactorRule
            {
                Name = "Factor",
                Factor = 1.6
            };
        }

        public static IntervalRule GenerateIntervalRule()
        {
            return new IntervalRule("myFirstIntervalRule")
            {
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable,
                FixedInterval = 0.123,
                DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge,
                Setting = new Setting()
                {
                    MaxSpeed = .123,
                    Below = 1.0,
                    Min = 2.0,
                    Max = 3.0,
                    Above = 4.0
                }
            };
        }

        public static RelativeTimeRule GenerateRelativeTimeRule()
        {
            return new RelativeTimeRule("myFirstRelativeTimeRule", false);
        }

        public static StandardCondition GenerateCondition()
        {
            return new StandardCondition {Name = "myFirstCondition"};
        }

        public static DirectionalCondition GenerateDirectionalCondition()
        {
            return new DirectionalCondition
            {
                Name = "myDirectionalCondition",
                LongName = "Test",
                Operation = Operation.LessEqual
            };
        }

        public static void AddDummyLinksToGroup(ControlGroup controlGroup)
        {
            foreach (Input input in controlGroup.Inputs)
            {
                // wip: quantityID is hardcoded to Water level, Crest level and Discharge
                input.ParameterName = "Water level";
                input.Feature = new RtcTestFeature {Name = "location"};
            }

            foreach (Output output in controlGroup.Outputs)
            {
                // wip: quantityID is hardcoded to Water level, Crest level and Discharge
                output.ParameterName = "Crest level";
                output.Feature = new RtcTestFeature {Name = "location"};
            }
        }

        private static bool CompareEqualityOfInput(Input left, Input right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.SetPoint != right.SetPoint)
            {
                return false;
            }

            if (left.IsConnected && right.IsConnected)
            {
                return left.ParameterName == right.ParameterName;
            }

            return true;
        }

        private static bool CompareEqualityOfOutput(Output left, Output right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.IntegralPart != right.IntegralPart)
            {
                return false;
            }

            if (left.IsConnected && right.IsConnected)
            {
                return left.ParameterName == right.ParameterName;
            }

            return true;
        }

        private static bool CompareEqualityOfRtcBaseObjects(RtcBaseObject left, RtcBaseObject right)
        {
            return left.Name == right.Name;
        }

        public static bool CompareEqualityOfConditions(ConditionBase left, ConditionBase right)
        {
            if (CompareEqualityOfRtcBaseObjects(left, right))
            {
                if (CompareEqualityOfInput((Input) left.Input, (Input) right.Input))
                {
                    if (!CompareConditionOutputs(left.FalseOutputs, right.FalseOutputs))
                    {
                        return false;
                    }

                    if (!CompareConditionOutputs(left.TrueOutputs, right.TrueOutputs))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool CompareEqualityOfStandardConditions(StandardCondition left, StandardCondition right)
        {
            if (CompareEqualityOfConditions(left, right))
            {
                if (Math.Abs(left.Value - right.Value) < double.Epsilon && left.Reference == right.Reference && left.Operation == right.Operation)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfRules(RuleBase left, RuleBase right)
        {
            if (CompareEqualityOfRtcBaseObjects(left, right))
            {
                if (left.Inputs.Count != right.Inputs.Count)
                {
                    return false;
                }

                for (var i = 0; i < left.Inputs.Count; i++)
                {
                    if (!CompareEqualityOfInput((Input) left.Inputs[i], (Input) right.Inputs[i]))
                    {
                        return false;
                    }
                }

                if (left.Outputs.Count != right.Outputs.Count)
                {
                    return false;
                }

                for (var i = 0; i < left.Outputs.Count; i++)
                {
                    if (!CompareEqualityOfOutput(left.Outputs[i], right.Outputs[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool CompareEqualityOfPIDRules(PIDRule left, PIDRule right)
        {
            if (CompareEqualityOfRules(left, right))
            {
                if (left.Setting == right.Setting && Math.Abs(left.Kp - right.Kp) < double.Epsilon && Math.Abs(left.Ki - right.Ki) < double.Epsilon &&
                    Math.Abs(left.Kd - right.Kd) < double.Epsilon) //&& (left.IsAConstant == right.IsAConstant))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfTimeRules(TimeRule left, TimeRule right)
        {
            if (CompareEqualityOfRules(left, right))
            {
                if (left.Periodicity == right.Periodicity && left.InterpolationOptionsTime == right.InterpolationOptionsTime
                                                          && CompareEqualityOfTimeSeries(left.TimeSeries, right.TimeSeries))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfHydraulicRules(HydraulicRule left, HydraulicRule right)
        {
            if (CompareEqualityOfRules(left, right))
            {
                if (left.Interpolation == right.Interpolation && CompareEqualityOfFunctions(left.Function, right.Function))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfFactorRules(FactorRule left, FactorRule right)
        {
            if (CompareEqualityOfHydraulicRules(left, right))
            {
                if (Math.Abs(left.Factor - right.Factor) < double.Epsilon)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfIntervalRules(IntervalRule left, IntervalRule right)
        {
            if (CompareEqualityOfRules(left, right))
            {
                if (Math.Abs(left.DeadbandAroundSetpoint - right.DeadbandAroundSetpoint) < double.Epsilon
                    && left.InterpolationOptionsTime == right.InterpolationOptionsTime
                    && left.Setting == right.Setting
                    && left.IntervalType == right.IntervalType
                    && Math.Abs(left.FixedInterval - right.FixedInterval) < double.Epsilon
                    && left.DeadBandType == right.DeadBandType
                    && Math.Abs(left.Setting.MaxSpeed - right.Setting.MaxSpeed) < double.Epsilon
                    && Math.Abs(left.Setting.Min - right.Setting.Min) < double.Epsilon
                    && Math.Abs(left.Setting.Max - right.Setting.Max) < double.Epsilon
                    && Math.Abs(left.Setting.Below - right.Setting.Below) < double.Epsilon
                    && Math.Abs(left.Setting.Above - right.Setting.Above) < double.Epsilon
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfRelativeTimeRules(RelativeTimeRule left, RelativeTimeRule right)
        {
            if (CompareEqualityOfRules(left, right))
            {
                if (left.FromValue == right.FromValue && left.Interpolation == right.Interpolation &&
                    CompareEqualityOfFunctions(left.Function, right.Function))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareEqualityOfControlGroups(ControlGroup left, ControlGroup right)
        {
            for (var i = 0; i < left.Inputs.Count; i++)
            {
                if (!CompareEqualityOfInput(left.Inputs[i], right.Inputs[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < left.Outputs.Count; i++)
            {
                if (!CompareEqualityOfOutput(left.Outputs[i], right.Outputs[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < left.Rules.Count; i++)
            {
                if (!CompareEqualityOfRules(left.Rules[i], right.Rules[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < left.Conditions.Count; i++)
            {
                if (!CompareEqualityOfConditions(left.Conditions[i], right.Conditions[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < left.Signals.Count; i++)
            {
                if (!CompareEqualityOfLookupSignals(left.Signals[i], right.Signals[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareEqualityOfLookupSignals(SignalBase left, SignalBase right)
        {
            if (left.Name.Equals(right.Name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a control group with the following layout.
        /// input                  condition              rule                  output
        /// trueRuleInput ------------------------------  Rule if true ------  outputIfFalse
        /// /
        /// true
        /// /
        /// conditionInput -------- BooleanCondition
        /// \
        /// false
        /// \
        /// falseRuleInput ------------------------------ Rule if false ------- outputIfFalse
        /// </summary>
        /// <returns></returns>
        public static ControlGroup CreateGroup2Rules()
        {
            var controlGroup = new ControlGroup {Name = "Control group"};

            var trueRuleInput = new Input();
            var conditionInput = new Input();
            var falseRuleInput = new Input();
            controlGroup.Inputs.Add(trueRuleInput);
            controlGroup.Inputs.Add(conditionInput);
            controlGroup.Inputs.Add(falseRuleInput);

            var trueRule = new PIDRule {Name = "Rule if true"};
            trueRule.Inputs.Add(trueRuleInput);
            var falseRule = new PIDRule {Name = "Rule if false"};
            falseRule.Inputs.Add(falseRuleInput);
            controlGroup.Rules.Add(trueRule);
            controlGroup.Rules.Add(falseRule);

            var condition = new StandardCondition {Input = conditionInput};
            condition.FalseOutputs.Add(falseRule);
            condition.TrueOutputs.Add(trueRule);
            controlGroup.Conditions.Add(condition);
            var outputIfFalse = new Output();
            var outputIfTrue = new Output();
            falseRule.Outputs.Add(outputIfFalse);
            trueRule.Outputs.Add(outputIfTrue);
            controlGroup.Outputs.Add(outputIfTrue);
            controlGroup.Outputs.Add(outputIfFalse);

            return controlGroup;
        }

        private static TimeSeries GenerateTimeSeries()
        {
            var timeSeries = new TimeSeries();
            var t = new DateTime(2000, 3, 3);
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("flow", new Unit("m3/s", "m3/s")));
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            timeSeries[t] = 0.0;
            timeSeries[t.AddMinutes(5)] = 20.0;
            timeSeries.Name = "someTime";
            return timeSeries;
        }

        private static bool CompareConditionOutputs(IList<RtcBaseObject> left, IList<RtcBaseObject> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            if (left.Count != 0)
            {
                for (var i = 0; i < left.Count; i++)
                {
                    if (left[i].GetType() != right[i].GetType())
                    {
                        return false;
                    }

                    if (left[i] is RuleBase)
                    {
                        if (!CompareEqualityOfRules((RuleBase) left[i], (RuleBase) right[i]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!CompareEqualityOfConditions((ConditionBase) left[i], (ConditionBase) right[i]))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool CompareEqualityOfFunctions(Function left, Function right)
        {
            if (left.Name != right.Name)
            {
                return false;
            }

            return true;
        }

        private static bool CompareEqualityOfTimeSeries(TimeSeries left, TimeSeries right)
        {
            if (left.Name == right.Name)
            {
                if (left.Components[0].ValueType == right.Components[0].ValueType)
                {
                    if (left.Components[0].Values.Count == right.Components[0].Values.Count)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}