using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
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

        public static StandardCondition GenerateCondition(IControlGroup controlGroup)
        {
            return new StandardCondition {Name = "myFirstCondition"};
        }

        public static DirectionalCondition GenerateDirectionalCondition(ControlGroup controlGroup)
        {
            return new DirectionalCondition
            {
                Name = "myDirectionalCondition",
                LongName = "Test",
                Operation = Operation.LessEqual
            };
        }

        public static void AddDummyLinksToGroup(ControlledTestModel controlledTestModel, ControlGroup controlGroup)
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

        public static bool CompareEqualityOfInput(Input left, Input right)
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

        public static bool CompareEqualityOfOutput(Output left, Output right)
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

        public static bool CompareEqualityOfRtcBaseObjects(RtcBaseObject left, RtcBaseObject right)
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

        public static bool CompareArray(double[] left, double[] right, double tolerance)
        {
            for (var i = 0; i < left.Length; i++)
            {
                if (!(Math.Abs(left[i] - right[i]) <= tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public static ControlGroup CreateControlGroupWithLookupSignalAndPIDRule()
        {
            var input = new Input();
            var output = new Output();
            var pidRule = new PIDRule()
            {
                Name = "pid rule",
                Inputs = {input},
                Outputs = {output},
                Kd = 0.0,
                Ki = 0.2,
                Kp = 0.5,
                Setting = new Setting
                {
                    Max = 123.6,
                    MaxSpeed = 0.2,
                    Min = 116.0
                }
            };

            var lookupSignal = new LookupSignal
            {
                Name = "lookup signal",
                Inputs = {input},
                RuleBases = {pidRule},
                Function = new Function
                {
                    Arguments = {new Variable<double>("x")},
                    Components = {new Variable<double>("y")}
                }
            };
            lookupSignal.Function[1.1] = new[]
            {
                2.3
            };

            var controlGroup = new ControlGroup
            {
                Name = "Control Group",
                Rules = {pidRule},
                Signals = {lookupSignal},
                Inputs = {input},
                Outputs = {output}
            };

            return controlGroup;
        }

        public static ControlGroup CreateGroupRuleWithoutConditionWithoutInput(RuleBase ruleBase)
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            var ruleOutput = new Output();

            controlGroup.Outputs.Add(ruleOutput);

            controlGroup.Rules.Add(ruleBase);
            ruleBase.Outputs.Add(ruleOutput);

            return controlGroup;
        }

        public static ControlGroup CreateControlGroupWithTimeRule(string groupName, ControlledTestModel controlledModel, RealTimeControlModel realTimeControlModel, int inputFeatureIndex = 0)
        {
            ControlGroup controlGroup = CreateGroupTimeRuleWithoutCondition();
            controlGroup.Name = groupName;
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity
            {
                Activities =
                {
                    realTimeControlModel,
                    controlledModel
                }
            };

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[inputFeatureIndex]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            ((TimeRule) controlGroup.Rules[0]).InterpolationOptionsTime = InterpolationType.Constant;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 0, 0, 0)] = 11.0;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 1, 0, 0)] = 12.0;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 2, 0, 0)] = 13.0;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 3, 0, 0)] = 14.0;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 4, 0, 0)] = 15.0;
            ((TimeRule) controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 5, 0, 0)] = 16.0;

            return controlGroup;
        }

        public static void SetupControlledTestModel(out ControlledTestModel controlledModel, out RealTimeControlModel realTimeControlModel)
        {
            realTimeControlModel = new RealTimeControlModel();

            controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                InputFeatures =
                {
                    new RtcTestFeature {Name = "input_feature1"},
                    new RtcTestFeature {Name = "input_feature2"}
                },
                OutputFeatures =
                {
                    new RtcTestFeature {Name = "output_feature1"},
                    new RtcTestFeature {Name = "output_feature2"}
                }
            };
        }

        public static ControlGroup SetupHydraulicRuleControlGroup(ControlledTestModel controlledModel, RealTimeControlModel realTimeControlModel, bool addCondition, int inputFeatureIndex = 0)
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(addCondition);

            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity
            {
                Activities =
                {
                    realTimeControlModel,
                    controlledModel
                }
            };

            IDataItem outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            if (addCondition)
            {
                IDataItem outputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
                outputDataItem2.Value = 2.0;
                realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[1]).LinkTo(outputDataItem2);

                ((StandardCondition) controlGroup.Conditions[0]).Operation = Operation.Greater;
                controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active
            }

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[inputFeatureIndex]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            ((HydraulicRule) controlGroup.Rules[0]).Function[-1000.0] = 33.0;
            ((HydraulicRule) controlGroup.Rules[0]).Function[+1000.0] = 33.0;

            return controlGroup;
        }

        public static ControlledTestModel SetupTwoIdenticalHydraulicRuleControlGroups(out RealTimeControlModel realTimeControlModel, out ControlGroup controlGroup2, out ControlGroup controlGroup1)
        {
            ControlledTestModel controlledModel;
            SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            controlGroup1 = SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);
            controlGroup2 = SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true, 1);
            controlGroup2.Name = "group2";

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            intputDataItem1.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup1.Conditions[0].Input).LinkTo(intputDataItem1);

            IDataItem intputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[2]).First();
            intputDataItem2.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup2.Conditions[0].Input).LinkTo(intputDataItem2);

            // Modify lookuptable to check results are different per rule
            ((HydraulicRule) controlGroup2.Rules[0]).Function[-1000.0] = 66.0;
            ((HydraulicRule) controlGroup2.Rules[0]).Function[+1000.0] = 66.0;

            return controlledModel;
        }

        public static ControlGroup SetupInvertorRule(ControlledTestModel controlledModel, RealTimeControlModel realTimeControlModel, ConditionBase conditionBase)
        {
            ControlGroup controlGroup = conditionBase == null
                                            ? RealTimeControlModelHelper.CreateGroupInvertorRule()
                                            : RealTimeControlModelHelper.CreateGroupRuleWithOneInputOneConditionInput(new FactorRule
                                            {
                                                Name = "InvertorRule",
                                                Factor = -1.0
                                            }, conditionBase);

            realTimeControlModel.ControlGroups.Add(controlGroup);

            IDataItem outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            new TestCompositeActivity
            {
                Activities =
                {
                    realTimeControlModel,
                    controlledModel
                }
            };

            return controlGroup;
        }

        public static ControlGroup SetupRelativeTimeRule(out ControlledTestModel controlledModel, out RealTimeControlModel realTimeControlModel, bool fromValue)
        {
            realTimeControlModel = new RealTimeControlModel();
            controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupRelativeTimeRule();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity
            {
                Activities =
                {
                    realTimeControlModel,
                    controlledModel
                }
            };

            var relativeTimeRule = (RelativeTimeRule) controlGroup.Rules[0];

            IDataItem outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            relativeTimeRule.FromValue = fromValue;
            relativeTimeRule.Function[0.0] = 33.0;
            relativeTimeRule.Function[10000.0] = 66.0;
            ((StandardCondition) controlGroup.Conditions[0]).Operation = Operation.Greater;
            controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active

            return controlGroup;
        }

        public static void SetupControlTestmodelWithFourInputs(out ControlledTestModel controlledModel, out RealTimeControlModel realTimeControlModel)
        {
            realTimeControlModel = new RealTimeControlModel();

            controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            controlledModel.InputFeatures.Clear();
            controlledModel.OutputFeatures.Clear();
            controlledModel.InputFeatures.AddRange(new[]
            {
                new RtcTestFeature {Name = "input_feature1"},
                new RtcTestFeature {Name = "input_feature2"},
                new RtcTestFeature {Name = "input_feature3"},
                new RtcTestFeature {Name = "input_feature4"}
            });
            controlledModel.OutputFeatures.AddRange(new[]
            {
                new RtcTestFeature {Name = "output_feature1"},
                new RtcTestFeature {Name = "output_feature2"}
            });
        }

        public static ControlGroup SetupPidRule(DateTime startTime, DateTime stopTime, TimeSpan timestep, out ControlledTestModel controlledModel, out RealTimeControlModel realTimeControlModel, out Input ruleInput, bool addCondition)
        {
            realTimeControlModel = new RealTimeControlModel();
            controlledModel = new ControlledTestModel
            {
                StartTime = startTime,
                StopTime = stopTime,
                TimeStep = timestep,
                InputFeatures =
                {
                    new RtcTestFeature {Name = "input1"},
                    new RtcTestFeature {Name = "input2"}
                },
                OutputFeatures =
                {
                    new RtcTestFeature {Name = "output1"},
                    new RtcTestFeature {Name = "output2"}
                }
            };

            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(addCondition);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity
            {
                Activities =
                {
                    realTimeControlModel,
                    controlledModel
                }
            };

            IDataItem outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            if (addCondition)
            {
                IDataItem outputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
                outputDataItem2.Value = 2.0;
                realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[1]).LinkTo(outputDataItem2);
            }

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var pidRule = (PIDRule) controlGroup.Rules[0];
            pidRule.Kd = 0.0;
            pidRule.Ki = 0.2;
            pidRule.Kp = 0.5;
            pidRule.Setting.Max = 123.6;
            pidRule.Setting.MaxSpeed = 0.2;
            pidRule.Setting.Min = 116.0;

            // Set timeseries; this is the series the interval rule will try to satisfy
            pidRule.TimeSeries[new DateTime(2000, 1, 1, 0, 0, 0)] = 100.0;
            pidRule.TimeSeries[new DateTime(2100, 1, 1, 0, 0, 0)] = 200.0;
            ruleInput = (Input) pidRule.Inputs[0];

            if (addCondition)
            {
                var conditionInput = (Input) controlGroup.Conditions[0].Input;
                ((StandardCondition) controlGroup.Conditions[0]).Operation = Operation.Greater;
                conditionInput.Value = 2;             // Value 
                controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active
            }

            return controlGroup;
        }

        public static ControlGroup GetControlGroupForRule(HydraulicRule rule)
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add((Input) rule.Inputs[0]);
            controlGroup.Outputs.Add(rule.Outputs[0]);

            var condition = new StandardCondition {Name = "Hydro Condition"};
            condition.Input = controlGroup.Inputs[0];
            condition.TrueOutputs.Add(rule);
            controlGroup.Conditions.Add(condition);

            return controlGroup;
        }

        public static HydraulicRule GetHydraulicRule()
        {
            var ruleInput = new Input();
            var ruleOutput = new Output();
            var rule = new HydraulicRule();

            rule.Inputs.Add(ruleInput);
            rule.Outputs.Add(ruleOutput);

            return rule;
        }

        public static ControlGroup CreateControlGroupWithDiverseRulesAndConditions()
        {
            var controlGroup = new ControlGroup();

            var input = new Input();
            controlGroup.Inputs.Add(input);
            var output = new Output();
            controlGroup.Outputs.Add(output);
            var output2 = new Output();
            controlGroup.Outputs.Add(output2);

            // First set of rules & conditions
            var directionalCondition = new DirectionalCondition {Name = "DirectionalCondition"};
            controlGroup.Conditions.Add(directionalCondition);

            var intervalRule = new IntervalRule {Name = "intervalRule"};
            controlGroup.Rules.Add(intervalRule);

            var hydraulicRule = new HydraulicRule {Name = "hydraulicRule"};
            hydraulicRule.Function[-2.0] = 0.0;
            hydraulicRule.Function[+2.0] = 4.0;
            controlGroup.Rules.Add(hydraulicRule);

            directionalCondition.TrueOutputs.Add(hydraulicRule);
            directionalCondition.FalseOutputs.Add(intervalRule);

            directionalCondition.Input = input;
            hydraulicRule.Inputs.Add(input);
            intervalRule.Inputs.Add(input);

            hydraulicRule.Outputs.Add(output2);
            intervalRule.Outputs.Add(output2);

            var timeCondition = new TimeCondition {Name = "TimeConditionAlwaysOn"};
            timeCondition.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition.TimeSeries[new DateTime(2200, 1, 1)] = false;
            timeCondition.Extrapolation = ExtrapolationType.Constant;
            controlGroup.Conditions.Add(timeCondition);

            var relativeTimeRule = new RelativeTimeRule {Name = "RelativeTimeRule"};
            relativeTimeRule.FromValue = false;
            relativeTimeRule.Function[0.0] = 2.0;
            relativeTimeRule.Function[3600.0] = 3.00;
            controlGroup.Rules.Add(relativeTimeRule);

            var pidRuleNotActive = new PIDRule {Name = "PIDRuleNotActive"};
            pidRuleNotActive.Kp = 0.3;
            pidRuleNotActive.PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.Constant;
            pidRuleNotActive.ConstantValue = 1.0;
            controlGroup.Rules.Add(pidRuleNotActive);

            timeCondition.TrueOutputs.Add(relativeTimeRule);
            timeCondition.FalseOutputs.Add(pidRuleNotActive);

            pidRuleNotActive.Inputs.Add(input);
            pidRuleNotActive.Outputs.Add(output);
            relativeTimeRule.Outputs.Add(output);

            return controlGroup;
        }

        public static int SetupAndExecuteRelativeTimeRuleSobekExample(out List<double> rtcOutput)
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            ControlGroup controlGroup = SetupRelativeTimeRule(out controlledModel, out realTimeControlModel, true);
            controlledModel.StopTime = controlledModel.StartTime.AddDays(15);
            controlledModel.TimeStep = new TimeSpan(1, 0, 0, 0); // 24 * 60 * 60 = 86400 seconds

            var relativeTimeRule = (RelativeTimeRule) controlGroup.Rules[0];
            relativeTimeRule.Function.Clear();
            relativeTimeRule.Function[0.0] = 0.0;
            relativeTimeRule.Function[172800.0] = -1.0;
            relativeTimeRule.Function[345600.0] = -2.0;
            relativeTimeRule.Function[518400.0] = -3.0;

            var condition = (StandardCondition) controlGroup.Conditions[0];
            condition.Operation = Operation.GreaterEqual;
            condition.Value = 0.9;

            IDataItem intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            intputDataItem1.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup.Conditions[0].Input).LinkTo(intputDataItem1);

            var waterLevelAtObservationPoint = new[]
            {
                1.00,
                0.91,
                0.80,
                0.70,
                0.70,
                0.70,
                0.70,
                1.00,
                1.00,
                0.90,
                0.80,
                0.80,
                1.00,
                1.00,
                1.00
            };

            // The initial value of the output must be set in the state vector
            controlGroup.Outputs[0].Value = 1;

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            //Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            //Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);
            var timeStepsCount = 0;
            rtcOutput = new List<double>();

            if (ActivityStatus.Initialized != realTimeControlModel.Status ||
                ActivityStatus.Initialized != controlledModel.Status)
            {
                return timeStepsCount;
            }

            // Run the models
            // For the RelativfeTimeFromValue the output is also used as input for RTCTools
            // Relative series is from 33 at t=0 to 66 at t=10000

            // If relative time rule is RELATIVE state vector (12) should be ignored
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = waterLevelAtObservationPoint[timeStepsCount];

                //Assert.AreEqual(new DateTime(2000, 1, 1 + timeStepsCount, 0, 0, 0), realTimeControlModel.CurrentTime);
                if (new DateTime(2000, 1, 1 + timeStepsCount, 0, 0, 0) != realTimeControlModel.CurrentTime)
                {
                    return 0;
                }

                realTimeControlModel.Execute();
                controlledModel.Execute();

                rtcOutput.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }

            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

//            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
//            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            if (ActivityStatus.Initialized != realTimeControlModel.Status ||
                ActivityStatus.Initialized != controlledModel.Status)
            {
                return timeStepsCount;
            }

            return timeStepsCount;
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

        private static ControlGroup CreateGroupTimeRuleWithoutCondition()
        {
            return CreateGroupRuleWithoutConditionWithoutInput(new TimeRule());
        }
    }
}