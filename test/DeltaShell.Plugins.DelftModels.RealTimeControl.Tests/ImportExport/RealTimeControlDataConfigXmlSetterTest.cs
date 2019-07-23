using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlDataConfigXmlSetterTest
    {
        private RealTimeControlDataConfigXmlSetter dataConfigSetter;
        private ILogHandler logHandler;
        private IControlGroup controlGroup;
        private const string InputName = "input_name";
        private const string ComponentName = "component_name";
        private const string ControlGroupName = "control_group_name";
        private readonly TimeSpan timeStep = new TimeSpan(0, 1, 0, 0);

        private const string AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage = "The collected log messages did not contain the expected message.";
        private const string AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero = "Number of logged messages was expected to be zero.";

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            dataConfigSetter = new RealTimeControlDataConfigXmlSetter(logHandler);
            controlGroup = new ControlGroup {Name = ControlGroupName};
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            dataConfigSetter = null;
            controlGroup = null;
        }

        [TestCase(PIInterpolationOptionEnumStringType.BLOCK, InterpolationType.Constant, PIExtrapolationOptionEnumStringType.BLOCK, ExtrapolationType.Constant)]
        [TestCase(PIInterpolationOptionEnumStringType.LINEAR, InterpolationType.Linear, PIExtrapolationOptionEnumStringType.PERIODIC, ExtrapolationType.Periodic)]
        public void GivenATimeRuleElementWithInterpolationAndExtrapolationAndAMatchingRuleInControlGroup_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_CorrectInterpolationAndExtrapolationIsSetOnRule(
            PIInterpolationOptionEnumStringType interpolationType, InterpolationType expectedInterpolation,
            PIExtrapolationOptionEnumStringType extrapolationType, ExtrapolationType expectedExtrapolation)
        {
            // Given
            var timeSeriesElement = CreateTimeSeriesElement(RtcXmlTag.TimeRule, interpolationType, extrapolationType);
            var rule = new TimeRule(ComponentName);
            controlGroup.Rules.Add(rule);

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedInterpolation, rule.InterpolationOptionsTime, 
                $"Interpolation for time rule was expected to be '{expectedInterpolation.ToString()}'");
            Assert.AreEqual(expectedExtrapolation, rule.Periodicity, 
                $"Extrapolation for time rule was expected to be '{expectedExtrapolation.ToString()}'");
        }

        [Test]
        public void GivenANullParameterAsElements_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_ThenNothingHappensAndMethodIsReturned()
        {
            Assert.DoesNotThrow(
                () => dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                    null,
                    new[] {controlGroup}),
                "Method throws an unexpected exception when parameter 'elements' is null.");

            Assert.AreEqual(0, logHandler.LogMessagesTable.Count, 
                AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [Test]
        public void GivenANullParameterAsControlGroups_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_ThenNothingHappensAndMethodIsReturned()
        {
            Assert.DoesNotThrow(
                () => dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                    new[] {new PITimeSeriesXML()},
                    null),
                "Method throws an unexpected exception when parameter 'controlGroups' is null.");

            Assert.AreEqual(0, logHandler.LogMessagesTable.Count,
                AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [TestCase(PIInterpolationOptionEnumStringType.BLOCK, InterpolationType.Constant)]
        [TestCase(PIInterpolationOptionEnumStringType.LINEAR, InterpolationType.Linear)]
        public void GivenARelativeTimeRuleElementWithInterpolationAndExtrapolationAndAMatchingRuleInControlGroup_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_CorrectInterpolationAndExtrapolationIsSetOnRule(
            PIInterpolationOptionEnumStringType interpolationType, InterpolationType expectedInterpolation)
        {
            // Given
            var timeSeriesElement = CreateTimeSeriesElement(RtcXmlTag.RelativeTimeRule, interpolationType);
            var rule = new RelativeTimeRule {Name = ComponentName};
            controlGroup.Rules.Add(rule);

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedInterpolation, rule.Interpolation, 
                $"Interpolation for relative time rule was expected to be '{expectedInterpolation.ToString()}'");
        }

        [TestCase(PIInterpolationOptionEnumStringType.BLOCK, InterpolationType.Constant, PIExtrapolationOptionEnumStringType.BLOCK, ExtrapolationType.Constant)]
        [TestCase(PIInterpolationOptionEnumStringType.LINEAR, InterpolationType.Linear, PIExtrapolationOptionEnumStringType.PERIODIC, ExtrapolationType.Periodic)]
        public void GivenAPidRuleElementWithInterpolationAndExtrapolationAndAMatchingRuleInControlGroup_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_CorrectInterpolationAndExtrapolationIsSetOnRule(
            PIInterpolationOptionEnumStringType interpolationType, InterpolationType expectedInterpolation,
            PIExtrapolationOptionEnumStringType extrapolationType, ExtrapolationType expectedExtrapolation)
        {
            // Given
            var timeSeriesElement = CreateTimeSeriesElement(RtcXmlTag.PIDRule, interpolationType, extrapolationType);
            var rule = new PIDRule(ComponentName);
            controlGroup.Rules.Add(rule);

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedInterpolation, rule.InterpolationOptionsTime, 
                $"Interpolation for pid rule was expected to be '{expectedInterpolation.ToString()}'");
            Assert.AreEqual(expectedExtrapolation, rule.ExtrapolationOptionsTime, 
                $"Extrapolation for pid rule was expected to be '{expectedExtrapolation.ToString()}'");
        }

        [TestCase(PIInterpolationOptionEnumStringType.BLOCK, InterpolationType.Constant, PIExtrapolationOptionEnumStringType.BLOCK, ExtrapolationType.Constant)]
        [TestCase(PIInterpolationOptionEnumStringType.LINEAR, InterpolationType.Linear, PIExtrapolationOptionEnumStringType.PERIODIC, ExtrapolationType.Periodic)]
        public void GivenAnIntervalRuleElementWithInterpolationAndExtrapolationAndAMatchingRuleInControlGroup_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_CorrectInterpolationAndExtrapolationIsSetOnRule(
            PIInterpolationOptionEnumStringType interpolationType, InterpolationType expectedInterpolation,
            PIExtrapolationOptionEnumStringType extrapolationType, ExtrapolationType expectedExtrapolation)
        {
            // Given
            var timeSeriesElement = CreateTimeSeriesElement(RtcXmlTag.IntervalRule, interpolationType, extrapolationType);
            var rule = new IntervalRule(ComponentName);
            controlGroup.Rules.Add(rule);

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedInterpolation, rule.InterpolationOptionsTime, 
                $"Interpolation for interval rule was expected to be '{expectedInterpolation.ToString()}'");
            Assert.AreEqual(expectedExtrapolation, rule.Extrapolation, 
                $"Extrapolation for interval rule was expected to be '{expectedExtrapolation.ToString()}'");
        }

        [TestCase(PIInterpolationOptionEnumStringType.BLOCK, InterpolationType.Constant, PIExtrapolationOptionEnumStringType.BLOCK, ExtrapolationType.Constant)]
        [TestCase(PIInterpolationOptionEnumStringType.LINEAR, InterpolationType.Linear, PIExtrapolationOptionEnumStringType.PERIODIC, ExtrapolationType.Periodic)]
        public void GivenATimeConditionElementWithInterpolationAndExtrapolationAndAMatchingConditionInControlGroup_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_CorrectInterpolationAndExtrapolationIsSetOnCondition(
            PIInterpolationOptionEnumStringType interpolationType, InterpolationType expectedInterpolation,
            PIExtrapolationOptionEnumStringType extrapolationType, ExtrapolationType expectedExtrapolation)
        {
            // Given
            var timeSeriesElement = CreateTimeSeriesElement(RtcXmlTag.TimeCondition, interpolationType, extrapolationType);
            var condition = new TimeCondition {Name = ComponentName};
            controlGroup.Conditions.Add(condition);

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(expectedInterpolation, condition.InterpolationOptionsTime, 
                $"Interpolation for time condition was expected to be '{expectedInterpolation.ToString()}'");
            Assert.AreEqual(expectedExtrapolation, condition.Extrapolation, 
                $"Extrapolation for time condition was expected to be '{expectedExtrapolation.ToString()}'");
        }

        [TestCase(RtcXmlTag.Input, PIInterpolationOptionEnumStringType.BLOCK, PIExtrapolationOptionEnumStringType.BLOCK)]
        [TestCase(RtcXmlTag.SP, PIInterpolationOptionEnumStringType.LINEAR, PIExtrapolationOptionEnumStringType.PERIODIC)]
        public void GivenAnElementWithAnIdWithNoRtcComponentTag_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_TheElementIsSkippedAndRuleHasDefaultInterpolationAndExtrapolation(string tag, PIInterpolationOptionEnumStringType interpolationType, PIExtrapolationOptionEnumStringType extrapolationType)
        {
            // Given
            var timeSeriesElement = new PITimeSeriesXML
            {
                locationId = tag + ControlGroupName + "/" + ComponentName,
                interpolationOption = interpolationType,
                extrapolationOption = extrapolationType
            };
            var rule = new TimeRule(ComponentName);
            controlGroup.Rules.Add(rule);

            var defaultInterpolation = rule.InterpolationOptionsTime;
            var defaultExtrapolation = rule.Periodicity;

            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(
                new[] {timeSeriesElement},
                new[] {controlGroup});

            // Then
            Assert.AreEqual(defaultInterpolation, rule.InterpolationOptionsTime, 
                $"Interpolation for rule was expected to be '{defaultInterpolation.ToString()}'");
            Assert.AreEqual(defaultExtrapolation, rule.Periodicity, 
                $"Extrapolation for rule was expected to be '{defaultExtrapolation.ToString()}'");
        }

        [Test]
        public void GivenNullGivenAsParameters_WhenSetInterpolationAndExtrapolationRtcComponentsIsCalled_NothingHappens()
        {
            // When
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(null, null);

            // Then
            Assert.AreEqual(0, logHandler.LogMessagesTable.Count, 
                AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [TestCase(-2, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 3600)]
        [TestCase(1, 7200)]
        [TestCase(2, 10800)]
        public void GivenAnHydraulicRuleAndAMatchingElement_WhenSetTimeLagOnHydraulicRulesIsCalled_CorrectTimeLagIsSetOnHydraulicRule(int vectorLength, int expectedTimeLag)
        {
            // Given
            var timeSeriesElement = new RTCTimeSeriesXML
            {
                id = RtcXmlTag.Delayed + InputName,
                vectorLength = vectorLength
            };

            var hydraulicRule = new HydraulicRule {Inputs = {new Input {Name = InputName}}};

            // When
            dataConfigSetter.SetTimeLagOnHydraulicRules(
                new[] {timeSeriesElement},
                new[] {hydraulicRule},
                timeStep);

            // Then
            Assert.AreEqual(expectedTimeLag, hydraulicRule.TimeLag,
                $"Expected time lag for hydraulic rule was expected to be {expectedTimeLag}.");
        }

        [Test]
        public void GivenAnHydraulicRuleWithoutInputs_WhenSetTimeLagOnHydraulicRulesIsCalled_ThenExpectedLogMessageIsGiven()
        {
            // Given
            var timeSeriesElement = new RTCTimeSeriesXML();
            var hydraulicRule = new HydraulicRule();

            var expectedMessage = string.Format(
                Resources.RealTimeControlDataConfigXmlSetter_SetTimeLagOnHydraulicRules_Hydraulic_rule___0___must_have_an_input__Please_check_file____1___,
                hydraulicRule.Name, RealTimeControlXMLFiles.XmlTools);

            // When
            dataConfigSetter.SetTimeLagOnHydraulicRules(
                new[] {timeSeriesElement},
                new[] {hydraulicRule},
                timeStep);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage), 
                AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            Assert.AreEqual(0, hydraulicRule.TimeLag, 
                "Expected time lag for hydraulic rule was expected to be 0.");
        }

        [Test]
        public void GivenAnHydraulicRuleAndWithoutAMatchingInputElement_WhenSetTimeLagOnHydraulicRulesIsCalled_ThenTimeLagIsUnchanged()
        {
            // Given
            var timeSeriesElement = new RTCTimeSeriesXML
            {
                vectorLength = 2
            };
            var hydraulicRule = new HydraulicRule
            {
                Inputs = {new Input {Name = InputName}},
                TimeLag = 0
            };
           
            // When
            dataConfigSetter.SetTimeLagOnHydraulicRules(
                new[] {timeSeriesElement},
                new[] {hydraulicRule},
                timeStep);

            // Then
            Assert.That(hydraulicRule.TimeLag, Is.EqualTo(0)); // Time lag of hydraulic rule is unchanged (0)
        }

        [Test]
        public void GivenANullParameterAsElements_WhenSetTimeLagOnHydraulicRulesIsCalled_ThenNothingHappensAndMethodIsReturned()
        {
            Assert.DoesNotThrow(
                () => dataConfigSetter.SetTimeLagOnHydraulicRules(
                    null,
                    new[] {new HydraulicRule()},
                    new TimeSpan()),
                "Method throws an unexpected exception when parameter 'elements' is null");

            Assert.AreEqual(0, logHandler.LogMessagesTable.Count,
                AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [Test]
        public void GivenANullParameterAsControlGroups_WhenSetTimeLagOnHydraulicRulesIsCalled_ThenNothingHappensAndMethodIsReturned()
        {
            Assert.DoesNotThrow(
                () => dataConfigSetter.SetTimeLagOnHydraulicRules(
                    new[] {new RTCTimeSeriesXML()},
                    null,
                    timeStep),
                "Method throws an unexpected exception when parameter 'controlGroups' is null.");

            Assert.AreEqual(0, logHandler.LogMessagesTable.Count,
                AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        private static PITimeSeriesXML CreateTimeSeriesElement(
            string tag,
            PIInterpolationOptionEnumStringType interpolationType = PIInterpolationOptionEnumStringType.LINEAR,
            PIExtrapolationOptionEnumStringType extrapolationType = PIExtrapolationOptionEnumStringType.BLOCK)
        {
            var timeSeriesElement = new PITimeSeriesXML
            {
                locationId = tag + ControlGroupName + "/" + ComponentName,
                interpolationOption = interpolationType,
                extrapolationOption = extrapolationType
            };

            return timeSeriesElement;
        }
    }
}