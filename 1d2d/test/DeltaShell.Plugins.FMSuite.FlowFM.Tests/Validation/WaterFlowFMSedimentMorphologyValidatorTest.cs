using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture()]
    public class WaterFlowFMSedimentMorphologyValidatorTest
    {

        [Test]
        public void ValidateSedimentNameTest()
        {
            var validName = "Sediment_001";
            var issue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(validName);
            Assert.IsNull(issue);
            var invalidName = "Sediment#001";
            issue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(invalidName);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void ValidateMorphologyBetaWarningTest()
        {
            var model = new WaterFlowFMModel();
            model.SedimentFractions.Add(new SedimentFraction {Name = "SedFrac"});
            var report = model.Validate();
            var morReport = report.SubReports.FirstOrDefault(r => r.Category.Contains("Morphology / Sediment Beta warning"));
            Assert.IsNull(morReport);

            model.ModelDefinition.UseMorphologySediment = true;
            report = model.Validate();
            morReport = report.SubReports.FirstOrDefault(r => r.Category.Contains("Morphology / Sediment Beta warning"));
            Assert.AreEqual(0, morReport.AllErrors.Count());
            Assert.IsNotNull(morReport);
            var betaWarningIssue = morReport.GetAllIssuesRecursive().FirstOrDefault(i => i.Severity == ValidationSeverity.Warning);
            Assert.IsNotNull(betaWarningIssue);

            Assert.That(betaWarningIssue.Message.Contains("Morphology is beta version"));
        }

        [Test]
        public void Test_ValidateWithMorpohlogyBetaWarning_WithoutSediments_Returns_ValidationIssue_With_ExpectedMessage()
        {
            var model = new WaterFlowFMModel() {ModelDefinition = {UseMorphologySediment = true}};
            var expectedMessage = Resources
                .WaterFlowFMSedimentMorphologyValidator_ValidateAtLeastOneSedimentFractionInModel_At_least_one_sediment_fraction_is_required_when_using_morphology;

            var validationReport = WaterFlowFMSedimentMorphologyValidator.ValidateWithMorphologyBetaWarning(model);
            var errorMessages = validationReport.AllErrors.Where(i => i.Message == expectedMessage).Select(i => i.Message);
            Assert.AreEqual(errorMessages.Count(), 1);
        }

        [Test]
        public void Test_ValidateWithMorpohlogyBetaWarning_WithSediments_Returns_No_ValidationIssue()
        {
            var model = new WaterFlowFMModel() { ModelDefinition = { UseMorphologySediment = true } };
            var expectedMessage = Resources
                .WaterFlowFMSedimentMorphologyValidator_ValidateAtLeastOneSedimentFractionInModel_At_least_one_sediment_fraction_is_required_when_using_morphology;

            model.SedimentFractions.Add(new SedimentFraction() { Name = "SedFrac" });

            var validationReport = WaterFlowFMSedimentMorphologyValidator.ValidateWithMorphologyBetaWarning(model);
            var errorMessages = validationReport.AllErrors.Where(i => i.Message == expectedMessage).Select(i => i.Message);
            Assert.AreEqual(errorMessages.Count(), 0);
        }

        [Test]
        public void TestValidateInitialSedimentThicknessOfSedimentFractionsInModel_WithNoSedimentFractions()
        {
            var fmModel = new WaterFlowFMModel { ModelDefinition = { UseMorphologySediment = true } };
            var issues = GetValidationIssuesWithMessages(fmModel, new List<string>(){ Resources.WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness});
            Assert.AreEqual(0, issues.Count());
        }
        
        [Test]
        public void TestValidateInitialSedimentThicknessOfSedimentFractionsInModel_WithSedimentFractionWithInitialSedimentThicknessGreaterThanZero()
        {
            var fmModel = GetFMModelWithDefaultSandAndMudFractions();
            fmModel.SedimentFractions.ForEach(
                sf => sf.CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                .First(p => p.Name == "IniSedThick").Value = 1);

            var issues = GetValidationIssuesWithMessages(fmModel, new List<string>() { Resources.WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness });
            Assert.AreEqual(0, issues.Count());
        }

        [Test]
        public void TestValidateInitialSedimentThicknessOfSedimentFractionsInModel_WithOneOfTheSedimentFractionsWithInitialSedimentThicknessGreaterThanZero()
        {
            var fmModel = GetFMModelWithDefaultSandAndMudFractions();
            Assert.AreEqual(2, fmModel.SedimentFractions.Count);
            
            fmModel.SedimentFractions[0].CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                .First(p => p.Name == "IniSedThick").Value = 0;

            fmModel.SedimentFractions[1].CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                .First(p => p.Name == "IniSedThick").Value = 1;

            var issues = GetValidationIssuesWithMessages(fmModel, new List<string>() { Resources.WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness });
            Assert.AreEqual(0, issues.Count());
        }

        [Test]
        public void TestValidateInitialSedimentThicknessOfSedimentFractionsInModel_WithNoSedimentFractionsWithInitialSedimentThicknessGreaterThanZero()
        {
            var fmModel = GetFMModelWithDefaultSandAndMudFractions();
            fmModel.SedimentFractions.ForEach(
                sf => sf.CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                .First(p => p.Name == "IniSedThick").Value = 0);

            var issues = GetValidationIssuesWithMessages(fmModel, new List<string>() { Resources.WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness });
            Assert.AreEqual(1, issues.Count());
        }

        [Test]
        public void GivenAProjectWithNonInterpolatedInitialThicknessSediment_WhenValidating_ThenWarningMessageAppears()
        {
            var spatiallyVaryingNames = new List<string>
            {
                "Sediment_sand_IniSedThick"
            };

            var sedimentProperties = new EventedList<ISedimentProperty>()
            {
                new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 0, 0, false, double.MaxValue, true, "m","Initial sediment layer thickness at bed", true, false)
                {
                    SpatiallyVaryingName = spatiallyVaryingNames[0]
                }
            };
            var fmModel = GetFmModelWithSedimentFraction(sedimentProperties);
            SetDataItemValueConverters(fmModel, spatiallyVaryingNames);

            var messages = spatiallyVaryingNames.Select(n => string.Format(
                Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                n)).ToList();
            var issues = GetValidationIssuesWithMessages(fmModel, messages);
            Assert.That(issues.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GivenAProjectWithSedimentConcentrationSedimentFraction_WhenValidating_ThenNoWarningMessageAppears()
        {
            var spatiallyVaryingNames = new List<string>
            {
                "Sediment_sand_SedConc"
            };

            var sedimentProperties = new EventedList<ISedimentProperty>()
            {
                new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, double.MaxValue, true, "kg/m³",
                    "Initial Concentration", true, false, sediments => false)
                {
                    SpatiallyVaryingName = spatiallyVaryingNames[0]
                }
            };
            var fmModel = GetFmModelWithSedimentFraction(sedimentProperties);
            SetDataItemValueConverters(fmModel, spatiallyVaryingNames);

            var messages = spatiallyVaryingNames.Select(n => string.Format(
                Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                n)).ToList();
            var issues = GetValidationIssuesWithMessages(fmModel, messages);
            Assert.That(issues.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GivenAProjectWithnOSpatiallyVaryingSedimentProperties_WhenValidating_ThenWarningMessageAppears()
        {
            var spatiallyVaryingNames = new List<string>
            {
                "Sediment_sand_IniSedThick"
            };

            var sedimentProperties = new EventedList<ISedimentProperty>()
            {
                new SedimentProperty<double>("IniSedThick", 0, 0, false, double.MaxValue, true, "m","Initial sediment layer thickness at bed", false)
            };
            var fmModel = GetFmModelWithSedimentFraction(sedimentProperties);
            SetDataItemValueConverters(fmModel, spatiallyVaryingNames);

            var messages = spatiallyVaryingNames.Select(n => string.Format(
                Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                n)).ToList();
            var issues = GetValidationIssuesWithMessages(fmModel, messages);
            Assert.That(issues.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GivenAProjectWithTwoSpatiallyVaryingSedimentProperties_WhenValidating_ThenTwoWarningMessagesAppears()
        {
            var spatiallyVaryingNames = new List<string>
            {
                "Sediment_sand_IniSedThick",
                "Sediment_sand_IniSedThick2"
            };

            var sedimentProperties = new EventedList<ISedimentProperty>()
            {
                new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 0, 0, false, double.MaxValue, true, "m","Initial sediment layer thickness at bed", true, false)
                {
                    SpatiallyVaryingName = spatiallyVaryingNames[0]
                },
                new SpatiallyVaryingSedimentProperty<double>("IniSedThick2", 0, 0, false, double.MaxValue, true, "m","Initial sediment layer thickness at bed", true, false)
                {
                    SpatiallyVaryingName = spatiallyVaryingNames[1]
                }
            };
            var fmModel = GetFmModelWithSedimentFraction(sedimentProperties);
            SetDataItemValueConverters(fmModel, spatiallyVaryingNames);

            var messages = spatiallyVaryingNames.Select(n => string.Format(
                Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                n)).ToList();
            var issues = GetValidationIssuesWithMessages(fmModel, messages);
            Assert.That(issues.Count(), Is.EqualTo(2));
        }

        #region Test helper methods
        private static WaterFlowFMModel GetFMModelWithDefaultSandAndMudFractions()
        {
            var fmModel = new WaterFlowFMModel()
            {
                ModelDefinition = { UseMorphologySediment = true },
                SedimentFractions = new EventedList<ISedimentFraction>()
                {
                    new SedimentFraction
                    {
                        Name = "Sand",
                        CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes().FirstOrDefault(st => st.Name == "Sand"),
                    },
                    new SedimentFraction
                    {
                        Name = "Mud",
                        CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes().FirstOrDefault(st => st.Name == "Mud"),
                    }
                }
            };

            return fmModel;
        }

        private static WaterFlowFMModel GetFmModelWithSedimentFraction(IEventedList<ISedimentProperty> sedimentProperties)
        {
            var fmModel = new WaterFlowFMModel {ModelDefinition = {UseMorphologySediment = true}};
            var sedimentFraction = new SedimentFraction
            {
                Name = "Sand",
                CurrentSedimentType = new SedimentType
                {
                    Properties = sedimentProperties
                }
            };
            fmModel.SedimentFractions.Add(sedimentFraction);
            return fmModel;
        }

        private static void SetDataItemValueConverters(WaterFlowFMModel fmModel, List<string> spatiallyVaryingNames)
        {
            var iniSedThickDataItems = fmModel.AllDataItems.Where(d => spatiallyVaryingNames.Contains(d.Name));
            foreach (var iniSedThickDataItem in iniSedThickDataItems)
            {
                var valueConverter =
                    SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(iniSedThickDataItem);
                valueConverter.SpatialOperationSet.AddOperation(new SetValueOperation());
            }
        }

        private static IEnumerable<ValidationIssue> GetValidationIssuesWithMessages(WaterFlowFMModel fmModel, List<string> messages)
        {
            var validationReport = fmModel.Validate();
            var morSedValidationReport =
                validationReport.SubReports.FirstOrDefault(r => r.Category == Resources.WaterFlowFMSedimentMorphologyValidator_ValidateMorphologyBetaWarning_Morphology___Sediment_Beta_warning);
            Assert.NotNull(morSedValidationReport);

            var issues = new List<ValidationIssue>();
            foreach (var message in messages)
            {
                issues.AddRange(morSedValidationReport.Issues.Where(i => i.Message.Equals(message)));
            }
            
            return issues;
        }
        #endregion
        
    }
}