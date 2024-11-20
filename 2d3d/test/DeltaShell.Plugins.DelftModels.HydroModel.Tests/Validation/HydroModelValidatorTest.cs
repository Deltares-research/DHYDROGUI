using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.HydroModel.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Validation
{
    [TestFixture]
    public class HydroModelValidatorTest
    {
        private MockRepository mocks;
        private HydroModelValidator validator;
        private string hydroModelSpecificReportName;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            validator = new HydroModelValidator();
            hydroModelSpecificReportName = Resources.HydroModelValidator_Validate_HydroModel_Specific;
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

            ValidationReport result = validator.Validate(model);
            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.AreEqual(model.Name + " (Hydro Model)", result.Category);

            ValidationIssue issue = result.AllErrors.ToArray().First();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            Assert.AreEqual(Resources.HydroModelValidator_Validate_Current_Workflow_cannot_be_empty, issue.Message);
        }

        [Test]
        public void GivenIntegratedModelWithIActivityAsCurrentWorkflowWhenValidatingThenReturnNoHydroModelSpecificIssues()
        {
            var hydroModel = new HydroModel();

            var activity = mocks.DynamicMock<IActivity>();
            mocks.ReplayAll();

            var workFlow = new SequentialActivity {Activities = {activity}};
            hydroModel.CurrentWorkflow = workFlow;

            ValidationReport validationReport = validator.Validate(hydroModel);
            ValidationReport hydroModelSpecificReport = validationReport.SubReports
                                                                        .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            ValidationIssue[] hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(0));
        }

        [Test]
        [TestCase("SameName", "SameName")]
        [TestCase("samename", "SAMENAME")]
        public void GivenIntegratedModelWithEquallyNamedModelsInCurrentWorkflowWhenValidatingThenReturnModelStructureValidationIssue(string equalModelName1, string equalModelName2)
        {
            var model1 = mocks.DynamicMock<IActivity>();
            model1.Expect(m => m.Name).Return(equalModelName1).Repeat.Any();

            var model2 = mocks.DynamicMock<IActivity>();
            model2.Expect(m => m.Name).Return(equalModelName2).Repeat.Any();

            var workflow = mocks.DynamicMock<ICompositeActivity>();
            workflow.Expect(ca => ca.Activities).Return(new EventedList<IActivity>()
                    {
                        model1,
                        model2
                    })
                    .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel {CurrentWorkflow = workflow};
            ValidateAndAssertOnWorkflowValidationReport(hydroModel, equalModelName1);
        }

        [Test]
        [TestCase("SameName", "SameName")]
        [TestCase("samename", "SAMENAME")]
        public void GivenIntegratedModelWithEquallyNamedModelsInComplexerCurrentWorkflowWhenValidatingThenReturnModelStructureValidationIssue(string equalModelName1, string equalModelName2)
        {
            var model1 = mocks.DynamicMock<IActivity>();
            model1.Expect(m => m.Name).Return(equalModelName1).Repeat.Any();

            var model2 = mocks.DynamicMock<IActivity>();
            model2.Expect(m => m.Name).Return(equalModelName2).Repeat.Any();

            var model3 = mocks.DynamicMock<IActivity>();
            model3.Expect(m => m.Name).Return("otherName").Repeat.Any();

            var workflow1 = mocks.DynamicMock<ICompositeActivity>();
            var workflow2 = mocks.DynamicMock<ICompositeActivity>();

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>()
                     {
                         model2,
                         model3
                     })
                     .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>()
                     {
                         model1,
                         workflow1
                     })
                     .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel {CurrentWorkflow = workflow2};
            ValidateAndAssertOnWorkflowValidationReport(hydroModel, equalModelName1);
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

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>()
                     {
                         model2,
                         model3
                     })
                     .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>()
                     {
                         model1,
                         workflow1
                     })
                     .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel {CurrentWorkflow = workflow2};
            ValidationReport validationReport = validator.Validate(hydroModel);
            ValidationReport hydroModelSpecificReport = validationReport.SubReports
                                                                        .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            ValidationIssue[] hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(0));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [TestCase(true, true, 0)]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(false, false, 0)]
        public void ValidateIntegratedModelFlowFmAndWaveBothModelsHaveTheSameTypeOfGrid(bool fmIsSpherical, bool waveIsSpherical, int amountOfErrors)
        {
            // Arrange
            using (var fmModel = new WaterFlowFMModel())
            using (var waveModel = new WaveModel())
            using (HydroModel hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.All))
            {
                PrepareValidFMWavesIntegratedModel(hydroModel, fmModel, waveModel);
                SetFmGridCoordinateType(fmModel, fmIsSpherical);
                SetWaveGridCoordinateType(waveModel, waveIsSpherical);

                const string expectedMessage =
                    "Wave model and FlowFM model, have grids with a different coordinate system . These coordinate systems have to be of the same type (Cartesian or spherical) to run the integrated model";

                // Act
                ValidationReport report = hydroModel.Validate();

                // Assert
                IEnumerable<ValidationIssue> errors = report.AllErrors.Where(error => error.Message == expectedMessage);
                Assert.That(errors.Count(), Is.EqualTo(amountOfErrors));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenEmptyWorkflowHydroModel_WhenValidate_ThenReportsCurrentWorkflowError()
        {
            // Given
            var hydroModel = new HydroModel();
            Assert.That(hydroModel.CurrentWorkflow, Is.Null);

            // When
            ValidationReport validationReport = hydroModel.Validate();

            // Then
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            ValidationIssue generatedError = validationReport.AllErrors.ToArray()[0];
            Assert.That(generatedError.Subject, Is.EqualTo(hydroModel));
            Assert.That(generatedError.Message, Is.EqualTo(Resources.HydroModelValidator_Validate_Current_Workflow_cannot_be_empty));
        }

        private void ValidateAndAssertOnWorkflowValidationReport(HydroModel hydroModel, string equalModelName1)
        {
            ValidationReport validationReport = validator.Validate(hydroModel);
            ValidationReport hydroModelSpecificReport = validationReport.SubReports
                                                                        .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            ValidationIssue[] hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(1));

            ValidationIssue workflowIssue = hydroModelSpecificIssues[0];
            Assert.That(workflowIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(workflowIssue.Message,
                        Is.EqualTo(string.Format(
                                       Resources
                                           .HydroModelValidator_ValidateIfModelNamesAreUnique_Two_or_more_activities_in_the_current_workflow_have_the_same_name___0____possibly_only_differing_by_uppercase_letters__Please_make_sure_that_these_activity_names_are_uniquely_named_,
                                       equalModelName1.ToLower())));
        }

        private static void SetWaveGridCoordinateType(WaveModel waveModel, bool isSpherical = false)
        {
            waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(isSpherical ? 4326 : 28992); //4326 = WGS84, Spherical system.

            CurvilinearGrid waveGrid = waveModel.OuterDomain.Grid;
            Assert.NotNull(waveGrid);
            Assert.AreEqual(waveModel.CoordinateSystem, waveGrid.CoordinateSystem);
            Assert.AreEqual(isSpherical, waveGrid.CoordinateSystem.IsGeographic);
        }

        private static void SetFmGridCoordinateType(WaterFlowFMModel fmModel, bool isSpherical = false)
        {
            fmModel.Grid.Clear();
            fmModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(isSpherical ? 4326 : 28992); //4326 = WGS84, Spherical system.
            fmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(20, 20, 20, 20);

            Assert.NotNull(fmModel.Grid);
            Assert.AreEqual(fmModel.CoordinateSystem, fmModel.Grid.CoordinateSystem);
            Assert.AreEqual(isSpherical, fmModel.Grid.CoordinateSystem.IsGeographic);
        }

        private void PrepareValidFMWavesIntegratedModel(HydroModel hydroModel, IWaterFlowFMModel fmModel, WaveModel waveModel)
        {
            hydroModel.Activities.Clear();

            Assert.NotNull(waveModel);
            hydroModel.Activities.Add(fmModel);

            Assert.NotNull(fmModel);
            hydroModel.Activities.Add(waveModel);

            Assert.IsNotEmpty(hydroModel.Activities);

            /* Wave Model */
            waveModel.OuterDomain.Grid = CreateWaveModelOuterDomainGrid();
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.WriteCOM).Value = true;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.COMFile).Value = "file.txt";

            /*FM Model*/
            fmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = fmModel.TimeStep;
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = fmModel.TimeStep;

            /*Integrated Model*/
            waveModel.ModelDefinition.ModelReferenceDateTime = hydroModel.StartTime;
        }

        private static CurvilinearGrid CreateWaveModelOuterDomainGrid()
        {
            var mSize = 2;
            var nSize = 3;
            var xCoordinates = new[]
            {
                0.1,
                2.0,
                0.1,
                2.0,
                0.1,
                2.0
            };
            var yCoordinates = new[]
            {
                1.0,
                1.0,
                3.0,
                3.0,
                5.0,
                5.0
            };

            //         0          1    = M
            //      
            //   0   (0.1,1)------(2,1)
            //         |          |
            //         |          |
            //   1   (0.1,3)------(2,3)
            //         |          |
            //         |          |
            //   2   (0.1,5)------(2,5)
            //
            //  = N

            var grid = CurvilinearGrid.CreateDefault();
            grid.Resize(nSize, mSize, xCoordinates, yCoordinates);
            grid.IsTimeDependent = false;
            return grid;
        }
    }
}