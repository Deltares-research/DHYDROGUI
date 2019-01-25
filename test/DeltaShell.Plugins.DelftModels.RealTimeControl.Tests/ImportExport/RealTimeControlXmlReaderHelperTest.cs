using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlXmlReaderHelperTest
    {
        [TestCase("[TimeCondition]control_group_name/time_condition_name", "time_condition_name")]
        [TestCase("[TimeRule]control_group_name/time_rule_name", "time_rule_name")]
        [TestCase("control_group_name/time_rule_name", "time_rule_name")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetRuleOrConditionNameFromElementIdIsCalled_ThenExpectedStringIsReturned(string id, string expectedString)
        {
            // When
            var resultedString = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

            // Then
            Assert.AreEqual(expectedString, resultedString);
        }

        [TestCase("[TimeCondition]control_group_name/time_condition_name", "[TimeCondition]", "control_group_name")]
        [TestCase("[TimeRule]control_group_name/time_rule_name", "[TimeRule]", "control_group_name")]
        [TestCase("[TimeRule]control_group_name/time_rule_name", null, "control_group_name")]
        [TestCase("[Input]parameter/quantity", "[Input]", null)]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetControlGroupNameFromElementIdIsCalled_ThenExpectedStringIsReturned(string id, string tag, string expectedString)
        {
            // When
            var resultedString = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id, tag);

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
            var expectedControlGroup = new ControlGroup { Name = "control_group_name" };
            var controlgroups =
                new List<IControlGroup> { new ControlGroup { Name = "extra_control_group" }, expectedControlGroup };

            // When
            var resultedControlGroup = controlgroups.GetControlGroupByElementId(id);

            // Then
            Assert.AreEqual(expectedControlGroup, resultedControlGroup);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElementAndControlGroupsIsNull_WhenGetControlGroupByElementIdIsCalled_ThenNullIsReturned()
        {
            // Given
            const string id = "[TimeRule]control_group_name/time_rule_name";

            IList<IControlGroup> controlGroups = null;
            // When
            var controlGroup = controlGroups.GetControlGroupByElementId(id);

            // Then
            Assert.IsNull(controlGroup);
        }

        [Test]
        public void GivenAnIdFromAnRtcXmlElementWithAControlGroupNameThatDoesNotExist_WhenGetControlGroupByElementIdIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingControlGroupName = "not_existing_control_group_name";
            var id = $"[TimeRule]{missingControlGroupName}/time_rule_name";
            var controlgroups = new List<IControlGroup>
            {
                new ControlGroup { Name = "extra_control_group" },
                new ControlGroup { Name = "control_group_name" }
            };

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    var resultedControlGroup = controlgroups.GetControlGroupByElementId(id);

                    // Then
                    Assert.AreEqual(null, resultedControlGroup);
                },
                string.Format(Resources.RealTimeControlXmlReaderHelper_GetControlGroupByElementId_Could_not_find_the_controlgroup___0___that_is_referenced_in_id___1____The_group_needs_to_be_referenced_in_file___2___, missingControlGroupName, id, RealTimeControlXMLFiles.XmlTools));
        }

        [Test]
        public void GivenAConnectionPointName_WhenGetConnectionPointByNameIsCalled_ThenExpectedConnectionPointIsReturned()
        {
            // Given
            const string inputName = "[Input]parameter/quantity";
            const string outputName = "[Output]parameter/quantity";
            var expectedInput = new Input { Name = inputName };
            var expectedOutput = new Output { Name = outputName };

            var connectionPoints = new List<ConnectionPoint>
            {
                expectedInput,
                expectedOutput,
            };

            // When
            var resultedInput = connectionPoints.GetByName<Input>(inputName);
            var resultedOutput = connectionPoints.GetByName<Output>(outputName);

            // Then
            Assert.AreEqual(expectedInput, resultedInput);
            Assert.AreEqual(expectedOutput, resultedOutput);
        }

        [Test]
        public void GivenAnInputNameAndInputsIsNull_WhenGetByNameIsCalled_ThenNullIsReturnedAndNothingHappens()
        {
            // Given
            const string name = "[Input]parameter/quantity";
            IEnumerable<ConnectionPoint> connectionPoints = null;

            // When
            var input = connectionPoints.GetByName<Input>(name);

            // Then
            Assert.IsNull(input);
        }

        [Test]
        public void GivenAConnectionPointNameAndConnectionPointsIsNull_WhenGetConnectionPointByNameIsCalled_ThenNullIsReturnedAndNothingHappens()
        {
            // Given
            const string name = "[Input]parameter/quantity";
            IEnumerable<Input> inputs = null;

            // When
            var input = inputs.GetByName<Input>(name);

            // Then
            Assert.IsNull(input);
        }


        [Test]
        public void GivenAConnectionPointNameThatDoesNotExist_WhenGetConnectionPointByNameIsCalled_ThenNullIsReturnedAndExpectedErrorMessageIsGiven()
        {
            // Given
            const string missingOutputName = "not_existing_connection_point_name";

            var outputs = new List<Output> { new Output { Name = "some_name" } };

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    var resultedControlGroup = outputs.GetByName<Output>(missingOutputName);

                    // Then
                    Assert.AreEqual(null, resultedControlGroup);
                },
                string.Format(Resources.RealTimeControlXmlReaderHelper_GetConnectionPointByName_Could_not_find_the_input_output___0____The_input_output_needs_to_be_referenced_in_file___1___, missingOutputName, RealTimeControlXMLFiles.XmlData));
        }

        [TestCase("[TimeRule]control_group_name/time_rule_name", "time_rule_name", "[RelativeTimeRule]control_group_name/relative_time_rule_name", "relative_time_rule_name")]
        [TestCase("[]control_group_name/time_rule_name", "time_rule_name", "[]control_group_name/relative_time_rule_name", "relative_time_rule_name")]
        [TestCase("control_group_name/time_rule_name", "time_rule_name", "control_group_name/relative_time_rule_name", "relative_time_rule_name")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetRuleByElementIdInControlGroupIsCalled_ThenExpectedRuleIsReturned(string timeRuleId, string timeRuleName, string standardRuleId, string standardRuleName)
        {
            // Given
            var expectedTimeRule = new TimeRule { Name = timeRuleName };
            var expectedRelativeTimeRule = new RelativeTimeRule() { Name = standardRuleName };

            var controlGroup = new ControlGroup();
            controlGroup.Rules.AddRange(new List<RuleBase> { expectedTimeRule, expectedRelativeTimeRule });

            // When
            var resultedRule1 = controlGroup.GetRuleByElementId(timeRuleId);
            var resultedRule2 = controlGroup.GetRuleByElementId(standardRuleId);

            // Then
            Assert.AreEqual(expectedTimeRule, resultedRule1);
            Assert.AreEqual(expectedRelativeTimeRule, resultedRule2);
        }

        [Test]
        public void GivenAnIdFromAnRuleXmlElementAndControlGroupIsNull_WhenGetRuleByElementIdInControlGroupIsCalled_ThenNullIsReturned()
        {
            // Given
            const string id = "[TimeRule]control_group_name/time_rule_name";
            ControlGroup controlgroup = null;
            // When
            var rule = controlgroup.GetRuleByElementId(id);

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

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    var resultedRule = controlGroup.GetRuleByElementId(id);

                    // Then
                    Assert.AreEqual(null, resultedRule);
                },
                string.Format(Resources.RealTimeControlXmlReaderHelper_GetRuleByElementIdInControlGroup_Could_not_find_the_rule___0___that_is_referenced_in_id___1___The_rule_needs_to_be_referenced_in_file___2___, missingRuleName, id, RealTimeControlXMLFiles.XmlData));
        }

        [TestCase("[TimeCondition]control_group_name/time_condition_name", "time_condition_name", "[StandardCondition]control_group_name/relative_time_condition_name", "relative_time_condition_name")]
        [TestCase("[]control_group_name/time_condition_name", "time_condition_name", "[]control_group_name/relative_time_condition_name", "relative_time_condition_name")]
        [TestCase("control_group_name/time_condition_name", "time_condition_name", "control_group_name/relative_time_condition_name", "relative_time_condition_name")]
        public void GivenAnIdFromAnRtcXmlElement_WhenGetConditionByElementIdInControlGroupIsCalled_ThenExpectedConditionIsReturned(string timeConditionId, string timeConditionName, string standardConditionId, string standardConditionName)
        {
            // Given
            var expectedTimeCondition = new TimeCondition { Name = timeConditionName };
            var expectedStandardCondition = new StandardCondition { Name = standardConditionName };

            var controlGroup = new ControlGroup();
            controlGroup.Conditions.AddRange(new List<ConditionBase> { expectedTimeCondition, expectedStandardCondition });

            // When
            var resultedCondition1 = controlGroup.GetConditionByElementId<TimeCondition>(timeConditionId);
            var resultedCondition2 = controlGroup.GetConditionByElementId<StandardCondition>(standardConditionId);

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
            var condition = controlGroup.GetConditionByElementId<TimeCondition>(id);

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
            controlGroup.Conditions.AddRange(new List<ConditionBase>
            {
                new StandardCondition {Name = "some_name"},
                new TimeCondition {Name = "some_other_name"}
            });

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    var resultedCondition = controlGroup.GetConditionByElementId<TimeCondition>(id);

                    // Then
                    Assert.AreEqual(null, resultedCondition);
                },
                string.Format(Resources.RealTimeControlXmlReaderHelper_GetConditionByElementIdInControlGroup_Could_not_find_the_condition___0____The_condition_needs_to_be_referenced_in_file___1___, missingConditionName, RealTimeControlXMLFiles.XmlData));
        }
    }
}
