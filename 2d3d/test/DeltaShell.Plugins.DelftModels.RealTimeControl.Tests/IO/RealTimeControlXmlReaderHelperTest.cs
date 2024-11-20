using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlXmlReaderHelperTest
    {
        private ILogHandler logHandler;

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElementAndControlGroupsIsNull_WhenGetControlGroupByElementIdIsCalled_ThenNullIsReturned()
        {
            // Given
            const string id = "[TimeRule]control_group_name/time_rule_name";

            // When
            IControlGroup controlGroup = ((IList<IControlGroup>) null).GetControlGroupByElementId(id, null);

            // Then
            Assert.IsNull(controlGroup);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElementWithAControlGroupNameThatDoesNotExist_WhenGetControlGroupByElementIdIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingControlGroupName = "not_existing_control_group_name";
            var id = $"[TimeRule]{missingControlGroupName}/time_rule_name";

            string expectedMessage = string.Format(Resources.RealTimeControlXmlReaderHelper_GetControlGroupByElementId_Could_not_find_the_controlgroup___0___that_is_referenced_in_id___1____The_group_needs_to_be_referenced_in_file___2___,
                                                   missingControlGroupName, id, RealTimeControlXmlFiles.XmlTools);

            // When
            IControlGroup resultedControlGroup = new List<IControlGroup>().GetControlGroupByElementId(id, logHandler);

            // Then
            Assert.AreEqual(null, resultedControlGroup);
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage), "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAnInputName_WhenGetConnectionPointByNameIsCalled_ThenExpectedInputIsReturned()
        {
            // Given
            const string inputName = "[Input]parameter/quantity";
            var expectedInput = new Input {Name = inputName};

            var connectionPoints = new List<ConnectionPoint> {expectedInput};

            // When
            var resultedInput = connectionPoints.GetByName<Input>(inputName, null);

            // Then
            Assert.AreEqual(expectedInput, resultedInput);
        }

        [Test]
        public void GivenAnOutputName_WhenGetConnectionPointByNameIsCalled_ThenExpectedOutputIsReturned()
        {
            // Given
            const string outputName = "[Output]parameter/quantity";
            var expectedOutput = new Output {Name = outputName};
            var connectionPoints = new List<ConnectionPoint> {expectedOutput};

            // When
            var resultedOutput = connectionPoints.GetByName<Output>(outputName, null);

            // Then
            Assert.AreEqual(expectedOutput, resultedOutput);
        }

        [Test]
        public void GivenAnInputNameAndInputsIsNull_WhenGetByNameIsCalled_ThenNullIsReturnedAndNothingHappens()
        {
            // Given
            const string name = "[Input]parameter/quantity";

            // When
            var input = ((IEnumerable<ConnectionPoint>) null).GetByName<Input>(name, null);

            // Then
            Assert.IsNull(input);
        }

        [Test]
        public void GivenAConnectionPointNameThatDoesNotExist_WhenGetConnectionPointByNameIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingConnectionPointName = "not_existing_connection_point_name";

            string expectedMessage = string.Format(
                Resources.RealTimeControlXmlReaderHelper_GetConnectionPointByName_Could_not_find_the_input_output___0____The_input_output_needs_to_be_referenced_in_file___1___,
                missingConnectionPointName,
                RealTimeControlXmlFiles.XmlData);

            // When
            var resultedControlGroup = new List<ConnectionPoint>().GetByName<Output>(missingConnectionPointName, logHandler);

            // Then
            Assert.IsNull(resultedControlGroup);
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetRuleByElementIdInControlGroupIsCalled_ThenExpectedRuleIsReturned()
        {
            // Given
            const string timeRuleId = "[TimeRule]control_group_name/time_rule_name";
            const string timeRuleName = "time_rule_name";
            const string relativeTimeRuleId = "[RelativeTimeRule]control_group_name/relative_time_rule_name";
            const string relativeTimeRuleName = "relative_time_rule_name";

            var expectedTimeRule = new TimeRule {Name = timeRuleName};
            var expectedRelativeTimeRule = new RelativeTimeRule {Name = relativeTimeRuleName};

            var controlGroup = new ControlGroup();
            controlGroup.Rules.AddRange(new List<RuleBase>
            {
                expectedTimeRule,
                expectedRelativeTimeRule
            });

            // When
            RuleBase resultedRule1 = controlGroup.GetRuleByElementId(timeRuleId, null);
            RuleBase resultedRule2 = controlGroup.GetRuleByElementId(relativeTimeRuleId, null);

            // Then
            Assert.AreEqual(expectedTimeRule, resultedRule1);
            Assert.AreEqual(expectedRelativeTimeRule, resultedRule2);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElement_WhenGenericGetRuleByElementIdInControlGroupIsCalled_ThenExpectedRuleIsReturned()
        {
            // Given
            const string timeRuleId = "[TimeRule]control_group_name/time_rule_name";
            const string timeRuleName = "time_rule_name";
            const string relativeTimeRuleId = "[RelativeTimeRule]control_group_name/relative_time_rule_name";
            const string relativeTimeRuleName = "relative_time_rule_name";

            var expectedTimeRule = new TimeRule {Name = timeRuleName};
            var expectedRelativeTimeRule = new RelativeTimeRule {Name = relativeTimeRuleName};

            var controlGroup = new ControlGroup();
            controlGroup.Rules.AddRange(new List<RuleBase>
            {
                expectedTimeRule,
                expectedRelativeTimeRule
            });

            // When
            RuleBase resultedRule1 = controlGroup.GetRuleByElementId<TimeRule>(timeRuleId, null);
            RuleBase resultedRule2 = controlGroup.GetRuleByElementId<RelativeTimeRule>(relativeTimeRuleId, null);

            // Then
            Assert.AreEqual(expectedTimeRule, resultedRule1);
            Assert.AreEqual(expectedRelativeTimeRule, resultedRule2);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElement_WhenGenericGetRuleByElementIdInControlGroupIsCalledWithTheWrongType_ThenNullIsReturnedAndExpectedMessageIsLogged()
        {
            // Given
            const string timeRuleId = "[TimeRule]control_group_name/time_rule_name";
            const string timeRuleName = "time_rule_name";

            var expectedTimeRule = new TimeRule {Name = timeRuleName};

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(expectedTimeRule);

            string expectedMessage = string.Format(
                Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___,
                timeRuleName, timeRuleId, RealTimeControlXmlFiles.XmlData);

            // When
            RuleBase rule = controlGroup.GetRuleByElementId<RelativeTimeRule>(timeRuleId, logHandler);

            // Then
            Assert.IsNull(rule);
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Any(m => m.Contains(expectedMessage)),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElementAndNullAsControlGroup_WhenGenericGetRuleByElementIdInControlGroupIsCalled_ThenNullIsReturnedAndExpectedMessageIsLogged()
        {
            // Given
            const string timeRuleId = "[TimeRule]control_group_name/time_rule_name";

            // When
            RuleBase rule = ((IControlGroup) null).GetRuleByElementId<RelativeTimeRule>(timeRuleId, null);

            // Then
            Assert.IsNull(rule);
        }

        [Test]
        public void GivenAnIdFromAnRuleXmlElementAndControlGroupIsNull_WhenGetRuleByElementIdInControlGroupIsCalled_ThenNullIsReturned()
        {
            // Given
            const string id = "[TimeRule]control_group_name/time_rule_name";

            // When
            RuleBase rule = ((ControlGroup) null).GetRuleByElementId(id, null);

            // Then
            Assert.IsNull(rule);
        }

        [Test]
        public void GivenAnIdFromARuleXmlElementWithARuleThatDoesNotExist_WhenGetRuleByElementIdInControlGroupIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingRuleName = "not_existing_rule_name";
            var id = $"[TimeRule]control_group_name/{missingRuleName}";

            var controlGroup = new ControlGroup();
            controlGroup.Rules.AddRange(new List<RuleBase>
            {
                new RelativeTimeRule {Name = "some_name"},
                new TimeRule {Name = "some_other_name"}
            });

            string expectedMessage = string.Format(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___,
                                                   missingRuleName, id, RealTimeControlXmlFiles.XmlData);

            // When
            RuleBase resultedRule = controlGroup.GetRuleByElementId(id, logHandler);

            // Then
            Assert.AreEqual(null, resultedRule);
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetConditionByElementIdInControlGroupIsCalled_ThenExpectedConditionIsReturned()
        {
            // Given
            const string timeConditionId = "[TimeCondition]control_group_name/time_condition_name";
            const string timeConditionName = "time_condition_name";
            const string standardConditionId = "[StandardCondition]control_group_name/relative_time_condition_name";
            const string standardConditionName = "relative_time_condition_name";

            var expectedTimeCondition = new TimeCondition {Name = timeConditionName};
            var expectedStandardCondition = new StandardCondition {Name = standardConditionName};

            var controlGroup = new ControlGroup();
            controlGroup.Conditions.AddRange(new List<ConditionBase>
            {
                expectedTimeCondition,
                expectedStandardCondition
            });

            // When
            var resultedCondition1 = controlGroup.GetConditionByElementId<TimeCondition>(timeConditionId, null);
            var resultedCondition2 = controlGroup.GetConditionByElementId<StandardCondition>(standardConditionId, null);

            // Then
            Assert.AreEqual(expectedTimeCondition, resultedCondition1);
            Assert.AreEqual(expectedStandardCondition, resultedCondition2);
        }

        [Test]
        public void GivenAnIdFromAnConditionXmlElementAndControlGroupIsNull_WhenGetConditionByElementIdInControlGroupIsCalled_ThenNullIsReturnedAndNothingHappens()
        {
            // Given
            const string id = "[TimeCondition]control_group_name/time_condition_name";
            ControlGroup controlGroup = null;

            // When
            var condition = controlGroup.GetConditionByElementId<TimeCondition>(id, null);

            // Then
            Assert.IsNull(condition);
        }

        [Test]
        public void GivenAnIdFromAConditionXmlElementWithAConditionThatDoesNotExist_WhenGetConditionByElementIdInControlGroupIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingConditionName = "not_existing_condition_name";
            var id = $"[TimeCondition]control_group_name/{missingConditionName}";
            var controlGroup = new ControlGroup();

            string expectedMessage = string.Format(
                Resources
                    .RealTimeControlXmlReaderHelper_GetConditionByElementIdInControlGroup_Could_not_find_the_condition___0____The_condition_needs_to_be_referenced_in_file___1___,
                missingConditionName, RealTimeControlXmlFiles.XmlData);

            // When
            var resultedCondition = controlGroup.GetConditionByElementId<TimeCondition>(id, logHandler);

            // Then
            Assert.AreEqual(null, resultedCondition);
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenAnIdWithTagsInIt_WhenGetTagFromElementIsCalled_ThenTheCorrectTagIsReturned()
        {
            IEnumerable<string> tagsOfInterest = RtcXmlTag.ConnectionPointTags.Concat(RtcXmlTag.ComponentTags);
            foreach (string tag in tagsOfInterest)
            {
                // Given
                var id = $"[Status]{tag}[Delayed]";

                // When
                string resultedTag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                // Then
                Assert.AreEqual(tag, resultedTag);
            }
        }

        [Test]
        public void Given_WhenGetTagFromElementIsCalled_ThenTheCorrectTagIsReturned()
        {
            // Given
            const string id = "[Status][Delayed]";

            // When
            string resultedTag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

            // Then
            Assert.AreEqual(null, resultedTag);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetSignalByElementIdInControlGroupIsCalled_ThenExpectedSignalIsReturned()
        {
            // Given
            const string signalId = "[LookupSignal]control_group_name/signal_name";
            const string signalName = "signal_name";

            var expectedSignal = new LookupSignal() {Name = signalName};

            var controlGroup = new ControlGroup();
            controlGroup.Signals.AddRange(new List<SignalBase> {expectedSignal});

            // When
            SignalBase resultedSignal = controlGroup.GetSignalByElementId<LookupSignal>(signalId, null);

            // Then
            Assert.AreEqual(expectedSignal, resultedSignal);
        }

        [TestCase("[TimeCondition]control_group_name/time_condition_name", "time_condition_name")]
        [TestCase("[TimeRule]control_group_name/time_rule_name", "time_rule_name")]
        [TestCase("control_group_name/time_rule_name", "time_rule_name")]
        [TestCase("[Input]parameter_name/quantity", null)]
        [TestCase("", "")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetRuleOrConditionNameFromElementIdIsCalled_ThenExpectedStringIsReturned(string id, string expectedString)
        {
            // When
            string resultedString = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(id);

            // Then
            Assert.AreEqual(expectedString, resultedString);
        }

        [TestCase("[TimeCondition]control_group_name/time_condition_name", "control_group_name")]
        [TestCase("[TimeRule]control_group_name/time_rule_name", "control_group_name")]
        [TestCase("control_group_name/time_rule_name", "control_group_name")]
        [TestCase("[tag]control_group_name/other_name", "control_group_name")]
        [TestCase("", "")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetControlGroupNameFromElementIdIsCalled_ThenExpectedStringIsReturned(string id, string expectedString)
        {
            // When
            string resultedString = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);

            // Then
            Assert.AreEqual(expectedString, resultedString);
        }

        [TestCase("[TimeRule]control_group_name/time_rule_name")]
        [TestCase("[SomeTag]control_group_name/some_name")]
        [TestCase("[]control_group_name/some_name")]
        [TestCase("control_group_name/some_name")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetControlGroupByElementIdIsCalled_ThenExpectedControlGroupIsReturned(string id)
        {
            // Given
            var expectedControlGroup = new ControlGroup {Name = "control_group_name"};
            var controlgroups = new List<IControlGroup> {expectedControlGroup};

            // When
            IControlGroup resultedControlGroup = controlgroups.GetControlGroupByElementId(id, null);

            // Then
            Assert.AreEqual(expectedControlGroup, resultedControlGroup);
        }
    }
}