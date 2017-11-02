using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveModelValidatorTest
    {
        [Test]
        public void CheckWaveDomainValidation()
        {
            var model = new WaveModel();
            model.OuterDomain.SpectralDomainData.UseDefaultDirectionalSpace = false;
            model.OuterDomain.SpectralDomainData.NDir = 0;
            model.OuterDomain.SpectralDomainData.UseDefaultFrequencySpace = false;
            model.OuterDomain.SpectralDomainData.NFreq = 0;

            var validationReport = new WaveModelValidator().Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == "Number of directions cannot be zero"));
            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == "Number of frequencies cannot be zero"));
        }

        [Test]
        public void CheckWaveTimePointValidation()
        {
            var model = new WaveModel();
            var validationReport = new WaveModelValidator().Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == "No time points defined"));

            model.IsCoupledToFlow = true;
            validationReport = new WaveModelValidator().Validate(model);
            Assert.IsFalse(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == "No time points defined"));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void CheckWaveCouplingValidationWithoutFlowModel()
        {
            var model = new WaveModel {IsCoupledToFlow = true};
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory,
                        KnownWaveProperties.COMFile).SetValueAsString("../FlowFM_output/");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value = true;
            var validationReport = new WaveModelValidator().Validate(model);
            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == "Coupled wave model must use COM-file"));
        }

        [Test]
        /* Units tests created after DELFT3DFM-510 */
        [TestCase(0.0   , false , InputFieldDataType.TimeVarying, false)]
        [TestCase(0.0, false, InputFieldDataType.Constant, false)]
        [TestCase(0.0, true, InputFieldDataType.TimeVarying, false)]
        [TestCase(0.0, true, InputFieldDataType.Constant, true)]
        [TestCase(30.0, false, InputFieldDataType.TimeVarying, false)]
        [TestCase(30.0, false, InputFieldDataType.Constant, false)]
        [TestCase(30.0, true, InputFieldDataType.TimeVarying, false)]
        [TestCase(30.0, true, InputFieldDataType.Constant, false)]
        [Category(TestCategory.Integration)]
        public void CheckWavePropertiesWithFlowModel(double windSpeed, bool quadruplets, InputFieldDataType windType, bool warningAlert  )
        {
            var model = new WaveModel();
            var reportMessage = "WindSpeed is zero whereas quadruple is true.";
            var reportSeverity = ValidationSeverity.Warning;
            
            /* Assigning variables */
            model.TimePointData.WindSpeedConstant = windSpeed;
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Quadruplets).
                                            Value = quadruplets;
            model.TimePointData.WindDataType = windType;
            
            /*Test*/
            var validationReport = new WaveModelValidator().Validate(model);
            if (warningAlert)
            {
                Assert.IsTrue(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == reportSeverity && i.Message == reportMessage));
            }
            else
            {
                Assert.IsFalse(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == reportSeverity && i.Message == reportMessage));    
            }
        }
    }
}
