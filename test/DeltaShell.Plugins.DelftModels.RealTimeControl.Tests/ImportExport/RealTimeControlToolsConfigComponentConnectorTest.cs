using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlToolsConfigComponentConnectorTest
    {
        private RealTimeControlToolsConfigComponentConnector toolsConfigComponentConnector;
        private ILogHandler logHandler;
        private const string ControlGroupName = "control_group_name";
        private const string OutputName = "output_name";
        private const string InputName = "input_name";
        private const string ComponentName = "component_name";
        private IControlGroup controlGroup;
        private Output output;
        private Input input;
        private IList<ConnectionPoint> connectionPoints;

        private const string TrueOutputName = "true_output_rule";
        private const string FalseOutput1Name = "false_output_condition";
        private const string FalseOutput2Name = "false_output_false_output_condition";

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            toolsConfigComponentConnector = new RealTimeControlToolsConfigComponentConnector(logHandler);
            controlGroup = new ControlGroup {Name = ControlGroupName};
            output = new Output {Name = OutputName};
            input = new Input {Name = InputName};
            connectionPoints = new List<ConnectionPoint> {output, input};
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            toolsConfigComponentConnector = null;
            controlGroup = null;
            output = null;
            input = null;
            connectionPoints = null;
        }

        [Test]
        public void GivenASignalElementAndAControlGroupWithThisSignal_WhenConnectSignalIsCalled_CorrectInputIsAddedToRule()
        {
            // Given
            var signalElement = CreateLookupTableRuleElement(RtcXmlTag.LookupSignal);
            var signal = new LookupSignal {Name = ComponentName};
            controlGroup.Signals.Add(signal);

            // When
            toolsConfigComponentConnector.ConnectSignals(
                new[] {signalElement},
                new[] {controlGroup},
                connectionPoints);

            // Then
            Assert.AreEqual(1, signal.Inputs.Count);
            Assert.AreEqual(InputName, signal.Inputs.Single().Name);
        }

        [Test]
        public void GivenATimeRuleElementAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateTimeRuleElement();
            var rule = new TimeRule(ComponentName);
            controlGroup.Rules.Add(rule);

            AssertNoOutputs(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                new ConnectionPoint[] {output});

            // Then
            AssertValidityOutput(rule);
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching rule
        public void GivenATimeRuleElementAndWithoutAMatchingControlGroupOrRule_WhenConnectRulesIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var ruleElement = CreateTimeRuleElement();
            controlGroup.Name = controlGroupName;

            // When, Then
            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectRules(
                    new List<RuleXML> {ruleElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or rule");
        }

        [Test]
        public void GivenARelativeTimeRuleElementAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateRelativeTimeRuleElement();
            var rule = new RelativeTimeRule(ComponentName, false);
            controlGroup.Rules.Add(rule);

            AssertNoOutputs(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                new ConnectionPoint[] {output});

            // Then
            AssertValidityOutput(rule);
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching rule
        public void GivenARelativeTimeRuleElementAndWithoutAMatchingControlGroupOrRule_WhenConnectRulesIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var ruleElement = CreateRelativeTimeRuleElement();
            controlGroup.Name = controlGroupName;

            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectRules(
                    new List<RuleXML> {ruleElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or rule");
        }

        [Test]
        public void GivenARelativeTimeRuleElementAndAControlGroupWithoutThisRule_WhenConnectRulesIsCalled_CorrectLogMessageIsGenerated()
        {
            // Given
            const string id = RtcXmlTag.RelativeTimeRule + ControlGroupName + "/" + ComponentName;
            var ruleElement = CreateRelativeTimeRuleElement();

            var expectedMessage = string.Format(
                Resources.RealTimeControlToolsConfigComponentConnector_ConnectRelativeTimeRules_Could_not_find_Relative_Time_Rule_with_id___0____See_file____1___,
                id, RealTimeControlXMLFiles.XmlTools);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                new ConnectionPoint[] {output});

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
                "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAPidRuleElementWithConstantSetPointAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreatePidRuleElementWithConstantSetPoint();
            var rule = new PIDRule(ComponentName);
            controlGroup.Rules.Add(rule);

            AssertNoConnectionPoints(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
        }

        [Test]
        public void GivenAPidRuleElementWithSignalAsSetpointAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRuleAndRuleToSignal()
        {
            // Given
            var ruleElement = CreatePidRuleElementWithSignalSetPoint();
            var rule = new PIDRule(ComponentName);
            controlGroup.Rules.Add(rule);
            var signal = new LookupSignal("Signal1");
            controlGroup.Signals.Add(signal);

            AssertNoConnectionPoints(rule);
            Assert.AreEqual(0, signal.RuleBases.Count);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] { ruleElement },
                new[] { controlGroup },
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
            Assert.IsNotNull(signal.RuleBases);
            Assert.AreEqual(ComponentName, signal.RuleBases.First().Name);
        }

        [Test]
        public void GivenAPidRuleElementWithoutSetPointInFileAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreatePidRuleElementNullSetPoint();
            var rule = new PIDRule(ComponentName);
            controlGroup.Rules.Add(rule);

            const string id = RtcXmlTag.PIDRule + ControlGroupName + "/" + ComponentName;
            var expectedMessage = string.Format(
                Resources.RealTimeControlDataConfigXmlSetter_SetSetPointOnPIDRules_PID_rule___0___must_have_a_setpoint__Please_check_file____1___,
                id, RealTimeControlXMLFiles.XmlTools);


            AssertNoConnectionPoints(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] { ruleElement },
                new[] { controlGroup },
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
                "The collected log messages did not contain the expected message.");
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching rule
        public void GivenAPidRuleElementAndWithoutAMatchingControlGroupOrRule_WhenConnectRulesIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var ruleElement = CreatePidRuleElementWithConstantSetPoint();
            controlGroup.Name = controlGroupName;

            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectRules(
                    new List<RuleXML> {ruleElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or rule.");
        }

        [Test]
        public void GivenAnIntervalRuleElementWithConstantSetPointAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateIntervalRuleElementWithConstantSetPoint();
            var rule = new IntervalRule(ComponentName);
            controlGroup.Rules.Add(rule);

            AssertNoConnectionPoints(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
        }

        [Test]
        public void GivenAnIntervalRuleElementWithSignalSetPointAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateIntervalRuleElementWithSignalSetPoint();
            var rule = new IntervalRule(ComponentName);
            controlGroup.Rules.Add(rule);
            var signal = new LookupSignal("Signal1");
            controlGroup.Signals.Add(signal);

            AssertNoConnectionPoints(rule);
            Assert.AreEqual(0, signal.RuleBases.Count);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] { ruleElement },
                new[] { controlGroup },
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
            Assert.IsNotNull(signal.RuleBases);
            Assert.AreEqual(ComponentName, signal.RuleBases.First().Name);
        }

        [Test]
        public void GivenAnIntervalRuleElementWithoutSetPointInFileAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateIntervalRuleElementWithNullSetPoint();
            var rule = new IntervalRule(ComponentName);
            controlGroup.Rules.Add(rule);

            const string id = RtcXmlTag.IntervalRule + ControlGroupName + "/" + ComponentName;
            var expectedMessage = string.Format(
                Resources.RealTimeControlDataConfigXmlSetter_SetSetPointOnIntervalRules_Interval_rule___0___must_have_a_setpoint__Please_check_file____1___,
                id, RealTimeControlXMLFiles.XmlTools);

            AssertNoConnectionPoints(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] { ruleElement },
                new[] { controlGroup },
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
                "The collected log messages did not contain the expected message.");
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching rule
        public void GivenAnIntervalRuleElementAndWithoutAMatchingControlGroupOrRule_WhenConnectRulesIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var ruleElement = CreateIntervalRuleElementWithConstantSetPoint();
            controlGroup.Name = controlGroupName;

            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectRules(
                    new List<RuleXML> {ruleElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or rule");
        }

        [Test]
        public void GivenALookupTableRuleElementAndAControlGroupWithThisRule_WhenConnectRulesIsCalled_CorrectInputAndOutputIsAddedToRule()
        {
            // Given
            var ruleElement = CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule);
            var rule = new HydraulicRule {Name = ComponentName};
            controlGroup.Rules.Add(rule);

            AssertNoConnectionPoints(rule);

            // When
            toolsConfigComponentConnector.ConnectRules(
                new[] {ruleElement},
                new[] {controlGroup},
                connectionPoints);

            // Then
            AssertValidityConnectionPoints(rule);
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching rule
        public void GivenALookupTableRuleElementAndWithoutAMatchingControlGroupOrRule_WhenConnectRulesIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var ruleElement = CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule);
            controlGroup.Name = controlGroupName;

            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectRules(
                    new List<RuleXML> {ruleElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or rule");
        }

        [Test]
        public void GivenAStandardConditionElementAndAControlGroupWithThisCondition_WhenConnectConditionsIsCalled_CorrectOutputAndInputIsAddedToCondition()
        {
            // Given
            var conditionElement = CreateStandardConditionElementWithTrueAndFalseOutput();

            var condition = new StandardCondition { Name = ComponentName };
            var trueOutputRule = new HydraulicRule { Name = TrueOutputName };
            var falseOutputCondition1 = new TimeCondition { Name = FalseOutput1Name };
            var falseOutputCondition2 = new DirectionalCondition { Name = FalseOutput2Name };

            AddAllComponentsToControlGroup(condition, falseOutputCondition1, falseOutputCondition2, trueOutputRule);

            CheckValidityInitialCondition(condition);
            // When
            toolsConfigComponentConnector.ConnectConditions(
                new List<TriggerXML> {conditionElement},
                new[] {controlGroup},
                new ConnectionPoint[] {input});

            // Then
            CheckValidityResultedCondition(condition, trueOutputRule, falseOutputCondition1, falseOutputCondition2);
        }

        [TestCase("no_match")] // no matching control group
        [TestCase(ControlGroupName)] // no matching condition
        public void GivenAStandardConditionElementAndWithoutAMatchingControlGroupOrCondition_WhenConnectConditionsIsCalled_ThenNothingHappens(string controlGroupName)
        {
            // Given
            var conditionElement = CreateStandardConditionElementWithTrueAndFalseOutput();
            controlGroup.Name = controlGroupName;

            Assert.DoesNotThrow(() => toolsConfigComponentConnector.ConnectConditions(
                    new List<TriggerXML> {conditionElement},
                    new[] {controlGroup},
                    new ConnectionPoint[] {input}),
                "Method throws an unexpected exception when there is no matching control group or condition");
        }

        private static TriggerXML CreateStandardConditionElementWithTrueAndFalseOutput()
        {
            var trueOutputElement = CreateTrueOutput();
            var falseOutputElement =
                CreateFalseOutput(RtcXmlTag.TimeCondition, ControlGroupName, FalseOutput1Name, FalseOutput2Name);
            var conditionElement =
                CreateStandardConditionElement(RtcXmlTag.StandardCondition, trueOutputElement, falseOutputElement);
            return conditionElement;
        }

        private static void CheckValidityResultedCondition(StandardCondition condition, HydraulicRule trueOutputRule,
            TimeCondition falseOutputCondition1, DirectionalCondition falseOutputCondition2)
        {
            Assert.NotNull(condition.Input);
            Assert.AreEqual(1, condition.TrueOutputs.Count,
                "Number of true outputs of the standard condition was expected to be 1.");
            Assert.AreSame(trueOutputRule, condition.TrueOutputs.Single());

            var falseOutputs = condition.FalseOutputs;
            Assert.AreEqual(1, falseOutputs.Count,
                "Number of false outputs of the standard condition was expected to be 1.");

            var falseOutput1 = falseOutputs.Single() as TimeCondition;
            Assert.NotNull(falseOutput1);
            Assert.AreSame(falseOutputCondition1, falseOutput1,
                "Standard condition did not connect to correct condition for false output.");

            falseOutputs = falseOutputCondition1.FalseOutputs;
            Assert.AreEqual(1, falseOutputs.Count, 
                "Number of false outputs of the standard condition was expected to be 1.");

            var falseOutput2 = falseOutputs.Single() as DirectionalCondition;
            Assert.NotNull(falseOutput2);
            Assert.AreSame(falseOutputCondition2, falseOutput2,
                "Standard condition did not connect to correct condition for false output.");
        }

        private static void CheckValidityInitialCondition(StandardCondition condition)
        {
            Assert.IsNull(condition.Input);
            Assert.AreEqual(0, condition.TrueOutputs.Count, "Initial number of true outputs for standard condition was expected to be 0.");
            Assert.AreEqual(0, condition.FalseOutputs.Count, "Initial number of false outputs for standard condition was expected to be 0.");
        }

        private void AddAllComponentsToControlGroup(StandardCondition condition, TimeCondition falseOutputCondition1,
            DirectionalCondition falseOutputCondition2, HydraulicRule trueOutputRule)
        {
            controlGroup.Conditions.Add(condition);
            controlGroup.Conditions.Add(falseOutputCondition1);
            controlGroup.Conditions.Add(falseOutputCondition2);
            controlGroup.Rules.Add(trueOutputRule);
        }

        private static TriggerXML CreateTrueOutput()
        {
            var trueOutputElement = new TriggerXML
            {
                Item = TrueOutputName
            };
            return trueOutputElement;
        }

        private static TriggerXML CreateFalseOutput(string tag, string controlGroupName, string falseOutputName1, string falseOutputName2 = null)
        {
            var item = new StandardTriggerXML
            {
                id = tag + controlGroupName + "/" + falseOutputName1
            };
            if (falseOutputName2 != null)
            {
                item.@false.Add(CreateFalseOutput(RtcXmlTag.DirectionalCondition, controlGroupName, falseOutputName2));
            }

            var falseOutputElement = new TriggerXML
            {
                Item = item
            };

            return falseOutputElement;
        }

        private static RuleXML CreateTimeRuleElement()
        {
            var timeRuleElement = new TimeAbsoluteXML
            {
                id = RtcXmlTag.TimeRule + ControlGroupName + "/" + ComponentName,
                output = new TimeAbsoluteOutputXML
                {
                    y = OutputName
                },
            };

            var ruleElement = new RuleXML
            {
                Item = timeRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateRelativeTimeRuleElement()
        {
            var timeRelativeRuleElement = new TimeRelativeXML
            {
                id = RtcXmlTag.RelativeTimeRule + ControlGroupName + "/" + ComponentName,
                output = new TimeRelativeOutputXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = timeRelativeRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreatePidRuleElementWithConstantSetPoint()
        {
            var pidRuleElement = new PidXML
            {
                id = RtcXmlTag.PIDRule + ControlGroupName + "/" + ComponentName,
                input = new InputPidXML
                {
                    x = InputName,
                    Item = 4
                },
                output = new OutputPidXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = pidRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreatePidRuleElementWithSignalSetPoint()
        {
            var pidRuleElement = new PidXML
            {
                id = RtcXmlTag.PIDRule + ControlGroupName + "/" + ComponentName,
                input = new InputPidXML
                {
                    x = InputName,
                    Item = RtcXmlTag.Signal + ControlGroupName + "/" + "Signal1",
                },
                output = new OutputPidXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = pidRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreatePidRuleElementNullSetPoint()
        {
            var pidRuleElement = new PidXML
            {
                id = RtcXmlTag.PIDRule + ControlGroupName + "/" + ComponentName,
                input = new InputPidXML
                {
                    x = InputName,
                    Item = null
                },
                output = new OutputPidXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = pidRuleElement
            };

            return ruleElement;
        }
        private static RuleXML CreateIntervalRuleElementWithConstantSetPoint()
        {
            var intervalRuleElement = new IntervalXML
            {
                id = RtcXmlTag.IntervalRule + ControlGroupName + "/" + ComponentName,
                input = new IntervalInputXML
                {
                    x = new IntervalInputXMLX
                    {
                        Value = InputName
                    },
                    setpoint = RtcXmlTag.SP + ControlGroupName + "/" + ComponentName
                },
                output = new IntervalOutputXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = intervalRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateIntervalRuleElementWithSignalSetPoint()
        {
            var intervalRuleElement = new IntervalXML
            {
                id = RtcXmlTag.IntervalRule + ControlGroupName + "/" + ComponentName,
                input = new IntervalInputXML
                {
                    x = new IntervalInputXMLX
                    {
                        Value = InputName
                    },
                    setpoint = RtcXmlTag.Signal + ControlGroupName + "/" + "Signal1",
                },
                output = new IntervalOutputXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = intervalRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateIntervalRuleElementWithNullSetPoint()
        {
            var intervalRuleElement = new IntervalXML
            {
                id = RtcXmlTag.IntervalRule + ControlGroupName + "/" + ComponentName,
                input = new IntervalInputXML
                {
                    x = new IntervalInputXMLX
                    {
                        Value = InputName
                    },
                    setpoint = null
                },
                output = new IntervalOutputXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = intervalRuleElement
            };

            return ruleElement;
        }

        private static RuleXML CreateLookupTableRuleElement(string tag)
        {
            var lookupTableRuleElement = new LookupTableXML
            {
                id = tag + ControlGroupName + "/" + ComponentName,
                input = new LookupTableInputXML
                {
                    x = new LookupTableInputXMLX
                    {
                        Value = InputName
                    }
                },
                output = new LookupTableOutputXML
                {
                    y = OutputName
                }
            };

            var ruleElement = new RuleXML
            {
                Item = lookupTableRuleElement
            };

            return ruleElement;
        }

        private static TriggerXML CreateStandardConditionElement(string tag, TriggerXML trueCondition = null, TriggerXML falseCondition = null)
        {
            var standardConditionElement = new StandardTriggerXML
            {
                id = tag + ControlGroupName + "/" + ComponentName,
            };

            if (trueCondition != null && falseCondition != null)
            {
                standardConditionElement.@true.Add(trueCondition);
                standardConditionElement.@false.Add(falseCondition);
            }

            standardConditionElement.condition.Item = new RelationalConditionXMLX1Series
            {
                Value = InputName
            };

            var conditionElement = new TriggerXML
            {
                Item = standardConditionElement
            };

            return conditionElement;
        }

        private static void AssertValidityConnectionPoints(RuleBase rule)
        {
            AssertValidityInput(rule);
            AssertValidityOutput(rule);
        }

        private static void AssertValidityInput(RuleBase rule)
        {
            Assert.AreEqual(1, rule.Inputs.Count, "Rule was expected to have 1 input.");
            Assert.AreEqual(InputName, rule.Inputs.Single().Name);
        }

        private static void AssertValidityOutput(RuleBase rule)
        {
            Assert.AreEqual(1, rule.Outputs.Count, "Rule was expected to have 1 output.");
            Assert.AreEqual(OutputName, rule.Outputs.Single().Name);
        }

        private static void AssertNoConnectionPoints(RuleBase rule)
        {
            AssertNoInputs(rule);
            AssertNoOutputs(rule);
        }

        private static void AssertNoInputs(RuleBase rule)
        {
            Assert.AreEqual(0, rule.Inputs.Count, 
                "Rule was expected to initially have zero inputs.");
        }

        private static void AssertNoOutputs(RuleBase rule)
        {
            Assert.AreEqual(0, rule.Outputs.Count, 
                "Rule was expected to initially have zero outputs.");
        }
    }
}
