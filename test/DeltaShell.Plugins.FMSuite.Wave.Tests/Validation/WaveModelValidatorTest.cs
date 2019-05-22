using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveModelValidatorTest
    {
        [Test]
        public void GivenWaveModel_WhenValidatingModel_ThenOutputParametersValidationReportIsGenerated()
        {
            // Given
            var waveModel = new WaveModel();

            // When
            var validationReport = new WaveModelValidator().Validate(waveModel);

            // Then
            var outputParameterReportIncluded = validationReport.SubReports.Any(report => report.Category == "Output parameters");
            Assert.IsTrue(outputParameterReportIncluded);
        }

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
                            i.Severity == ValidationSeverity.Error && i.Message == Resources.WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file));
        }

        
        [Test]
        public void WaveModel_With_OuterDomain_SphericalCoordinates_And_WaveSetupIsTrue_ValidationFails()
        {
            var filePath = TestHelper.GetTestFilePath(@"WaveWithSphericalCoordinates\nonValidModel\d3dfm1125.mdw");
            Assert.IsTrue(File.Exists(filePath));

            var fileCopy = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(fileCopy));

            using (var model = new WaveModel(fileCopy))
            {
                var waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup);
                waveSetup.Value = true;
                model.ModelDefinition.SetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup, waveSetup);
                waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup);
                Assert.IsTrue((bool)waveSetup.Value);

                Assert.IsTrue(CheckDomainGrid(model.OuterDomain, WaveModel.CoordinateSystemType.Spherical));

                var validationReport = WaveDomainValidator.Validate(model);
                Assert.IsTrue(validationReport.AllErrors.Any());

                var expectedMssg = Resources.WaveDomainValidator_ValidateAllDomainsShareCoordinateSystem_WaveSetup_should_be_false_when_using_Spherical_Coordinate_Systems_;
                Assert.IsTrue(validationReport.AllErrors.Any( err => err.Message == expectedMssg));
            }
        }

        [Test]
        public void WaveModel_With_SphericalCoordinates_And_WaveSetupIsFalse_ValidationSucceeds()
        {
            var filePath = TestHelper.GetTestFilePath(@"WaveWithSphericalCoordinates\nonValidModel\d3dfm1125.mdw");
            Assert.IsTrue(File.Exists(filePath));

            var fileCopy = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(fileCopy));

            using (var model = new WaveModel(fileCopy))
            {
                var waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup);
                waveSetup.Value = false;
                model.ModelDefinition.SetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup, waveSetup);
                waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.WaveSetup);
                Assert.IsFalse((bool)waveSetup.Value);

                Assert.IsTrue(CheckDomainGrid(model.OuterDomain, WaveModel.CoordinateSystemType.Spherical));

                var validationReport = WaveDomainValidator.Validate(model);
                Assert.IsFalse(validationReport.AllErrors.Any());
            }
        }

        private bool CheckDomainGrid(WaveDomainData domain, string coordinateSystemName)
        {
            if (domain.Grid == null) return false;

            string coordinateSystem;
            if (domain.Grid.Attributes.TryGetValue("CoordinateSystem", out coordinateSystem))
                return coordinateSystem == coordinateSystemName;
            return false;
        }
    }
}
