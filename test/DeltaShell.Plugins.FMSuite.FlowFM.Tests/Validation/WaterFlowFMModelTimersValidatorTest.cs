using System;
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

            var validator = new WaterFlowFMModelTimersValidator();

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
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                var validator = new WaterFlowFMModelTimersValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.IsEmpty(issues);
            }
        }

        [Test]
        [TestCase(0.1)]
        [TestCase(0.99)]
        [TestCase(1.01)]
        [TestCase(1.56)]
        [TestCase(1.99)]
        public void ValidatingWaterFlowFMModelWaqIntervalNotIntegerMultipleOfTimeStepReturnsOneError(double factor)
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                var newWaqInterval = new TimeSpan((long) (model.TimeStep.TotalMilliseconds * factor));
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(1, issues.Length);
                ValidationIssue validationIssue = issues[0];
                string category = model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).PropertyDefinition.Category;
                Assert.AreEqual(category, validationIssue.Subject);
                Assert.AreEqual(ValidationSeverity.Error, validationIssue.Severity);
                Assert.AreEqual("Waq output interval must be a multiple of the output timestep.", validationIssue.Message);
                Assert.AreSame(model, validationIssue.ViewData);
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void ValidatingWaterFlowFMModelWaqIntervalIsIntegerMultipleOfTimeStepReturnsNoIssues(int factor)
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                var newWaqInterval = new TimeSpan(model.TimeStep.Ticks * factor);
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.IsEmpty(issues);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelWaqIntervalIsIntegerMultipleOfTimeStepZeroTimeStep()
        {
            // this will return an issue, because you cannot devide by 0. So waq will not output.
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                model.TimeStep = new TimeSpan(0);
                var newWaqInterval = new TimeSpan(500);
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(3, issues.Length);
                ValidationIssue validationIssue = issues[2];
                string category = model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).PropertyDefinition.Category;
                Assert.AreEqual(category, validationIssue.Subject);
                Assert.AreEqual(ValidationSeverity.Error, validationIssue.Severity);
                Assert.AreEqual("Waq output interval must be a multiple of the output timestep.", validationIssue.Message);
                Assert.AreSame(model, validationIssue.ViewData);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelUserTimeStep()
        {
            // setup
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                var validator = new WaterFlowFMModelTimersValidator();
                Assert.AreEqual(new TimeSpan(0, 0, 5, 0), model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);
                Assert.AreEqual(new TimeSpan(0, 0, 20, 0), model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);
                
                // set invalid user output timestep
                model.TimeStep = new TimeSpan(0, 0, 7, 0);

                // call
                ValidationIssue[] issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(2, issues.Length);
                Assert.AreEqual("His output interval must be a multiple of the output timestep.", issues[0].Message);
                Assert.AreEqual("Map output interval must be a multiple of the output timestep.", issues[1].Message);
                
                // set valid user output timestep
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
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                // arrange
                model.StopTime = new DateTime(2017, 8, 7);
                model.StartTime = new DateTime(2017, 8, 8);
                var validator = new WaterFlowFMModelTimersValidator();

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
            using (WaterFlowFMModel model = CreateWaterFlowFMModelWithValidTimers())
            {
                model.ReferenceTime = new DateTime(2027, 8, 7);
                model.StartTime = new DateTime(2017, 8, 7);
                var validator = new WaterFlowFMModelTimersValidator();

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

        private WaterFlowFMModel CreateWaterFlowFMModelWithValidTimers()
        {
            return new WaterFlowFMModel();
        }
    }
}