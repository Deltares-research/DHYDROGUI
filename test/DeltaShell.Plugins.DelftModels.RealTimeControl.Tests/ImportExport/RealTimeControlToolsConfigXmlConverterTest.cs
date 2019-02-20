using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlToolsConfigXmlConverterTest
    {
        private RealTimeControlToolsConfigXmlConverter toolsConfigConverter;
        private ILogHandler logHandler;
        private const string ControlGroupName = "control_group_name";
        private const string ComponentName = "component_name";
        private IControlGroup controlGroup;

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            toolsConfigConverter = new RealTimeControlToolsConfigXmlConverter(logHandler);
            controlGroup = new ControlGroup {Name = ControlGroupName};

            Assert.AreEqual(0, controlGroup.Rules.Count,
                "Initial number of rules are expected to be 0 for new control group.");
            Assert.AreEqual(0, controlGroup.Conditions.Count,
                "Initial number of conditions are expected to be 0 for new control group.");
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            toolsConfigConverter = null;
            controlGroup = null;
        }

        [TestCase(timeRelativeEnumStringType.ABSOLUTE, false, interpolationOptionEnumStringType.LINEAR, InterpolationType.Linear)]
        [TestCase(timeRelativeEnumStringType.RELATIVE, true, interpolationOptionEnumStringType.BLOCK, InterpolationType.Constant)]
        public void GivenARelativeTimeRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup(
            timeRelativeEnumStringType reference, bool expectedFromValue,
            interpolationOptionEnumStringType interpolationOption, InterpolationType expectedInterpolation)
        {
            // Given
            var ruleElement = CreateRelativeTimeRuleElement(ControlGroupName, interpolationOption, reference);

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> {ruleElement},
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            AssertRelativeTimeRuleValidity(expectedFromValue, expectedInterpolation);
        }

        [TestCase(PIDRule.PIDRuleSetpointType.Constant, 7d)]
        [TestCase(PIDRule.PIDRuleSetpointType.TimeSeries, 0d)]
        [TestCase(PIDRule.PIDRuleSetpointType.Signal, 0d)]
        public void GivenAPidRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup(
            PIDRule.PIDRuleSetpointType expectedSetpointType, object expectedConstantValue)
        {
            // Given
            var ruleElement = CreatePidRuleElement(ControlGroupName, expectedSetpointType);

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> {ruleElement},
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            AssertPidRuleValidity(expectedSetpointType, expectedConstantValue);
        }

        [TestCase(ItemChoiceType5.settingMaxStep, Item1ChoiceType3.deadbandSetpointAbsolute, IntervalRule.IntervalRuleIntervalType.Fixed, IntervalRule.IntervalRuleDeadBandType.Fixed, false)]
        [TestCase(ItemChoiceType5.settingMaxSpeed, Item1ChoiceType3.deadbandSetpointRelative, IntervalRule.IntervalRuleIntervalType.Variable, IntervalRule.IntervalRuleDeadBandType.PercentageDischarge, false)]
        [TestCase(ItemChoiceType5.settingMaxSpeed, Item1ChoiceType3.deadbandSetpointRelative, IntervalRule.IntervalRuleIntervalType.Signal, IntervalRule.IntervalRuleDeadBandType.PercentageDischarge, true)]
        public void GivenAnIntervalRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup(
            ItemChoiceType5 intervalType,
            Item1ChoiceType3 deadBandType,
            IntervalRule.IntervalRuleIntervalType expectedIntervalType,
            IntervalRule.IntervalRuleDeadBandType expectedDeadBandType,
            bool signalAsSetpoint)
        {
            // Given
            var ruleElement = CreateIntervalRuleElement(ControlGroupName, signalAsSetpoint, intervalType, deadBandType);

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> {ruleElement},
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            AssertIntervalRuleValidity(expectedIntervalType, expectedDeadBandType);
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR, InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK, InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void GivenAnHydraulicRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup(
            interpolationOptionEnumStringType interpolation,
            interpolationOptionEnumStringType extrapolation,
            InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Given
            var ruleElement = CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule, ControlGroupName, interpolation,
                extrapolation);

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> {ruleElement},
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            AssertHydraulicRuleValidity(expectedInterpolation, expectedExtrapolation);
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR, InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK, InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void GivenAFactorRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup(
            interpolationOptionEnumStringType interpolation,
            interpolationOptionEnumStringType extrapolation,
            InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Given
            var ruleElement = CreateLookupTableRuleElement(
                RtcXmlTag.FactorRule, ControlGroupName, interpolation, extrapolation);

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> {ruleElement},
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            AssertFactorRuleValidity(expectedInterpolation, expectedExtrapolation);
        }

        [TestCase(RtcXmlTag.StandardCondition, typeof(StandardCondition), inputReferenceEnumStringType.EXPLICIT, StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.Equal, Operation.Equal, 3.3)]
        [TestCase(RtcXmlTag.StandardCondition, typeof(StandardCondition), inputReferenceEnumStringType.IMPLICIT, StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Greater, Operation.Greater, 3.3)]
        [TestCase(RtcXmlTag.TimeCondition, typeof(TimeCondition), inputReferenceEnumStringType.EXPLICIT, StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.GreaterEqual, Operation.GreaterEqual, 3.3)]
        [TestCase(RtcXmlTag.TimeCondition, typeof(TimeCondition), inputReferenceEnumStringType.IMPLICIT, StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Less, Operation.Less, 3.3)]
        [TestCase(RtcXmlTag.DirectionalCondition, typeof(DirectionalCondition), inputReferenceEnumStringType.EXPLICIT, StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.LessEqual, Operation.LessEqual, 0.0)]
        [TestCase(RtcXmlTag.DirectionalCondition, typeof(DirectionalCondition), inputReferenceEnumStringType.IMPLICIT, StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Unequal, Operation.Unequal, 0.0)]
        public void GivenAStandardConditionElementAndAControlGroup_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_CorrectConditionIsCreatedAndAddedToControlGroup(
            string tag, Type expectedConditionType,
            inputReferenceEnumStringType reference, string expectedReference,
            relationalOperatorEnumStringType operatorType, Operation expectedOperation,
            double expectedValue)
        {
            // Given
            var conditionElement = CreateStandardConditionElement(
                tag, ControlGroupName, ComponentName, reference, operatorType);

            // When
            toolsConfigConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(
                new List<TriggerXML> {conditionElement},
                new[] {controlGroup});

            // Then
            AssertCommonConditionValidity();
            AssertStandardConditionValidity(expectedConditionType, expectedReference, expectedOperation, expectedValue);
        }

        [TestCase(true, 2)]
        [TestCase(false, 1)]
        public void GivenAStandardConditionElementWithATrueOutputAndAControlGroup_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_CorrectConditionsAreCreatedAndAddedToControlGroup(
            bool hasOutput, int expectedNumberOfConditions)
        {
            // Given
            var conditionElement = CreateStandardConditionElement(
                RtcXmlTag.StandardCondition, ControlGroupName, ComponentName, hasOutput: hasOutput);

            // When
            toolsConfigConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(
                new List<TriggerXML> {conditionElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedNumberOfConditions, controlGroup.Conditions.Count,
                $"Expected number of conditions in control group was {expectedNumberOfConditions}.");
        }

        [Test]
        public void GivenATimeRuleElementAndAControlGroup_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_CorrectRuleIsCreatedAndAddedToControlGroup()
        {
            // Given
            var ruleElements = new List<RuleXML>
            {
                CreateTimeRuleElement(ControlGroupName)
            };

            // When
            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                ruleElements,
                new[] {controlGroup});

            // Then
            AssertCommonRuleValidity();
            var rule = controlGroup.Rules.Single();
            Assert.AreEqual(typeof(TimeRule), rule.GetType());
        }

        [Test]
        public void GivenSomeRuleElements_WhenCreateControlGroupsFromXmlElementIDsIsCalled_CorrectControlGroupsAreMade()
        {
            // Given
            const string controlGroup1Name = "control_group1";
            const string controlGroup2Name = "control_group2";

            var ruleElements = new List<RuleXML>
            {
                CreateTimeRuleElement(controlGroup1Name),
                CreateIntervalRuleElement(controlGroup2Name, false),
                CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule, controlGroup1Name),
                CreatePidRuleElement(controlGroup2Name),
                CreateRelativeTimeRuleElement(controlGroup1Name)
            };

            // When
            var controlGroups = toolsConfigConverter.CreateControlGroupsFromXmlElementIDs(ruleElements)
                .ToList();

            // Then
            Assert.AreEqual(2, controlGroups.Count, "Number of control groups was expected to be 2.");
            Assert.AreEqual(controlGroup1Name, controlGroups.First().Name);
            Assert.AreEqual(controlGroup2Name, controlGroups.Last().Name);
        }

        [Test]
        public void GivenNullParameterForElements_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_ThenNothingHappens()
        {
            Assert.DoesNotThrow(
                () => toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                    null,
                    new List<IControlGroup>()),
                "Method throws an unexpected exception when parameter 'ruleElements' is null.");
        }

        [Test]
        public void GivenNullParameterForControlGroups_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_ThenNothingHappens()
        {
            Assert.DoesNotThrow(
                () => toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(
                    new List<RuleXML>(),
                    null),
                "Method throws an unexpected exception when parameter 'controlGroups' is null.");
        }

        [Test]
        public void GivenNullParameterForElements_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_ThenNothingHappens()
        {
            Assert.DoesNotThrow(
                () => toolsConfigConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(
                    null,
                    new List<IControlGroup>()),
                "Method throws an unexpected exception when parameter 'conditionElements' is null.");
        }

        [Test]
        public void GivenNullParameterForControlGroups_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_ThenNothingHappens()
        {
            Assert.DoesNotThrow(
                () => toolsConfigConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(
                    new List<TriggerXML>(),
                    null),
                "Method throws an unexpected exception when parameter 'controlGroups' is null.");
        }

        [Test]
        public void
            GivenRuleElementsWithSignals_WhenSeparateSignalsFromRulesIsCalled_ThenSignalElementsShouldBeMovedToTheirOwnElements()
        {
            var signalElements = new List<RuleXML>();

            var ruleElements = new List<RuleXML>
            {
                CreateTimeRuleElement(ControlGroupName),
                CreateIntervalRuleElement(ControlGroupName, false),
                CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule, ControlGroupName),
                CreateLookupTableRuleElement(RtcXmlTag.LookupSignal, ControlGroupName),
                CreatePidRuleElement(ControlGroupName),
                CreateRelativeTimeRuleElement(ControlGroupName)
            };

            Assert.AreEqual(6, ruleElements.Count);
            Assert.AreEqual(0, signalElements.Count);

            // When

            toolsConfigConverter.SeparateSignalsFromRules(ruleElements, signalElements);

            // Then
            Assert.AreEqual(5, ruleElements.Count);
            Assert.AreEqual(1, signalElements.Count);
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR, InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK, InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void GivenASignalElementAndAControlGroup_WhenCreateSignalsFromXmlElementsAndAddToControlGroupIsCalled_CorrectSignalIsCreatedAndAddedToControlGroup(
            interpolationOptionEnumStringType interpolation,
            interpolationOptionEnumStringType extrapolation,
            InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Given
            var signalElement = CreateLookupTableRuleElement(RtcXmlTag.LookupSignal, ControlGroupName, interpolation,
                extrapolation);

            // When
            toolsConfigConverter.CreateSignalsFromXmlElementsAndAddToControlGroup(
                new List<RuleXML> { signalElement },
                new[] { controlGroup });

            // Then
            AssertCommonSignalValidity();
            AssertSignalValidity(expectedInterpolation, expectedExtrapolation);
        }

        private void AssertCommonConditionValidity()
        {
            Assert.AreEqual(1, controlGroup.Conditions.Count, 
                "Number of control groups was expected to be 1.");
            var condition = controlGroup.Conditions.Single();
            Assert.AreEqual(ComponentName, condition.Name);
        }

        private void AssertCommonRuleValidity()
        {
            Assert.AreEqual(1, controlGroup.Rules.Count,
                "Number of rules was expected to be 1.");
            var rule = controlGroup.Rules.Single();
            Assert.AreEqual(ComponentName, rule.Name);
        }

        private void AssertCommonSignalValidity()
        {
            Assert.AreEqual(1, controlGroup.Signals.Count,
                "Number of signals was expected to be 1.");
            var signal = controlGroup.Signals.Single();
            Assert.AreEqual(ComponentName, signal.Name);
        }

        private static RuleXML CreateTimeRuleElement(string controlGroupName)
        {
            var timeRuleElement = new TimeAbsoluteXML
            {
                id = RtcXmlTag.TimeRule + controlGroupName + "/" + ComponentName
            };

            var ruleElement = new RuleXML
            {
                Item = timeRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateRelativeTimeRuleElement(string controlGroupName,
            interpolationOptionEnumStringType interpolationOption = interpolationOptionEnumStringType.BLOCK,
            timeRelativeEnumStringType reference = timeRelativeEnumStringType.ABSOLUTE)
        {
            var timeRelativeRuleElement = new TimeRelativeXML
            {
                id = RtcXmlTag.RelativeTimeRule + controlGroupName + "/" + ComponentName,
                mode = TimeRelativeXMLMode.RETAINVALUEWHENINACTIVE,
                valueOption = reference,
                maximumPeriod = 1,
                interpolationOption = interpolationOption,
                controlTable = new List<TimeRelativeControlTableRecordXML>
                {
                    new TimeRelativeControlTableRecordXML
                    {
                        time = 60,
                        value = 10
                    },
                    new TimeRelativeControlTableRecordXML
                    {
                        time = 600,
                        value = 100
                    }
                }
            };

            var ruleElement = new RuleXML
            {
                Item = timeRelativeRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreatePidRuleElement(string controlGroupName, PIDRule.PIDRuleSetpointType expectedSetpointType = PIDRule.PIDRuleSetpointType.Constant)
        {
            var pidRuleElement = new PidXML
            {
                id = RtcXmlTag.PIDRule + controlGroupName + "/" + ComponentName,
                mode = PidXMLMode.PIDVEL,
                settingMin = 1,
                settingMax = 2,
                settingMaxSpeed = 3,
                kp = 4,
                ki = 5,
                kd = 6
            };

            if (expectedSetpointType == PIDRule.PIDRuleSetpointType.Constant)
                pidRuleElement.input.Item = 7.0d;
            else if (expectedSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries)
                pidRuleElement.input.Item = RtcXmlTag.SP + controlGroupName + "/" + ComponentName;
            else
                pidRuleElement.input.Item = RtcXmlTag.Signal + controlGroupName + "/" + ComponentName;

            var ruleElement = new RuleXML
            {
                Item = pidRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateIntervalRuleElement(string controlGroupName, bool signalAsSetpoint,
            ItemChoiceType5 intervalType = ItemChoiceType5.settingMaxSpeed,
            Item1ChoiceType3 deadBandType = Item1ChoiceType3.deadbandSetpointAbsolute)
        {
            var intervalRuleElement = new IntervalXML
            {
                id = RtcXmlTag.IntervalRule + controlGroupName + "/" + ComponentName,
                settingBelow = 1,
                settingAbove = 2,
                Item = 3, // interval
                ItemElementName = intervalType,
                Item1 = 4,
                Item1ElementName = deadBandType,
                input = new IntervalInputXML()
                {
                   setpoint = RtcXmlTag.SP + controlGroupName + ComponentName
                }
            };

            if (signalAsSetpoint)
            {
                intervalRuleElement.input.setpoint = RtcXmlTag.Signal + controlGroupName + "signal1";
            }

            var ruleElement = new RuleXML
            {
                Item = intervalRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateLookupTableRuleElement(string tag, string controlGroupName,
            interpolationOptionEnumStringType interpolation = interpolationOptionEnumStringType.BLOCK,
            interpolationOptionEnumStringType extrapolation = interpolationOptionEnumStringType.BLOCK)
        {
            var lookupTableRuleElement = new LookupTableXML
            {
                id = tag + controlGroupName + "/" + ComponentName,
                Item = new TableLookupTableXML
                {
                    record = new List<DateRecord2DataXML>
                    {
                        new DateRecord2DataXML {x = 1, y = 5},
                        new DateRecord2DataXML {x = 2, y = 4}
                    }
                },
                interpolationOption = interpolation,
                extrapolationOption = extrapolation
            };

            var ruleElement = new RuleXML
            {
                Item = lookupTableRuleElement
            };

            return ruleElement;
        }

        private static TriggerXML CreateStandardConditionElement(string tag, string controlGroupName,
            string conditionName,
            inputReferenceEnumStringType referencType = inputReferenceEnumStringType.EXPLICIT,
            relationalOperatorEnumStringType operatorType = relationalOperatorEnumStringType.Equal,
            bool hasOutput = false)
        {
            object item1;

            if (tag == RtcXmlTag.DirectionalCondition)
                item1 = new RelationalConditionXMLX2Series
                {
                    @ref = inputReferenceEnumStringType.EXPLICIT
                };

            else
                item1 = "3.3";

            var standardConditionElement = new StandardTriggerXML
            {
                id = tag + controlGroupName + "/" + conditionName,
                condition = new RelationalConditionXML
                {
                    Item = new RelationalConditionXMLX1Series
                    {
                        @ref = referencType
                    },
                    relationalOperator = operatorType,
                    Item1 = item1
                }
            };

            if (hasOutput)
                standardConditionElement.@true = new List<TriggerXML>
                {
                    CreateStandardConditionElement(tag, controlGroupName, conditionName + ":true_output")
                };

            var conditionElement = new TriggerXML
            {
                Item = standardConditionElement
            };

            return conditionElement;
        }

        private void AssertPidRuleValidity(PIDRule.PIDRuleSetpointType expectedSetpointType,
            object expectedConstantValue)
        {
            var rule = controlGroup.Rules.Single();
            var pidRule = rule as PIDRule;
            Assert.NotNull(pidRule);

            var setting = pidRule.Setting;
            Assert.AreEqual(1d, setting.Min, 
                $"Pid rule: minimum settings was expected to be {1d}.");
            Assert.AreEqual(2d, setting.Max, 
                $"Pid rule: maximum settings was expected to be {2d}.");
            Assert.AreEqual(3d, setting.MaxSpeed, 
                $"Pid rule: maximum speed was expected to be {3d}.");
            Assert.AreEqual(4d, pidRule.Kp, 
                $"Pid rule: Kp was expected to be {4d}.");
            Assert.AreEqual(5d, pidRule.Ki, 
                $"Pid rule: Ki was expected to be {5d}.");
            Assert.AreEqual(6d, pidRule.Kd, 
                $"Pid rule: Kd was expected to be {6d}.");
            Assert.AreEqual(expectedSetpointType, pidRule.PidRuleSetpointType, 
                $"Pid rule: setpoint type was expected to be {expectedSetpointType.ToString()}.");
            Assert.AreEqual(expectedConstantValue, pidRule.ConstantValue,
                $"Pid rule: constant value was expected to be {expectedConstantValue}.");
        }

        private void AssertRelativeTimeRuleValidity(bool expectedFromValue, InterpolationType expectedInterpolation)
        {
            var rule = controlGroup.Rules.Single();
            var relativeTimeRule = rule as RelativeTimeRule;
            Assert.NotNull(relativeTimeRule);
            Assert.AreEqual(expectedFromValue, relativeTimeRule.FromValue, 
                $"Relative time rule: from value was expected to be {expectedFromValue.ToString()}.");
            Assert.AreEqual(1, relativeTimeRule.MinimumPeriod,
                $"Relative time rule: minimum period was expected to be 1.");
            Assert.AreEqual(expectedInterpolation, relativeTimeRule.Interpolation,
                $"Relative time rule: interpolation was expected to be {expectedInterpolation.ToString()}.");

            var function = relativeTimeRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[60d], 10d);
            Assert.AreEqual(function[600d], 100d);
        }

        private void AssertIntervalRuleValidity(IntervalRule.IntervalRuleIntervalType expectedIntervalType,
            IntervalRule.IntervalRuleDeadBandType expectedDeadBandType)
        {
            var rule = controlGroup.Rules.Single();
            var intervalRule = rule as IntervalRule;
            Assert.NotNull(intervalRule);

            var setting = intervalRule.Setting;
            Assert.AreEqual(1d, setting.Below,
                $"Interval rule: settings below was expected to be {1d}.");
            Assert.AreEqual(2d, setting.Above,
                $"Interval rule: settings above was expected to be {2d}.");
            Assert.AreEqual(expectedIntervalType, intervalRule.IntervalType,
                $"Interval rule: interval type was expected to be {expectedIntervalType.ToString()}.");
            Assert.AreEqual(expectedDeadBandType, intervalRule.DeadBandType,
                $"Interval rule: dead band type was expected to be {expectedDeadBandType.ToString()}.");
            Assert.AreEqual(4d, intervalRule.DeadbandAroundSetpoint,
                $"Interval rule: dead band around set point was expected to be {4d}.");

            var expectedIntervalValue = 3d;
            if (expectedIntervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                Assert.AreEqual(expectedIntervalValue, intervalRule.FixedInterval,
                    $"Interval rule: fixed interval was expected to be {expectedIntervalValue}.");
                Assert.AreEqual(0.0d, setting.MaxSpeed,
                    $"Interval rule: maximum speed was expected to be {0d}.");
            }
            else
            {
                Assert.AreEqual(0.0d, intervalRule.FixedInterval,
                    $"Interval rule: fixed interval was expected to be {0d}.");
                Assert.AreEqual(expectedIntervalValue, setting.MaxSpeed,
                    $"Interval rule: maximum speed was expected to be {expectedIntervalValue}.");
            }
        }

        private void AssertSignalValidity(InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            var signal = controlGroup.Signals.Single();
            var lookupSignal = signal as LookupSignal;
            Assert.NotNull(lookupSignal);

            var function = lookupSignal.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[1d], 5d);
            Assert.AreEqual(function[2d], 4d);
            Assert.AreEqual(expectedInterpolation.ToString(), lookupSignal.Interpolation.ToString(),
                $"Signal: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), lookupSignal.Extrapolation.ToString(),
                $"Signal: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }
        private void AssertHydraulicRuleValidity(InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            var rule = controlGroup.Rules.Single();
            var hydraulicRule = rule as HydraulicRule;
            Assert.NotNull(hydraulicRule);

            var function = hydraulicRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[1d], 5d);
            Assert.AreEqual(function[2d], 4d);
            Assert.AreEqual(expectedInterpolation.ToString(), hydraulicRule.Interpolation.ToString(),
                $"Hydraulic rule: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), hydraulicRule.Extrapolation.ToString(),
                $"Hydraulic rule: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }

        private void AssertFactorRuleValidity(InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            var rule = controlGroup.Rules.Single();
            var factorRule = rule as FactorRule;
            Assert.NotNull(factorRule);

            var function = factorRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[-1d], 5d);
            Assert.AreEqual(function[1d], -5d);
            Assert.AreEqual(-5d, factorRule.Factor);
            Assert.AreEqual(expectedInterpolation.ToString(), factorRule.Interpolation.ToString(),
                $"Factor rule: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), factorRule.Extrapolation.ToString(),
                $"Factor rule: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }

        private void AssertStandardConditionValidity(Type expectedConditionType, string expectedReference,
            Operation expectedOperation, double expectedValue)
        {
            var condition = controlGroup.Conditions.Single();
            var standardCondition = condition as StandardCondition;
            Assert.NotNull(standardCondition);
            Assert.AreEqual(expectedConditionType, standardCondition.GetType(),
                $"Standard condition: condition type was expected to be {expectedConditionType}.");
            Assert.AreEqual(expectedReference, standardCondition.Reference,
                $"Standard condition: reference was expected to be {expectedReference}.");
            Assert.AreEqual(expectedOperation, standardCondition.Operation,
                $"Standard condition: operation was expected to be {expectedOperation.ToString()}.");
            Assert.AreEqual(expectedValue, standardCondition.Value,
                $"Standard condition: value was expected to be {expectedValue}.");
        }
    }
}