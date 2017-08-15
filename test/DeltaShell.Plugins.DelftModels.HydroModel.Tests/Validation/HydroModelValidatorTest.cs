using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.HydroModel.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using NUnit.Framework;
using Rhino.Mocks;
using Resources = DeltaShell.Plugins.DelftModels.HydroModel.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Validation
{
    [TestFixture]
    public class HydroModelValidatorTest
    {
        private MockRepository mocks;
        private HydroModelValidator validator;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            validator = new HydroModelValidator();

        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenHydroModelWithCurrentWorkflowEqualToNullWhenValidatingThenReturnDefaultValidationReportWithLengthOne()
        {
            var model = new HydroModel();

            var result = validator.Validate(model);
            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.AreEqual("Integrated Model (Hydro Model)", result.Category);
            
            var issue = result.AllErrors.ToArray().First();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            Assert.AreEqual("Current Workflow cannot be empty", issue.Message);
        }

        [Test]
        public void GivenIntegratedModelWithRainfallRunoffAsCurrentWorkflowWhenValidatingThenReturnHydroModelSpecificWorkflowValidationIssue()
        {
            // Setup
            var workflowName = "RR as sequential activity";
            var hydroModel = new HydroModel();
            
            var workFlow = new SequentialActivity
            {
                Name = workflowName,
                Activities = {new RainfallRunoffModel()}
            };
            hydroModel.CurrentWorkflow = workFlow;

            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports.Where(r => r.Category.Contains("HydroModel Specific")).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(1));

            var workflowIssue = hydroModelSpecificIssues[0];
            Assert.That(workflowIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(workflowIssue.Message, Is.EqualTo(string.Format(Resources.HydroModel_LogErrorsWhenUnsupportedWorkflow_The_workflow___0___is_currently_not_supported_in_DeltaShell, workflowName)));
        }

        [Test]
        public void GivenIntegratedModelWithIActivityAsCurrentWorkflowWhenValidatingThenReturnNoHydroModelSpecificIssues()
        {
            var hydroModel = new HydroModel();

            var activity = mocks.DynamicMock<IActivity>();
            mocks.ReplayAll();

            var workFlow = new SequentialActivity
            {
                Activities = { activity }
            };
            hydroModel.CurrentWorkflow = workFlow;

            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports.Where(r => r.Category.Contains("HydroModel Specific")).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(0));
        }
        
        [Test]
        public void GivenIntegratedModelWithEquallyNamedModelsInCurrentWorkflowWhenValidatingThenReturnModelStructureValidationIssue()
        {
            // Setup
            var modelName = "SameName";
            var model1 = mocks.DynamicMock<IActivity>();
            model1.Expect(m => m.Name).Return(modelName).Repeat.Any();
            
            var model2 = mocks.DynamicMock<IActivity>();
            model2.Expect(m => m.Name).Return(modelName).Repeat.Any();
            
            var workflow = mocks.DynamicMock<ICompositeActivity>();
            workflow.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model1, model2})
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow
            };
            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports.Where(r => r.Category.Contains("HydroModel Specific")).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(1));

            var workflowIssue = hydroModelSpecificIssues[0];
            Assert.That(workflowIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(workflowIssue.Message, 
                Is.EqualTo(string.Format(Resources.HydroModelValidator_ValidateIfModelNamesAreUnique_Two_or_more_activities_in_the_current_workflow_have_the_same_name___0____Please_make_sure_that_these_activity_names_are_uniquely_named_, modelName)));
        }

        [Test]
        public void GivenIntegratedModelWithEquallyNamedModelsInComplexerCurrentWorkflowWhenValidatingThenReturnModelStructureValidationIssue()
        {
            // Setup
            var modelName = "SameName";
            var model1 = mocks.DynamicMock<IActivity>();
            model1.Expect(m => m.Name).Return(modelName).Repeat.Any();

            var model2 = mocks.DynamicMock<IActivity>();
            model2.Expect(m => m.Name).Return(modelName).Repeat.Any();

            var model3 = mocks.DynamicMock<IActivity>();
            model3.Expect(m => m.Name).Return("otherName").Repeat.Any();

            var workflow1 = mocks.DynamicMock<ICompositeActivity>();
            var workflow2 = mocks.DynamicMock<ICompositeActivity>();

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() { model2, model3 })
                .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() { model1, workflow1 })
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow2
            };
            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports.Where(r => r.Category.Contains("HydroModel Specific")).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(1));

            var workflowIssue = hydroModelSpecificIssues[0];
            Assert.That(workflowIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(workflowIssue.Message,
                Is.EqualTo(string.Format(Resources.HydroModelValidator_ValidateIfModelNamesAreUnique_Two_or_more_activities_in_the_current_workflow_have_the_same_name___0____Please_make_sure_that_these_activity_names_are_uniquely_named_, modelName)));
        }

        [Test]
        public void GivenIntegratedModelWithDifferentlyNamedModelsInCurrentWorkflowWhenValidatingThenReturnNoHydroModelSpecificValidationIssues()
        {
            var model1 = mocks.DynamicMock<IActivity>();
            model1.Expect(m => m.Name).Return("name1").Repeat.Any();

            var model2 = mocks.DynamicMock<IActivity>();
            model2.Expect(m => m.Name).Return("name2").Repeat.Any();

            var model3 = mocks.DynamicMock<IActivity>();
            model3.Expect(m => m.Name).Return("name3").Repeat.Any();

            var workflow1 = mocks.DynamicMock<ICompositeActivity>();
            var workflow2 = mocks.DynamicMock<ICompositeActivity>();

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() { model2, model3 })
                .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() { model1, workflow1 })
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow2
            };
            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports.Where(r => r.Category.Contains("HydroModel Specific")).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(0));
        }
    }
}