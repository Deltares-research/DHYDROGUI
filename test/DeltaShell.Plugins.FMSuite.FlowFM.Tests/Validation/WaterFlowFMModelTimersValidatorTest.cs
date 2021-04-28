using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
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
            var issues = validator.ValidateModelTimers(modelStub, new TimeSpan(0, 0, 1, 0)).ToArray();

            // assert
            Assert.IsEmpty(issues);
            mocks.VerifyAll();
        }

        [Test]
        public void ValidatingWaterFlowFMModelWithValidTimersReturnsNoIssues()
        {
            // setup
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                var validator = new WaterFlowFMModelTimersValidator();

                // call
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

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
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                var newWaqInterval = new TimeSpan((long)(model.TimeStep.TotalMilliseconds * factor));
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(1, issues.Length);
                var validationIssue = issues[0];
                var category = model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).PropertyDefinition.Category;
                Assert.AreEqual(category, validationIssue.Subject);
                Assert.AreEqual(ValidationSeverity.Error, validationIssue.Severity);
                Assert.AreEqual("Waq output interval must be a multiple of the output timestep.", validationIssue.Message);
                var actualModel = ((FmValidationShortcut)validationIssue.ViewData).FlowFmModel;
                Assert.AreSame(model, actualModel);
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void ValidatingWaterFlowFMModelWaqIntervalIsIntegerMultipleOfTimeStepReturnsNoIssues(int factor)
        {
            // setup
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                var newWaqInterval = new TimeSpan(model.TimeStep.Ticks * factor);
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.IsEmpty(issues);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelWaqIntervalIsIntegerMultipleOfTimeStepZeroTimeStep()
        {
            // this will return an issue, because you cannot devide by 0. So waq will not output.
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                model.TimeStep = new TimeSpan(0);
                var newWaqInterval = new TimeSpan(500);
                model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value = newWaqInterval;

                var validator = new WaterFlowFMModelTimersValidator();

                // call
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(4, issues.Length);
                var validationIssue = issues[3];
                var category = model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).PropertyDefinition.Category;
                Assert.AreEqual(category, validationIssue.Subject);
                Assert.AreEqual(ValidationSeverity.Error, validationIssue.Severity);
                Assert.AreEqual("Waq output interval must be a multiple of the output timestep.", validationIssue.Message);
                var actualModel = ((FmValidationShortcut)validationIssue.ViewData).FlowFmModel;
                Assert.AreSame(model, actualModel);
            }

        }

        [Test]
        public void ValidatingWaterFlowFMModelUserTimeStep()
        {
            // setup
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                var validator = new WaterFlowFMModelTimersValidator();
                model.ModelDefinition.SetModelProperty(GuiProperties.RstOutputDeltaT, "86400"); // Default is 0
                
                Assert.AreEqual(new TimeSpan(0, 0, 10, 0), model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);
                Assert.AreEqual(new TimeSpan(0, 1, 0, 0), model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);
                Assert.AreEqual(new TimeSpan(1, 0, 0, 0), model.ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);

                // set invalid user output timestep
                model.TimeStep = new TimeSpan(0, 0, 7, 0);

                // call
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(3, issues.Length);
                Assert.AreEqual("His output interval must be a multiple of the output timestep.", issues[0].Message);
                Assert.AreEqual("Map output interval must be a multiple of the output timestep.", issues[1].Message);
                Assert.AreEqual("Rst output interval must be a multiple of the output timestep.", issues[2].Message);

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
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                // arrange
                model.StopTime = new DateTime(2017, 8, 7);
                model.StartTime = new DateTime(2017, 8, 8);
                model.ReferenceTime = new DateTime(2001, 1, 1);
                var validator = new WaterFlowFMModelTimersValidator();

                // act
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();
                var issue = issues[0];

                // assert
                Assert.AreEqual(1, issues.Length);
                Assert.AreEqual("The calculation period must be positive.", issue.Message);
                Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            }
        }

        [Test]
        public void ValidatingWaterFlowFMModelReferenceTimeGreaterThanStartTime()
        {
            using (var model = CreateWaterFlowFMModelWithValidTimers())
            {
                // arrange
                model.ReferenceTime = new DateTime(2027, 8, 7);
                model.StartTime = new DateTime(2017, 8, 7);
                model.StopTime = new DateTime(2016, 8, 7);
                var validator = new WaterFlowFMModelTimersValidator();

                // act
                var issues = validator.ValidateModelTimers(model, model.OutputTimeStep).ToArray();

                // assert
                Assert.AreEqual(2, issues.Length);
                Assert.AreEqual("The calculation period must be positive.", issues[0].Message);
                Assert.AreEqual(ValidationSeverity.Error, issues[0].Severity);
                Assert.AreEqual("Model start time precedes reference time", issues[1].Message);
                Assert.AreEqual(ValidationSeverity.Error, issues[1].Severity);
            }
        }

        private WaterFlowFMModel CreateWaterFlowFMModelWithValidTimers()
        {
            return new WaterFlowFMModel();
        }
    }
}