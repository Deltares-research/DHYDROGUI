using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

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
            ValidationReport validationReport = new WaveModelValidator().Validate(waveModel);

            // Then
            bool outputParameterReportIncluded = validationReport.SubReports.Any(report => report.Category == "Output parameters");
            Assert.IsTrue(outputParameterReportIncluded);
        }

        [Test]
        public void GivenWaveModel_WhenValidatingModel_ThenBoundariesValidationReportIsGenerated()
        {
            // Given
            var waveModel = new WaveModel();

            // When
            ValidationReport validationReport = new WaveModelValidator().Validate(waveModel);

            // Then
            bool boundariesReportIncluded = validationReport.SubReports.Any(report => report.Category == "Waves Model Boundaries");
            Assert.IsTrue(boundariesReportIncluded);
        }

        [Test]
        public void CheckWaveDomainValidation()
        {
            var model = new WaveModel();
            model.OuterDomain.SpectralDomainData.UseDefaultDirectionalSpace = false;
            model.OuterDomain.SpectralDomainData.NDir = 0;
            model.OuterDomain.SpectralDomainData.UseDefaultFrequencySpace = false;
            model.OuterDomain.SpectralDomainData.NFreq = 0;

            ValidationReport validationReport = new WaveModelValidator().Validate(model);

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
            ValidationReport validationReport = new WaveModelValidator().Validate(model);

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
        public void WaveModel_With_OuterDomain_SphericalCoordinates_And_WaveSetupIsTrue_ValidationFails()
        {
            string filePath = TestHelper.GetTestFilePath(@"WaveWithSphericalCoordinates\nonValidModel\d3dfm1125.mdw");
            Assert.IsTrue(File.Exists(filePath));

            string fileCopy = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(fileCopy));

            using (var model = new WaveModel(fileCopy))
            {
                WaveModelProperty waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup);
                waveSetup.Value = true;
                model.ModelDefinition.SetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup, waveSetup);
                waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup);
                Assert.IsTrue((bool) waveSetup.Value);

                Assert.IsTrue(CheckDomainGrid(model.OuterDomain, WaveModel.CoordinateSystemType.Spherical));

                ValidationReport validationReport = WaveDomainValidator.Validate(model);
                Assert.IsTrue(validationReport.AllErrors.Any());

                string expectedMssg = Resources.WaveDomainValidator_ValidateAllDomainsShareCoordinateSystem_WaveSetup_should_be_false_when_using_Spherical_Coordinate_Systems_;
                Assert.IsTrue(validationReport.AllErrors.Any(err => err.Message == expectedMssg));
            }
        }

        [Test]
        public void WaveModel_With_SphericalCoordinates_And_WaveSetupIsFalse_ValidationSucceeds()
        {
            string filePath = TestHelper.GetTestFilePath(@"WaveWithSphericalCoordinates\nonValidModel\d3dfm1125.mdw");
            Assert.IsTrue(File.Exists(filePath));

            string fileCopy = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(fileCopy));

            using (var model = new WaveModel(fileCopy))
            {
                WaveModelProperty waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup);
                waveSetup.Value = false;
                model.ModelDefinition.SetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup, waveSetup);
                waveSetup = model.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup);
                Assert.IsFalse((bool) waveSetup.Value);

                Assert.IsTrue(CheckDomainGrid(model.OuterDomain, WaveModel.CoordinateSystemType.Spherical));

                ValidationReport validationReport = WaveDomainValidator.Validate(model);
                Assert.IsFalse(validationReport.AllErrors.Any());
            }
        }

        [Test]
        public void GivenWaveModelWithSphericalCoordinateSystemAndWaveSetupIsTrue_WhenWaveDomainIsValidated_ThenCorrectViewDataIsThePhysicalProcessesTab()
        {
            //Given
            var sphericalCoordinateSystemCode = 4326;
            var waveModel = new WaveModel()
            {
                ModelDefinition = {WaveSetup = true},
                OuterDomain = new WaveDomainData("wavedomaindata"),
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(sphericalCoordinateSystemCode)
            };

            //When
            ValidationReport result = WaveDomainValidator.Validate(waveModel);

            //Then
            var viewData = (WaveValidationShortcut) result.SubReports.ElementAt(0).Issues.ElementAt(0).ViewData;
            Assert.That(viewData.TabName, Is.EqualTo("Physical Processes"));
        }

        private bool CheckDomainGrid(IWaveDomainData domain, string coordinateSystemName)
        {
            if (domain.Grid == null)
            {
                return false;
            }

            string coordinateSystem;
            if (domain.Grid.Attributes.TryGetValue("CoordinateSystem", out coordinateSystem))
            {
                return coordinateSystem == coordinateSystemName;
            }

            return false;
        }
    }
}