using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlTimeSeriesSetterTest
    {
        private const string controlGroupName = "control_group_name";
        private const string componentName = "component_name";
        private RealTimeControlTimeSeriesSetter timeSeriesSetter;
        private ILogHandler logHandler;
        private IControlGroup controlGroup;

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            timeSeriesSetter = new RealTimeControlTimeSeriesSetter(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            timeSeriesSetter = null;
            controlGroup = null;
        }

        [Test]
        public void GivenTimeSeriesXmlObjectAndAControlGroupWithAMatchingTimeRule_WhenSetTimeSeriesIsCalled_ThenCorrectTimeSeriesIsSet()
        {
            // Given
            const double value1 = 0.5;
            const double value2 = 100.001;
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.TimeRule, value1, value2);
            controlGroup = CreateControlGroupWithATimeRule();

            // When
            timeSeriesSetter.SetTimeSeries(
                timeSeriesElements,
                new[]
                {
                    controlGroup
                });

            // Then
            CheckValidityTimeSeriesTimeRule(controlGroup, value1, value2);
        }

        [Test]
        public void GivenTimeSeriesXmlObjectAndAControlGroupWithAMatchingTimeCondition_WhenSetTimeSeriesIsCalled_ThenCorrectTimeSeriesIsSet()
        {
            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.TimeCondition, 0, 1);
            controlGroup = CreateControlGroupWithATimeCondition();

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            CheckValidityTimeSeriesTimeCondition(controlGroup, true, false);
        }

        [Test]
        public void GivenTimeSeriesXmlObjectAndAControlGroupWithStandardCondition_WhenSetTimeSeriesIsCalled_ThenExpectedLogMessageIsCalled()
        {
            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.StandardCondition, 0, 0);
            controlGroup = CreateControlGroupWithAStandardCondition();

            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Object_with_id___0___does_not_seem_to_be_a_Time_Rule_or_Time_Condition__See_file____1___,
                CreateLocationId(RtcXmlTag.StandardCondition), RealTimeControlXmlFiles.XmlTimeSeries);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenNullParameters_WhenSetTimeSeriesIsCalled_NothingHappens()
        {
            Assert.DoesNotThrow(() => timeSeriesSetter.SetTimeSeries(null, null),
                                "When calling the method (RealTimeControlTimeSeriesSetter.SetTimeSeries()) with null parameters it threw an unexpected exception.");
        }

        [TestCase(IntervalRule.IntervalRuleSetPointType.Fixed)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Variable)]
        public void GivenTimeSeriesXmlObjectWithConstantValuesForAnIntervalRuleWhichSetpointIsNotSignal_WhenSetTimeSeriesIsCalled_ThenSetPointShouldBeSetToFixedWithCorrectConstantValue(IntervalRule.IntervalRuleSetPointType setPointType)
        {
            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.IntervalRule, 2, 2);
            controlGroup = CreateControlGroupWithAnIntervalRule(setPointType);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            TimeSeries intervalRuleTimeSeries = ((IntervalRule) controlGroup.Rules[0]).TimeSeries;

            Assert.AreEqual(IntervalRule.IntervalRuleSetPointType.Fixed, ((IntervalRule)controlGroup.Rules[0]).SetPointType);
            Assert.AreEqual(2, intervalRuleTimeSeries.Components[0].DefaultValue, "Fixed setpoint value of the interval rule is not imported");
            Assert.AreEqual(0, intervalRuleTimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero after importing the time serie on the interval rule with a fixed setpoint.");
        }

        [Test]
        public void GivenTimeSeriesXmlObjectWithConstantValuesForAnIntervalRuleWhichSetpointIsSignal_WhenSetTimeSeriesIsCalled_ThenAWarningShouldBeGiven()
        {

            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.IntervalRule, 2, 2);

            controlGroup = CreateControlGroupWithAnIntervalRule(IntervalRule.IntervalRuleSetPointType.Signal);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Rule__with_id___0___does_not_seem_to_use_a_time_serie_as_setpoint__See_file____1___Therefore_the_time_serie_is_not_imported,
                $"{RtcXmlTag.IntervalRule}{controlGroupName}/{componentName}", RealTimeControlXmlFiles.XmlTimeSeries);

            Assert.That(logHandler.LogMessages.WarningMessages.Contains(expectedMessage),
                        "The collected log messages did not contain the expected message.");
        }

        [TestCase(IntervalRule.IntervalRuleSetPointType.Fixed)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Variable)]
        public void GivenTimeSeriesXmlObjectWithVariableValuesForAnIntervalRuleWhichSetpointIsNotSignal_WhenSetTimeSeriesIsCalled_ThenSetpointShouldBeSetToVariableAndTheTimeSeriesShouldBeSet(IntervalRule.IntervalRuleSetPointType setPointType)
        {
            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.IntervalRule, 2, 3);
            controlGroup = CreateControlGroupWithAnIntervalRule(setPointType);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            Assert.AreEqual(IntervalRule.IntervalRuleSetPointType.Variable, ((IntervalRule)controlGroup.Rules[0]).SetPointType);
            CheckValidityTimeSeriesIntervalRule(controlGroup, 2, 3);
        }

        [Test]
        public void GivenTimeSeriesXmlObjectWithVariableValuesForAnIntervalRuleWhichSetpointIsSignal_WhenSetTimeSeriesIsCalled_ThenAWarningShouldBeGiven()
        {
            // Given
            List<TimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesElementList(RtcXmlTag.IntervalRule, 2, 3);

            controlGroup = CreateControlGroupWithAnIntervalRule(IntervalRule.IntervalRuleSetPointType.Signal);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Rule__with_id___0___does_not_seem_to_use_a_time_serie_as_setpoint__See_file____1___Therefore_the_time_serie_is_not_imported,
                $"{RtcXmlTag.IntervalRule}{controlGroupName}/{componentName}", RealTimeControlXmlFiles.XmlTimeSeries);

            Assert.That(logHandler.LogMessages.WarningMessages.Contains(expectedMessage),
                        "The collected log messages did not contain the expected message.");
        }

        [TestCase(IntervalRule.IntervalRuleSetPointType.Fixed)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Variable)]
        public void GivenTimeSeriesXmlObjectWithoutValuesForAnIntervalRuleWhichSetpointIsNotSignal_WhenSetTimeSeriesIsCalled_ThenAWarningShouldBeGiven(IntervalRule.IntervalRuleSetPointType setPointType)
        {
            // Given
            string locationId = CreateLocationId(RtcXmlTag.IntervalRule);

            var timeSeriesElements = new List<TimeSeriesComplexType> {new TimeSeriesComplexType {header = new HeaderComplexType {locationId = locationId}}};

            controlGroup = CreateControlGroupWithAnIntervalRule(setPointType);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            var intervalRule = (IntervalRule) controlGroup.Rules[0];

            Assert.AreEqual(0, intervalRule.TimeSeries.Components[0].DefaultValue, "Fixed setpoint value should not be set");
            Assert.AreEqual(0, intervalRule.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero after importing the time serie on the interval rule with a fixed setpoint.");

            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesSetter_For_interval_rule_with_id__0__there_is_no_time_data_found_in_file__1__for_setting_fixed_or_variable_setpoint_type_Setpoint_type_will_be_variable__,
                intervalRule.Name, RealTimeControlXmlFiles.XmlTimeSeries);

            // Then
            Assert.IsTrue(logHandler.LogMessages.WarningMessages.Contains(expectedMessage),
                          "The collected log messages did not contain the expected message.");
        }

        [Test]
        public void GivenTimeSeriesXmlObjectWithoutValuesForAnIntervalRuleWhichSetpointIsSignal_WhenSetTimeSeriesIsCalled_ThenAWarningShouldBeGiven()
        {
            // Given
            string locationId = CreateLocationId(RtcXmlTag.IntervalRule);

            var timeSeriesElements = new List<TimeSeriesComplexType> { new TimeSeriesComplexType { header = new HeaderComplexType { locationId = locationId } } };

            controlGroup = CreateControlGroupWithAnIntervalRule(IntervalRule.IntervalRuleSetPointType.Signal);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Rule__with_id___0___does_not_seem_to_use_a_time_serie_as_setpoint__See_file____1___Therefore_the_time_serie_is_not_imported,
                $"{RtcXmlTag.IntervalRule}{controlGroupName}/{componentName}", RealTimeControlXmlFiles.XmlTimeSeries);

            Assert.That(logHandler.LogMessages.WarningMessages.Contains(expectedMessage),
                        "The collected log messages did not contain the expected message.");
        }

        [TestCase(PIDRule.PIDRuleSetpointType.Signal)]
        [TestCase(PIDRule.PIDRuleSetpointType.Constant)]
        public void GivenTimeSeriesXmlObjectForAPIDRuleWithoutSetPointTypeNotTimeSeries_WhenSetTimeSeriesIsCalled_ThenAWarningShouldBeGiven(
            PIDRule.PIDRuleSetpointType setPointType)
        {
            // Given
            string locationId = CreateLocationId(RtcXmlTag.PIDRule);

            var timeSeriesElements = new List<TimeSeriesComplexType> {new TimeSeriesComplexType {header = new HeaderComplexType {locationId = locationId}}};

            controlGroup = CreateControlGroupWithAPIDRule(setPointType);

            // When
            timeSeriesSetter.SetTimeSeries(timeSeriesElements, new[]
            {
                controlGroup
            });

            // Then
            string expectedMessage = string.Format(
                Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Rule__with_id___0___does_not_seem_to_use_a_time_serie_as_setpoint__See_file____1___Therefore_the_time_serie_is_not_imported,
                locationId, RealTimeControlXmlFiles.XmlTimeSeries);

            Assert.That(logHandler.LogMessages.WarningMessages.Contains(expectedMessage),
                        "The collected log messages did not contain the expected message.");
        }

        private static string CreateLocationId(string tag)
        {
            return $"{tag}{controlGroupName}/{componentName}";
        }

        private static List<TimeSeriesComplexType> CreateTimeSeriesElementList(string tag, double value1, double value2)
        {
            string locationId = CreateLocationId(tag);

            var timeSeriesElements = new List<TimeSeriesComplexType> {GetTimeSeriesElement(locationId, value1, value2)};
            return timeSeriesElements;
        }

        private static TimeSeriesComplexType GetTimeSeriesElement(string locationId, double value1, double value2)
        {
            var ruleTimeSeriesElement = new TimeSeriesComplexType
            {
                header = new HeaderComplexType
                {
                    type = timeSeriesType.instantaneous,
                    locationId = locationId,
                    parameterId = "TimeSeries",
                    timeStep = new TimeStepComplexType
                    {
                        unit = timeStepUnitEnumStringType.minute,
                        multiplier = "30",
                        divider = "1"
                    },
                    startDate = new DateTimeComplexType
                    {
                        date = new DateTime(2018, 12, 12),
                        time = new DateTime(2018, 12, 12, 0, 0, 0)
                    },
                    endDate = new DateTimeComplexType
                    {
                        date = new DateTime(2018, 12, 13),
                        time = new DateTime(2018, 12, 13, 0, 0, 0)
                    },
                    missVal = -999
                },
                @event = new[]
                {
                    new EventComplexType
                    {
                        date = new DateTime(2018, 12, 12),
                        time = new DateTime(2018, 12, 12, 0, 0, 0, 0),
                        value = value1
                    },
                    new EventComplexType
                    {
                        date = new DateTime(2018, 12, 13),
                        time = new DateTime(2018, 12, 13, 0, 0, 0, 0),
                        value = value2
                    }
                }
            };
            return ruleTimeSeriesElement;
        }

        private static ControlGroup CreateControlGroupWithAStandardCondition()
        {
            var standardCondition = new StandardCondition {Name = componentName};
            var controlGroup = new ControlGroup {Name = controlGroupName};
            controlGroup.Conditions.Add(standardCondition);
            Assert.IsFalse(standardCondition is ITimeDependentRtcObject,
                           "Expected was that this object was not an TimeDependentRtcObject. If it is, this test is not valid anymore.");

            return controlGroup;
        }

        private static ControlGroup CreateControlGroupWithATimeCondition()
        {
            var timeCondition = new TimeCondition {Name = componentName};
            var controlGroup = new ControlGroup {Name = controlGroupName};
            controlGroup.Conditions.Add(timeCondition);
            Assert.AreEqual(0, timeCondition.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the time condition.");

            return controlGroup;
        }

        private static ControlGroup CreateControlGroupWithATimeRule()
        {
            var timeRule = new TimeRule(componentName);
            var controlGroup = new ControlGroup {Name = controlGroupName};
            controlGroup.Rules.Add(timeRule);
            Assert.AreEqual(0, timeRule.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the time rule.");

            return controlGroup;
        }

        private static ControlGroup CreateControlGroupWithAnIntervalRule(
            IntervalRule.IntervalRuleSetPointType intervalType)
        {
            var intervalRule = new IntervalRule(componentName) {SetPointType = intervalType};
            var controlGroup = new ControlGroup {Name = controlGroupName};
            controlGroup.Rules.Add(intervalRule);
            Assert.AreEqual(0, intervalRule.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the interval rule.");

            return controlGroup;
        }

        private static ControlGroup CreateControlGroupWithAPIDRule(PIDRule.PIDRuleSetpointType setPointType)
        {
            var pidRule = new PIDRule(componentName) {PidRuleSetpointType = setPointType};
            var controlGroup = new ControlGroup {Name = controlGroupName};
            controlGroup.Rules.Add(pidRule);
            Assert.AreEqual(0, pidRule.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the pid rule.");

            return controlGroup;
        }

        private static void CheckValidityTimeSeriesTimeCondition(IControlGroup controlGroup, bool value1, bool value2)
        {
            TimeCondition timeCondition = controlGroup.Conditions.OfType<TimeCondition>().Single();
            TimeSeries timeConditionTimeSeries = timeCondition.TimeSeries;
            Assert.AreEqual(2, timeConditionTimeSeries.GetValues().Count,
                            "Expected was that the count of time series records of the time rule was 2.");

            IMultiDimensionalArray timeConditionTimeSeriesDates = timeConditionTimeSeries.Arguments[0].Values;
            Assert.AreEqual(new DateTime(2018, 12, 12), timeConditionTimeSeriesDates[0]);
            Assert.AreEqual(new DateTime(2018, 12, 13), timeConditionTimeSeriesDates[1]);

            IMultiDimensionalArray timeConditionTimeSeriesValues = timeConditionTimeSeries.Components[0].Values;
            Assert.AreEqual(value1, timeConditionTimeSeriesValues[0]);
            Assert.AreEqual(value2, timeConditionTimeSeriesValues[1]);
        }

        private static void CheckValidityTimeSeriesTimeRule(IControlGroup controlGroup, double value1, double value2)
        {
            TimeRule timeRule = controlGroup.Rules.OfType<TimeRule>().Single();
            Assert.AreEqual(2, timeRule.TimeSeries.GetValues().Count,
                            "Expected was that the count of time series records of the time rule was 2.");

            TimeSeries timeRuleTimeSeries = timeRule.TimeSeries;
            IMultiDimensionalArray timeRuleTimeSeriesDates = timeRuleTimeSeries.Arguments[0].Values;
            Assert.AreEqual(new DateTime(2018, 12, 12), timeRuleTimeSeriesDates[0]);
            Assert.AreEqual(new DateTime(2018, 12, 13), timeRuleTimeSeriesDates[1]);

            IMultiDimensionalArray timeRuleTimeSeriesValues = timeRuleTimeSeries.Components[0].Values;
            Assert.AreEqual(value1, timeRuleTimeSeriesValues[0]);
            Assert.AreEqual(value2, timeRuleTimeSeriesValues[1]);
        }

        private static void CheckValidityTimeSeriesIntervalRule(IControlGroup controlGroup, double value1, double value2)
        {
            IntervalRule intervalRule = controlGroup.Rules.OfType<IntervalRule>().Single();
            Assert.AreEqual(2, intervalRule.TimeSeries.GetValues().Count,
                            "Expected was that the count of time series records of the time rule was 2.");

            TimeSeries intervalRuleTimeSeries = intervalRule.TimeSeries;
            IMultiDimensionalArray timeRuleTimeSeriesDates = intervalRuleTimeSeries.Arguments[0].Values;
            Assert.AreEqual(new DateTime(2018, 12, 12), timeRuleTimeSeriesDates[0]);
            Assert.AreEqual(new DateTime(2018, 12, 13), timeRuleTimeSeriesDates[1]);

            IMultiDimensionalArray timeRuleTimeSeriesValues = intervalRuleTimeSeries.Components[0].Values;
            Assert.AreEqual(value1, timeRuleTimeSeriesValues[0]);
            Assert.AreEqual(value2, timeRuleTimeSeriesValues[1]);
        }
    }
}