using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMLateralValidatorTest
    {
        [Test]
        public void TestValidModelWithoutLaterals_HasNoReports()
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();

            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void TestValidModelWithLateralsNull_ReportsInfoNoLateralsAvailable()
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();
            model.Laterals = null;

            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(InValidTimeZones))]
        public void TestValidModelWithLateralWithInValidTimeZone_ReportsErrorLateralTimeZoneOutOfRange(TimeSpan timeZone)
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();

            const string lateralName1 = "AnUniqueTestName1";
            AddLateralToModel(timeZone, model, lateralName1);
            string expectedReportMessage = string.Format(Resources.WaterFlowFMLateralValidator_ValidateDischargeTimeZone_Time_zone_of_lateral___0___falls_outside_of_allowed_range__12_00_and__12_00,
                                                         lateralName1);

            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues.Count(), Is.EqualTo(1));
            string reportMessage = report.Issues.First().Message;
            Assert.That(reportMessage.Contains(expectedReportMessage), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(InValidTimeZones))]
        public void TestValidModelWithMultipleLateralsWithInValidTimeZone_ReportsErrorLateralTimeZoneOutOfRange(TimeSpan timeZone)
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();

            const string lateralName1 = "AnUniqueTestName1";
            AddLateralToModel(timeZone, model, lateralName1);
            string expectedReportMessage1 = string.Format(Resources.WaterFlowFMLateralValidator_ValidateDischargeTimeZone_Time_zone_of_lateral___0___falls_outside_of_allowed_range__12_00_and__12_00,
                                                          lateralName1);

            const string lateralName2 = "AnUniqueTestName2";
            AddLateralToModel(timeZone, model, lateralName2);
            string expectedReportMessage2 = string.Format(Resources.WaterFlowFMLateralValidator_ValidateDischargeTimeZone_Time_zone_of_lateral___0___falls_outside_of_allowed_range__12_00_and__12_00,
                                                          lateralName2);

            const string lateralName3 = "AnUniqueTestName3";
            AddLateralToModel(timeZone, model, lateralName3);
            string expectedReportMessage3 = string.Format(Resources.WaterFlowFMLateralValidator_ValidateDischargeTimeZone_Time_zone_of_lateral___0___falls_outside_of_allowed_range__12_00_and__12_00,
                                                          lateralName3);

            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues.Count(), Is.EqualTo(3));

            ValidationIssue[] reportMessages = report.Issues.ToArray();
            string reportMessage = reportMessages[0].Message;
            Assert.That(reportMessage.Contains(expectedReportMessage1), Is.True);

            reportMessage = reportMessages[1].Message;
            Assert.That(reportMessage.Contains(expectedReportMessage2), Is.True);

            reportMessage = reportMessages[2].Message;
            Assert.That(reportMessage.Contains(expectedReportMessage3), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ValidTimeZones))]
        public void TestValidModelWithLateralWithValidTimeZone_HasNoReports(TimeSpan timeZone)
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();
            const string lateralName1 = "AnUniqueTestName1";
            AddLateralToModel(timeZone, model, lateralName1);

            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(ValidTimeZones))]
        public void TestValidModelWithMultipleLateralsWithValidTimeZone_HasNoReports(TimeSpan timeZone)
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();
            
            const string lateralName1 = "AnUniqueTestName1";
            AddLateralToModel(timeZone, model, lateralName1);
            
            const string lateralName2 = "AnUniqueTestName2";
            AddLateralToModel(timeZone, model, lateralName2);
            
            const string lateralName3 = "AnUniqueTestName3";
            AddLateralToModel(timeZone, model, lateralName3);
            
            //Act
            ValidationReport report = WaterFlowFMLateralValidator.Validate(model);

            //Assert
            Assert.That(report.Issues, Is.Empty);
        }

        private static IEnumerable<TestCaseData> ValidTimeZones()
        {
            yield return new TestCaseData(new TimeSpan(12, 0, 0));
            yield return new TestCaseData(new TimeSpan(10, 0, 0));
            yield return new TestCaseData(new TimeSpan(5, 0, 0));
            yield return new TestCaseData(new TimeSpan(0, 0, 0));
            yield return new TestCaseData(new TimeSpan(-5, 0, 0));
            yield return new TestCaseData(new TimeSpan(-10, 0, 0));
            yield return new TestCaseData(new TimeSpan(-12, 0, 0));
        }

        private static IEnumerable<TestCaseData> InValidTimeZones()
        {
            yield return new TestCaseData(new TimeSpan(20, 0, 0));
            yield return new TestCaseData(new TimeSpan(15, 0, 0));
            yield return new TestCaseData(new TimeSpan(13, 0, 0));
            yield return new TestCaseData(new TimeSpan(-13, 0, 0));
            yield return new TestCaseData(new TimeSpan(-15, 0, 0));
            yield return new TestCaseData(new TimeSpan(-20, 0, 0));
        }

        private static WaterFlowFMModel CreateValidModel()
        {
            return new WaterFlowFMModel
            {
                TimeStep = new TimeSpan(0, 0, 1, 0),
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                OutputTimeStep = new TimeSpan(0, 0, 2, 0)
            };
        }

        private static void AddLateralToModel(TimeSpan timeZone, WaterFlowFMModel model, string name)
        {
            var lateral = new Lateral { Feature = new Feature2D { Name = name } };
            lateral.Data.Discharge.TimeSeries.TimeZone = timeZone;
            model.Laterals.Add(lateral);
        }
    }
}