using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMModelTimersValidatorTest
    {
        [Test]
        public void ValidatingNonWaterFlowFMModelReturnNoIssues()
        {
            // setup
            var mocks = new MockRepository();
            var modelStub = mocks.Stub<ITimeDependentModel>();
            modelStub.StartTime = new DateTime(2015, 1, 14, 0, 0, 0);
            modelStub.StopTime = new DateTime(2015, 1, 14, 0, 1, 0);
            modelStub.TimeStep = new TimeSpan(0, 0, 1, 0);
            mocks.ReplayAll();

            WaterFlowFMModelTimersValidator validator = CreateValidator();

            // call
            ValidationIssue[] issues = validator.ValidateModelTimers(modelStub, new TimeSpan(0, 0, 1, 0)).ToArray();

            // assert
            Assert.IsEmpty(issues);
            mocks.VerifyAll();
        }

        [Test]
        public void ValidatingWaterFlowFMModelWithValidTimersReturnsNoIssues()
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.IsEmpty(issues);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidIntervalFactorTestCases))]
        public void ValidatingWaterFlowFMModelIntervalNotIntegerMultipleOfTimeStepReturnsOneIssue(
            string writeParameter,
            string timeSpanParameter,
            double factor)
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                model.TimeStep = new TimeSpan(0, 0, 5, 0);
                model.ModelDefinition.GetModelProperty(writeParameter).Value = true;
                model.ModelDefinition.GetModelProperty(timeSpanParameter).Value = new TimeSpan((long)(model.TimeStep.Ticks * factor));

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.That(issues.Length, Is.EqualTo(1));
                Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
                Assert.That(issues[0].ViewData, Is.SameAs(model));

                string expectedCategory = model.ModelDefinition.GetModelProperty(timeSpanParameter).PropertyDefinition.Category;
                var expectedMessage = "output interval must be a multiple of the user timestep.";

                Assert.That(issues[0].Subject, Is.EqualTo(expectedCategory));
                Assert.That(issues[0].Message, Does.Contain(expectedMessage));
            }
        }

        private static IEnumerable<TestCaseData> GetInvalidIntervalFactorTestCases()
        {
            var factors = new[] { 0.1, 0.99, 1.01, 1.56, 1.99 };
            IEnumerable<TestCaseData> testCases = GetTimerPropertyTestCases();
            return testCases.SelectMany(tc => factors.Select(f => ExtendTestCaseData(tc, f)));
        }

        [Test]
        [TestCaseSource(nameof(GetValidIntervalFactorTestCases))]
        public void ValidatingWaterFlowFMModelIntervalIsIntegerMultipleOfTimeStepReturnsNoIssues(
            string writeParameter,
            string timeSpanParameter,
            int factor)
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty(writeParameter).Value = true;
                model.ModelDefinition.GetModelProperty(timeSpanParameter).Value = new TimeSpan(model.TimeStep.Ticks * factor);

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.That(issues, Is.Empty);
            }
        }

        private static IEnumerable<TestCaseData> GetValidIntervalFactorTestCases()
        {
            var factors = new[] { 1, 2, 5 };
            IEnumerable<TestCaseData> testCases = GetTimerPropertyTestCases();
            return testCases.SelectMany(tc => factors.Select(f => ExtendTestCaseData(tc, f)));
        }

        [Test]
        [TestCaseSource(nameof(GetTimerPropertyTestCases))]
        public void ValidatingWaterFlowFMModelInvalidTimerWithUncheckedFlagNoReturnsIssues(
            string writeParameter,
            string timeSpanParameter)
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                model.TimeStep = new TimeSpan(0, 0, 5, 0);
                model.ModelDefinition.GetModelProperty(writeParameter).Value = false;
                model.ModelDefinition.GetModelProperty(timeSpanParameter).Value = new TimeSpan(0, 0, 6, 0);

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.That(issues, Is.Empty);
            }
        }

        private static IEnumerable<TestCaseData> GetTimerPropertyTestCases()
        {
            yield return new TestCaseData(GuiProperties.WriteHisFile, GuiProperties.HisOutputDeltaT);
            yield return new TestCaseData(GuiProperties.WriteMapFile, GuiProperties.MapOutputDeltaT);
            yield return new TestCaseData(GuiProperties.WriteClassMapFile, GuiProperties.ClassMapOutputDeltaT);
            yield return new TestCaseData(GuiProperties.WriteRstFile, GuiProperties.RstOutputDeltaT);
            yield return new TestCaseData(GuiProperties.SpecifyWaqOutputInterval, GuiProperties.WaqOutputDeltaT);
        }

        [Test]
        public void ValidatingWaterFlowFMModelWaqIntervalIsIntegerMultipleOfTimeStepZeroTimeStep()
        {
            // this will return an issue, because you cannot devide by 0. So waq will not output.
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                model.TimeStep = new TimeSpan(0);
                var newWaqInterval = new TimeSpan(500);
                model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value = true;
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(3, issues.Length);
                ValidationIssue validationIssue = issues[2];
                string category = model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).PropertyDefinition.Category;
                Assert.AreEqual(category, validationIssue.Subject);
                Assert.AreEqual(ValidationSeverity.Error, validationIssue.Severity);
                Assert.AreEqual("Waq output interval must be a multiple of the user timestep.", validationIssue.Message);
                Assert.AreSame(model, validationIssue.ViewData);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelUserTimeStep()
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                WaterFlowFMModelTimersValidator validator = CreateValidator();

                Assert.AreEqual(new TimeSpan(0, 0, 5, 0), model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);
                Assert.AreEqual(new TimeSpan(0, 0, 20, 0), model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);

                // set invalid user timestep
                model.TimeStep = new TimeSpan(0, 0, 7, 0);

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(2, issues.Length);
                Assert.AreEqual("His output interval must be a multiple of the user timestep.", issues[0].Message);
                Assert.AreEqual("Map output interval must be a multiple of the user timestep.", issues[1].Message);

                // set valid user timestep
                model.TimeStep = new TimeSpan(0, 0, 1, 0);

                // call
                issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(0, issues.Length);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelStopTimeSmallerThanStartTimeTest()
        {
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                // arrange
                model.StopTime = new DateTime(2017, 8, 7);
                model.StartTime = new DateTime(2017, 8, 8);

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // act
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();
                ValidationIssue issue = issues[0];

                // assert
                Assert.AreEqual(1, issues.Length);
                Assert.AreEqual("The calculation period must be positive.", issue.Message);
                Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            }
        }

        [Test]
        public void GivenFmModelWithReferenceTimeGreaterThanStartTime_WhenValidatingTime_ThenValidationErrorIsReturnedWithExpectedViewData()
        {
            // Given
            using (WaterFlowFMModel model = CreateWaterFlowFMModel())
            {
                model.ReferenceTime = new DateTime(2027, 8, 7);
                model.StartTime = new DateTime(2017, 8, 7);

                WaterFlowFMModelTimersValidator validator = CreateValidator();

                // When
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // Then
                Assert.That(issues.Length, Is.EqualTo(2));
                Assert.That(issues[0].Message, Is.EqualTo("The calculation period must be positive."));
                Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
                Assert.That(issues[1].Message, Is.EqualTo("Model start time precedes reference time"));
                Assert.That(issues[1].Severity, Is.EqualTo(ValidationSeverity.Error));

                var viewData = issues[1].ViewData as FmValidationShortcut;
                Assert.IsNotNull(viewData);
                Assert.That(viewData.FlowFmModel, Is.EqualTo(model));
                Assert.That(viewData.TabName, Is.EqualTo("Time Frame"));
            }
        }

        private static WaterFlowFMModelTimersValidator CreateValidator()
        {
            return new WaterFlowFMModelTimersValidator();
        }

        private static WaterFlowFMModel CreateWaterFlowFMModel()
        {
            return new WaterFlowFMModel();
        }

        private static TestCaseData ExtendTestCaseData(TestCaseData tc, params object[] args)
        {
            return new TestCaseData(tc.Arguments.Concat(args).ToArray());
        }
    }
}