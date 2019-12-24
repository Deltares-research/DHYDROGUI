using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NSubstitute;
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

            model.Owner = Substitute.For<ICompositeActivity>();
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

        [Test]
        public void GivenWaveModelWithSphericalCoordinateSystemAndWaveSetupIsTrue_WhenWaveDomainIsValidated_ThenCorrectViewDataIsThePhysicalProcessesTab()
        {
            //Given
            var sphericalCoordinateSystemCode = 4326;
            var waveModel = new WaveModel()
            {
                ModelDefinition = { WaveSetup = true},
                OuterDomain = new WaveDomainData("wavedomaindata"),
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(sphericalCoordinateSystemCode)
            };

            //When
            var result = WaveDomainValidator.Validate(waveModel);

            //Then
            var viewData = (WaveValidationShortcut) result.SubReports.ElementAt(0).Issues.ElementAt(0).ViewData;
            Assert.That(viewData.TabName, Is.EqualTo("Physical Processes"));
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
