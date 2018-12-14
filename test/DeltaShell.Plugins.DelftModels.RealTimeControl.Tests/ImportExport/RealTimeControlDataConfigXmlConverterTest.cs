using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlDataConfigXmlConverterTest
    {
        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsNull_WhenCreateControlGroupsFromXmlElementIDsIsCalled_ThenNullIsReturned()
        {
            // Given
            // When
            var resultedControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(null);

            // Then
            Assert.IsNull(resultedControlGroups);
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsEmpty_WhenCreateControlGroupsFromXmlElementIDsIsCalled_ThenNullIsReturned()
        {
            // Given
            var elements = new List<RTCTimeSeriesXML>();

            // When
            var resultedControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(elements);

            // Then
            Assert.IsNull(resultedControlGroups);
        }

        [TestCase("[RelativeTimeRule]")]
        [TestCase("[TimeRule]")]
        [TestCase("[StandardCondition]")]
        [TestCase("[TimeCondition]")]
        public void GivenAListOfRtcTimeSeriesXml_WhenCreateControlGroupsFromXmlElementIDsIsCalled_ThenAListOfControlGroupsIsReturned(string tag)
        {
            // Given
            const string groupName = "control_group";
            var elements = new List<RTCTimeSeriesXML>
            {
                new RTCTimeSeriesXML {id = $"{tag}{groupName}/some_rule_or_condition_name"}
            };

            // When
            var resultedControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(elements);

            // Then
            Assert.NotNull(resultedControlGroups);
            Assert.AreEqual(1, resultedControlGroups.Count);
            Assert.AreEqual(groupName, resultedControlGroups[0].Name);
        }

        [TestCase("[Input]")]
        [TestCase("[Output]")]
        public void GivenAListOfRtcTimeSeriesXmlOfConnectionPoints_WhenCreateControlGroupsFromXmlElementIDsIsCalled_ThenElementIsIgnored(string tag)
        {
            // Given
            const string groupName = "control_group";
            var elements = new List<RTCTimeSeriesXML>
            {
                new RTCTimeSeriesXML {id = $"{tag}{groupName}/some_name]"}
            };

            // When
            var resultedControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(elements);

            // Then
            Assert.NotNull(resultedControlGroups);
            Assert.IsEmpty(resultedControlGroups);
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXmlWithDuplicateGroupNames_WhenCreateControlGroupsFromXmlElementIDsIsCalled_ThenElementIsIgnored()
        {
            // Given
            const string groupName = "control_group";
            var elements = new List<RTCTimeSeriesXML>
            {
                new RTCTimeSeriesXML {id = $"[TimeCondition]{groupName}/some_rule_or_condition_name"},
                new RTCTimeSeriesXML {id = $"[RelativeTimeRule]{groupName}/some_rule_or_condition_name"}
            };

            // When
            var resultedControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(elements);

            // Then
            Assert.NotNull(resultedControlGroups);
            Assert.AreEqual(1, resultedControlGroups.Count);
            Assert.AreEqual(groupName, resultedControlGroups[0].Name);
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsNull_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_ThenNoRuleIsSetOnControlGroupAndNoExceptionIsThrown()
        {
            // Given
            var controlGroups = new List<ControlGroup>();

            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(null, controlGroups);
            });
            
            // Then
            Assert.IsEmpty(controlGroups.SelectMany(c=>c.Rules));
        }

        [Test]
        public void GivenAListOfControlGroupsIsNull_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_ThenNoExceptionIsThrown()
        {
            // Given
            var elements = new List<RTCTimeSeriesXML>();

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(elements, null);
            });
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXml_WhenCreateRulesFromXmlElementsAndAddToControlGroupIsCalled_ThenTheCorrectRuleIsCreatedAndAddedToControlGroup()
        {
            // Given
            const string groupName1 = "control_group_1";
            const string groupName2 = "control_group_2";

            const string timeRuleName = "time_rule";
            const string relativeTimeRuleName = "relative_time_rule";
            const string extraTimeRuleName = "extra_time_rule";

            var elements = new List<RTCTimeSeriesXML>
            {
                CreateRelativeTimeRuleElement(relativeTimeRuleName, groupName1),
                CreateRelativeTimeRuleElement(relativeTimeRuleName, groupName2),
                CreateTimeRuleElement(timeRuleName, groupName1),
                CreateTimeRuleElement(timeRuleName, groupName2),
                CreateTimeRuleElement(extraTimeRuleName, groupName2)
            };

            var controlGroup2 = new ControlGroup {Name = groupName2};
            var controlGroup1 = new ControlGroup {Name = groupName1};
            var controlGroups = new List<ControlGroup> {controlGroup1, controlGroup2};      

            // When
            RealTimeControlDataConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(elements, controlGroups);

            // Then
            Assert.NotNull(controlGroup1.Rules.Select(r => r is TimeRule && r.Name == timeRuleName));
            Assert.NotNull(controlGroup2.Rules.Select(r => r is TimeRule && r.Name == timeRuleName));
            Assert.NotNull(controlGroup1.Rules.Select(r => r is RelativeTimeRule && r.Name == relativeTimeRuleName));
            Assert.NotNull(controlGroup1.Rules.Select(r => r is RelativeTimeRule && r.Name == relativeTimeRuleName));
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsNull_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_ThenMethodReturns()
        {
            // Given
            var controlGroups = new List<ControlGroup>();

            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(null, controlGroups);
            });

            // Then
            Assert.IsEmpty(controlGroups.SelectMany(c => c.Conditions));
        }

        [Test]
        public void GivenAListOfControlGroupsIsNull_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_ThenMethodReturns()
        {
            // Given
            var elements = new List<RTCTimeSeriesXML>();

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(elements, null);
            });
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXml_WhenCreateConditionsFromXmlElementsAndAddToControlGroupIsCalled_ThenTheCorrectConditionIsCreatedAndAddedToControlGroup()
        {
            // Given
            const string groupName1 = "control_group_1";
            const string groupName2 = "control_group_2";
            const string timeConditionName = "time_condition";
            const string standardConditionName = "standard_condition";
            const string extraTimeConditionName = "extra_time_condition";

            var elements = new List<RTCTimeSeriesXML>
            {
                CreateStandardConditionElement(standardConditionName, groupName1),
                CreateStandardConditionElement(standardConditionName, groupName2),
                CreateTimeConditionElement(timeConditionName, groupName1),
                CreateTimeConditionElement(timeConditionName, groupName2),
                CreateTimeConditionElement(extraTimeConditionName, groupName2)
            };

            var controlGroup2 = new ControlGroup { Name = groupName2 };
            var controlGroup1 = new ControlGroup { Name = groupName1 };
            var controlGroups = new List<ControlGroup> { controlGroup1, controlGroup2 };

            // When
            RealTimeControlDataConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(elements, controlGroups);

            // Then
            Assert.NotNull(controlGroup1.Conditions.FirstOrDefault(c => c is TimeCondition && c.Name == timeConditionName));
            Assert.NotNull(controlGroup2.Conditions.FirstOrDefault(c => c is TimeCondition && c.Name == timeConditionName));
            Assert.NotNull(controlGroup1.Conditions.FirstOrDefault(c => c is StandardCondition && c.Name == standardConditionName));
            Assert.NotNull(controlGroup2.Conditions.FirstOrDefault(c => c is StandardCondition && c.Name == standardConditionName));
            Assert.Null(controlGroup1.Conditions.FirstOrDefault(c=> c.Name == extraTimeConditionName));
            Assert.NotNull(controlGroup2.Conditions.FirstOrDefault(r => r is TimeCondition && r.Name == extraTimeConditionName));
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsNull_WhenGetConnectionPointsFromXmlElementsIsCalled_ThenNullIsReturned()
        {
            // Given

            Assert.DoesNotThrow(() =>
            {
                // When
                var connectionPoints = RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(null);

                // Then
                Assert.IsNull(connectionPoints);
            });
        }

        [Test]
        public void GivenAListOfRtcTimeSeriesXml_WhenGetConnectionPointsFromXmlElementsIsCalled_ThenAListOfConnectionPointsIsReturned()
        {
            // Given
            var inputElement = CreateInputElement("parameter", "quantity");
            var outputElement = CreateOutputElement("parameter", "quantity");
            var timeConditionElement = CreateTimeConditionElement("time_condition", "control_group");

            var elements = new List<RTCTimeSeriesXML> { inputElement, outputElement, timeConditionElement};

            // When
            var connectionPoints = RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(elements);

            // Then
            Assert.NotNull(elements);
            Assert.AreEqual(2, connectionPoints.Count);

            var inputs = connectionPoints.OfType<Input>().ToList();
            Assert.NotNull(inputs);
            Assert.AreEqual(1, inputs.Count);
            Assert.AreEqual(inputElement.id, inputs[0].Name);

            var outputs = connectionPoints.OfType<Output>().ToList();
            Assert.NotNull(outputs);
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual(outputElement.id, outputs[0].Name);
        }

        // GetConnectionPointsByTagFromXmlElements
        [Test]
        public void GivenAListOfRtcTimeSeriesXmlIsNull_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenNoExceptionIsThrown()
        {
            // Given
            var controlgroups = new List<ControlGroup>();
            var outputs = new List<Output>();
            
            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(null, controlgroups, outputs);
            });
        }

        [Test]
        public void GivenAListOfControlGroupsIsNull_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenNoExceptionIsThrown()
        {
            // Given
            var elements = new List<RTCTimeSeriesXML>();
            var outputs = new List<Output>();

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(elements, null, outputs);
            });
        }

        [Test]
        public void GivenAListOfOutputsIsNull_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenNoExceptionIsThrown()
        {
            // Given
            var elements = new List<RTCTimeSeriesXML>();
            var controlgroups = new List<ControlGroup>();

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(elements, controlgroups, null);
            });
        }

        [Test]
        public void GivenAListOfOutputAsInputElementsWithNonexistingRelativeTimeRule_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenExpectedErrorMessageIsGiven()
        {
            // Given
            const string outputNameElement = "[Output]parameter/quantity";
            var output = new Output { Name = outputNameElement };

            const string ruleNameElement = "relative_time_rule";
            var rule = new RelativeTimeRule { Name = "some_other_name" };

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);

            var element = CreateOutputAsInputElement("parameter", "quantity", ruleNameElement);

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(new List<RTCTimeSeriesXML> { element }, new[] { controlGroup }, new[] { output });

                // Then
            }, string.Format(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_Output___0___is_input_for_rule___1____but_the_rule_could_not_be_found__See_file____2___, outputNameElement, ruleNameElement, RealTimeControlXMLFiles.XmlData));
        }

        [Test]
        public void GivenARelativeTimeRuleWithFromValueSetToTrue_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenExpectedErrorMessageIsGiven()
        {
            // Given
            const string outputNameElement = "[Output]parameter/quantity";
            var output = new Output { Name = outputNameElement };

            const string ruleNameElement = "relative_time_rule";
            var rule = new RelativeTimeRule { Name = ruleNameElement, FromValue = true};

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);

            var element = CreateOutputAsInputElement("parameter", "quantity", ruleNameElement);

            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(new List<RTCTimeSeriesXML> { element }, new[] { controlGroup }, new[] { output });
                
                // Then
            }, string.Format(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_Relative_Time_Rules_can_only_have_one_output_as_input__It_seems_that_rule___0___has_multiple_outputs_as_input__See_file____1___, ruleNameElement, RealTimeControlXMLFiles.XmlData));

            Assert.IsEmpty(rule.Outputs);
        }

        [Test]
        public void GivenAnXmlReferencingAnOutputThatDoesNotExist_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenExpectedErrorMessageIsGiven()
        {
            // Given
            const string outputNameElement = "[Output]parameter/quantity";
            var output = new Output { Name = "some_other_name" };

            const string ruleNameElement = "relative_time_rule";
            var rule = new RelativeTimeRule { Name = ruleNameElement };

            var controlGroup = new ControlGroup();                          
            controlGroup.Rules.Add(rule);

            var element = CreateOutputAsInputElement("parameter", "quantity", ruleNameElement);

            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                // When
                RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(new List<RTCTimeSeriesXML> { element }, new[] { controlGroup }, new[] { output });

                // Then
            }, string.Format(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_When_getting_an_output_as_input_for_rule___0____the_output___1___could_not_be_found_in_the_file__See_file____2___, ruleNameElement, outputNameElement, RealTimeControlXMLFiles.XmlData));

            Assert.IsEmpty(rule.Outputs);
        }

        [Test]
        public void GivenAnXmlElementReferencingExistingOutputAndRelativeTimeRule_WhenAddOutputAsInputForRelativeTimeRuleIsCalled_ThenOutputIsSetOnRelativeTimeRule()
        {
            // Given
            const string outputNameElement = "[Output]parameter/quantity";
            var output = new Output { Name = outputNameElement };

            const string ruleNameElement = "relative_time_rule";
            var rule = new RelativeTimeRule { Name = ruleNameElement };

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);

            var element = CreateOutputAsInputElement("parameter", "quantity", ruleNameElement);

            // When
            RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(new List<RTCTimeSeriesXML> { element }, new[] { controlGroup }, new[] { output });

            // Then
            Assert.NotNull(rule.Outputs);
            Assert.AreEqual(1, rule.Outputs.Count);
            Assert.AreEqual(output, rule.Outputs[0]);
        }

        private static RTCTimeSeriesXML CreateTimeConditionElement(string conditionName, string controlGroupName, PIInterpolationOptionEnumStringType interpolation = PIInterpolationOptionEnumStringType.BLOCK, PIExtrapolationOptionEnumStringType extrapolation = PIExtrapolationOptionEnumStringType.BLOCK)
        {
            var element = new RTCTimeSeriesXML {id = $"{RtcXmlTag.TimeCondition}{controlGroupName}/{conditionName}"};
            var piTimeSeries = new PITimeSeriesXML
            {
                locationId = $"{controlGroupName}/{conditionName}",
                parameterId = "TimeSeries",
                interpolationOption = interpolation,
                extrapolationOption = extrapolation
            };
            element.PITimeSeries = piTimeSeries;
            return element;
        }

        private static RTCTimeSeriesXML CreateRelativeTimeRuleElement(string ruleName, string controlGroupName, PIInterpolationOptionEnumStringType interpolation = PIInterpolationOptionEnumStringType.BLOCK)
        {
            var element = new RTCTimeSeriesXML {id = $"{RtcXmlTag.RelativeTimeRule}{controlGroupName}/{ruleName}"};
            var piTimeSeries = new PITimeSeriesXML
            {
                locationId = $"{controlGroupName}/{ruleName}",
                parameterId = "TimeSeries",
                interpolationOption = interpolation,
                extrapolationOption = PIExtrapolationOptionEnumStringType.BLOCK
            };
            element.PITimeSeries = piTimeSeries;
            return element;
        }

        private static RTCTimeSeriesXML CreateTimeRuleElement(string ruleName, string controlGroupName, PIInterpolationOptionEnumStringType interpolation = PIInterpolationOptionEnumStringType.BLOCK)
        {
            var element = new RTCTimeSeriesXML {id = $"{RtcXmlTag.TimeRule}{controlGroupName}/{ruleName}"};
            var piTimeSeries = new PITimeSeriesXML
            {
                locationId = $"{controlGroupName}/{ruleName}",
                parameterId = "TimeSeries",
                interpolationOption = interpolation,
                extrapolationOption = PIExtrapolationOptionEnumStringType.BLOCK
            };
            element.PITimeSeries = piTimeSeries;
            return element;
        }

        private static RTCTimeSeriesXML CreateStandardConditionElement(string conditionName, string controlGroupName)
        {
            var element = new RTCTimeSeriesXML
            {
                id = $"{RtcXmlTag.StandardCondition}{RtcXmlTag.Status}{controlGroupName}/{conditionName}"
            };
            return element;
        }

        private static RTCTimeSeriesXML CreateInputElement(string parameterName, string quantityName)
        {
            var element = new RTCTimeSeriesXML {id = $"{RtcXmlTag.Input}{parameterName}/{quantityName}"};
            var openMiExchangeItem = new OpenMIExchangeItemXML
            {
                elementId = parameterName,
                quantityId = quantityName
            };
            element.OpenMIExchangeItem = openMiExchangeItem;
            return element;
        }

        private static RTCTimeSeriesXML CreateOutputElement(string parameterName, string quantityName)
        {
            var element = new RTCTimeSeriesXML {id = $"{RtcXmlTag.Output}{parameterName}/{quantityName}"};
            var openMiExchangeItem = new OpenMIExchangeItemXML
            {
                elementId = parameterName,
                quantityId = quantityName
            };
            element.OpenMIExchangeItem = openMiExchangeItem;
            return element;
        }

        private static RTCTimeSeriesXML CreateOutputAsInputElement(string parameterName, string quantityName, string ruleName)
        {
            var element = new RTCTimeSeriesXML
            {
                id = $"{RtcXmlTag.Output}{parameterName}/{quantityName}{RtcXmlTag.OutputAsInput}{ruleName}"
            };
            var openMiExchangeItem = new OpenMIExchangeItemXML
            {
                elementId = parameterName,
                quantityId = quantityName
            };
            element.OpenMIExchangeItem = openMiExchangeItem;
            return element;
        }
    }
}
