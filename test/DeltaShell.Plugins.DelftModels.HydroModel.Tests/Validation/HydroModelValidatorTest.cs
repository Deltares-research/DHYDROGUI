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

            var result = validator.Validate(model);
            Assert.That(result.ErrorCount, Is.EqualTo(1));
            Assert.AreEqual(model.Name + " (Hydro Model)", result.Category);

            var issue = result.AllErrors.ToArray().First();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            Assert.AreEqual(Resources.HydroModelValidator_Validate_Current_Workflow_cannot_be_empty, issue.Message);
        }

        [Test]
        public void GivenIntegratedModelWithIActivityAsCurrentWorkflowWhenValidatingThenReturnNoHydroModelSpecificIssues()
        {
            var hydroModel = new HydroModel();

            var activity = mocks.DynamicMock<IActivity>();
            mocks.ReplayAll();

            var workFlow = new SequentialActivity
            {
                Activities = {activity}
            };
            hydroModel.CurrentWorkflow = workFlow;

            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports
                .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
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
            workflow.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model1, model2})
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow
            };
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

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model2, model3})
                .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model1, workflow1})
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow2
            };
            ValidateAndAssertOnWorkflowValidationReport(hydroModel, equalModelName1);
        }

        private void ValidateAndAssertOnWorkflowValidationReport(HydroModel hydroModel, string equalModelName1)
        {
            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports
                .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(1));

            var workflowIssue = hydroModelSpecificIssues[0];
            Assert.That(workflowIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(workflowIssue.Message,
                Is.EqualTo(string.Format(
                    Resources
                        .HydroModelValidator_ValidateIfModelNamesAreUnique_Two_or_more_activities_in_the_current_workflow_have_the_same_name___0____possibly_only_differing_by_uppercase_letters__Please_make_sure_that_these_activity_names_are_uniquely_named_,
                    equalModelName1.ToLower())));
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

            workflow1.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model2, model3})
                .Repeat.Any();
            workflow2.Expect(ca => ca.Activities).Return(new EventedList<IActivity>() {model1, workflow1})
                .Repeat.Any();
            mocks.ReplayAll();

            var hydroModel = new HydroModel
            {
                CurrentWorkflow = workflow2
            };
            var validationReport = validator.Validate(hydroModel);
            var hydroModelSpecificReport = validationReport.SubReports
                .Where(r => r.Category.Contains(hydroModelSpecificReportName)).ToArray().FirstOrDefault();
            Assert.NotNull(hydroModelSpecificReport);

            var hydroModelSpecificIssues = hydroModelSpecificReport.AllErrors.ToArray();
            Assert.That(hydroModelSpecificIssues.Length, Is.EqualTo(0));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ValidateIntegratedModelFlowFMAndWaveBothModelsHaveTheSameTypeOfGrid()
        {
            var fmModel = new WaterFlowFMModel();
            var waveModel = new WaveModel();
            var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.All);

            PrepareValidFMWavesIntegratedModel(hydroModel, fmModel, waveModel);
            ValidateInconsistentGridTypeErrorInReport(hydroModel, 1);

            //Both FM and Wave models have spherical grids -OK!
            var fmIsSpherical = true;
            var waveIsSpherical = true;
            SetFmGridCoordinateType(fmModel, fmIsSpherical);
            SetWaveGridCoordinateType(waveModel, waveIsSpherical);
            ValidateInconsistentGridTypeErrorInReport(hydroModel, 1);

            //Both FM and Wave models have cartesian grids - OK!
            SetFmGridCoordinateType(fmModel, fmIsSpherical = false);
            SetWaveGridCoordinateType(waveModel, waveIsSpherical = false);
            ValidateInconsistentGridTypeErrorInReport(hydroModel, 1);

            //FM model has spherical grid and Wave model has cartesian grid -NOT OK!
            SetFmGridCoordinateType(fmModel, fmIsSpherical = true);
            SetWaveGridCoordinateType(waveModel, waveIsSpherical = false);
            ValidateInconsistentGridTypeErrorInReport(hydroModel, 2);

            //FM model has cartesian grid and Wave model has spherical grid -NOT OK!
            SetFmGridCoordinateType(fmModel, fmIsSpherical = false);
            SetWaveGridCoordinateType(waveModel, waveIsSpherical = true);
            ValidateInconsistentGridTypeErrorInReport(hydroModel, 2);
        }

        private static void ValidateInconsistentGridTypeErrorInReport(HydroModel hydroModel, int expectedErrorCount)
        {
            var report = hydroModel.Validate();
            Assert.AreEqual(expectedErrorCount, report.ErrorCount);

            string expectedCategory1;
            string expectedReportName1;
            string expectedMsg1;

            string expectedCategory2;
            string expectedReportName2;
            string expectedMsg2;


            if (expectedErrorCount == 0)
            {
                Assert.That(expectedErrorCount > 0);
            }
            else if
                (expectedErrorCount == 1)
            {
                expectedCategory1 = "Waves (Waves Model)";
                expectedReportName1 = DeltaShell.Plugins.FMSuite.Wave.Properties.Resources.WavePropertiesValidator_Validate_Waves_Model_Properties;
                expectedMsg1 = DeltaShell.Plugins.FMSuite.Wave.Properties.Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_;

                var generalReport1 = report.SubReports.FirstOrDefault(sr => sr.Category == expectedCategory1 && sr.ErrorCount == 1);
                Assert.NotNull(generalReport1);

                var subReport1 = generalReport1.SubReports.FirstOrDefault(sr => sr.Category == expectedReportName1 && sr.ErrorCount == 1);
                Assert.NotNull(subReport1);

                var errorFound1 = subReport1.AllErrors.FirstOrDefault(err => err.Message == expectedMsg1);
                Assert.NotNull(errorFound1);
            }
            else if
                (expectedErrorCount == 2)
            {
                expectedCategory1 = "Waves (Waves Model)";
                expectedReportName1 = DeltaShell.Plugins.FMSuite.Wave.Properties.Resources.WavePropertiesValidator_Validate_Waves_Model_Properties;
                expectedMsg1 = DeltaShell.Plugins.FMSuite.Wave.Properties.Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_;

                expectedCategory2 = Resources.HydroModelValidator_Validate_HydroModel_Specific;
                expectedReportName2 = Resources.HydroModelValidator_ConstructModelGridReport_Grid_Coordinate_System_type;
                expectedMsg2 = Resources.HydroModelValidator_ConstructModelGridReport_Wave_and_WaterFlowFM_Grids_need_to_be_of_the_same_type__either_Spherical_or_Cartesian__;

                var generalReport1 = report.SubReports.FirstOrDefault(sr => sr.Category == expectedCategory1 && sr.ErrorCount == 1);
                Assert.NotNull(generalReport1);

                var subReport1 = generalReport1.SubReports.FirstOrDefault(sr => sr.Category == expectedReportName1 && sr.ErrorCount == 1);
                Assert.NotNull(subReport1);

                var errorFound1 = subReport1.AllErrors.FirstOrDefault(err => err.Message == expectedMsg1);
                Assert.NotNull(errorFound1);

                var generalReport2 = report.SubReports.FirstOrDefault(sr => sr.Category == expectedCategory2 && sr.ErrorCount == 1);
                Assert.NotNull(generalReport2);

                var subReport2 = generalReport2.SubReports.FirstOrDefault(sr => sr.Category == expectedReportName2 && sr.ErrorCount == 1);
                Assert.NotNull(subReport2);

                var errorFound2 = subReport2.AllErrors.FirstOrDefault(err => err.Message == expectedMsg2);
                Assert.NotNull(errorFound2);
            }
        }

        private static void SetWaveGridCoordinateType(WaveModel waveModel, bool isSpherical = false)
        {
            waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(isSpherical ? 4326 : 28992); //4326 = WGS84, Spherical system.

            var waveGrid = waveModel.OuterDomain.Grid;
            Assert.NotNull(waveGrid);
            Assert.AreEqual(waveModel.CoordinateSystem, waveGrid.CoordinateSystem);
            Assert.AreEqual(isSpherical, waveGrid.CoordinateSystem.IsGeographic);
        }

        private static void SetFmGridCoordinateType(WaterFlowFMModel fmModel, bool isSpherical = false)
        {
            fmModel.Grid.Clear();
            fmModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG( isSpherical ? 4326 : 28992); //4326 = WGS84, Spherical system.
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
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value = true;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile).Value = "file.txt";
            waveModel.GetFlowComFilePath = () => "";

            /*FM Model*/
            fmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = fmModel.TimeStep;
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = fmModel.TimeStep;

            /*Integrated Model*/
            waveModel.ModelDefinition.ModelReferenceDateTime = hydroModel.StartTime;
        }

        private static CurvilinearGrid CreateWaveModelOuterDomainGrid()
        {
            int mSize = 2;
            int nSize = 3;
            var xCoordinates = new[] {0.1, 2.0, 0.1, 2.0, 0.1, 2.0};
            var yCoordinates = new[] {1.0, 1.0, 3.0, 3.0, 5.0, 5.0};

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