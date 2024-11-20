using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class DomainSpecificSettingsViewModelTest
    {
        private WaveDomainData domainData;
        private DomainSpecificSettingsViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            domainData = new WaveDomainData("domain_name");
            viewModel = new DomainSpecificSettingsViewModel(domainData);
        }

        [Test]
        public void GetDomainName_ReturnsCorrectValue()
        {
            Assert.That(viewModel.DomainName, Is.EqualTo("domain_name"));
        }

        [Test]
        public void SetDirectionalSpaceSettings_WithSameValue_PropertyChangedIsNotFired()
        {
            // Setup
            DirectionalSpaceSettingsViewModel settings = viewModel.DirectionalSpaceSettings;

            // Call
            void Call() => viewModel.DirectionalSpaceSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 0, Call);
            Assert.That(viewModel.DirectionalSpaceSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetDirectionalSpaceSettings_WithNewValue_ThenPropertyIsSetCorrectlyAndPropertyChangedIsFiredOnce()
        {
            // Setup
            var settings = new DirectionalSpaceSettingsViewModel(null);

            // Call
            void Call() => viewModel.DirectionalSpaceSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 1, Call);
            Assert.That(viewModel.DirectionalSpaceSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetFrequencySpaceSettings_WithSameValue_PropertyChangedIsNotFired()
        {
            // Setup
            FrequencySpaceSettingsViewModel settings = viewModel.FrequencySpaceSettings;

            // Call
            void Call() => viewModel.FrequencySpaceSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 0, Call);
            Assert.That(viewModel.FrequencySpaceSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetFrequencySpaceSettings_WithNewValue_ThenPropertyIsSetCorrectlyAndPropertyChangedIsFiredOnce()
        {
            // Setup
            var settings = new FrequencySpaceSettingsViewModel(null);

            // Call
            void Call() => viewModel.FrequencySpaceSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 1, Call);
            Assert.That(viewModel.FrequencySpaceSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetHydroDynamicsSettings_WithSameValue_PropertyChangedIsNotFired()
        {
            // Setup
            HydroDynamicsSettingsViewModel settings = viewModel.HydroDynamicsSettings;

            // Call
            void Call() => viewModel.HydroDynamicsSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 0, Call);
            Assert.That(viewModel.HydroDynamicsSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetHydroDynamicsSettings_WithNewValue_ThenPropertyIsSetCorrectlyAndPropertyChangedIsFiredOnce()
        {
            // Setup
            var settings = new HydroDynamicsSettingsViewModel(null);

            // Call
            void Call() => viewModel.HydroDynamicsSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 1, Call);
            Assert.That(viewModel.HydroDynamicsSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetWindSettings_WithSameValue_PropertyChangedIsNotFired()
        {
            // Setup
            WindSettingsViewModel settings = viewModel.WindSettings;

            // Call
            void Call() => viewModel.WindSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 0, Call);
            Assert.That(viewModel.WindSettings, Is.EqualTo(settings));
        }

        [Test]
        public void SetWindSettings_WithNewValue_ThenPropertyIsSetCorrectlyAndPropertyChangedIsFiredOnce()
        {
            // Setup
            var settings = new WindSettingsViewModel(null);

            // Call
            void Call() => viewModel.WindSettings = settings;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(viewModel, 1, Call);
            Assert.That(viewModel.WindSettings, Is.EqualTo(settings));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetUseCustomDirectionalSpaceSettings_ReturnsCorrectValue(bool value)
        {
            // Setup
            domainData.SpectralDomainData.UseDefaultDirectionalSpace = value;

            // Call
            bool resultedValue = viewModel.UseCustomDirectionalSpaceSettings;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(!value));
        }

        [TestCase(true, true, 0)]
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 1)]
        public void SetUseCustomDirectionalSpaceSettings_SetsCorrectPropertyValueOnModel(bool originalValue, bool setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.UseCustomDirectionalSpaceSettings = originalValue;

            // Call
            void Call() => viewModel.UseCustomDirectionalSpaceSettings = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.UseCustomDirectionalSpaceSettings));
            Assert.That(domainData.SpectralDomainData.UseDefaultDirectionalSpace, Is.EqualTo(!setValue));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetUseCustomFrequencySpaceSettings_ReturnsCorrectValue(bool value)
        {
            // Setup
            domainData.SpectralDomainData.UseDefaultFrequencySpace = value;

            // Call
            bool resultedValue = viewModel.UseCustomFrequencySpaceSettings;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(!value));
        }

        [TestCase(true, true, 0)]
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 1)]
        public void SetUseCustomFrequencySpaceSettings_SetsCorrectPropertyValueOnModel(bool originalValue, bool setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.UseCustomFrequencySpaceSettings = originalValue;

            // Call
            void Call() => viewModel.UseCustomFrequencySpaceSettings = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.UseCustomFrequencySpaceSettings));
            Assert.That(domainData.SpectralDomainData.UseDefaultFrequencySpace, Is.EqualTo(!setValue));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetUseCustomHydroDynamicsSettings_ReturnsCorrectValue(bool value)
        {
            // Setup
            domainData.HydroFromFlowData.UseDefaultHydroFromFlowSettings = value;

            // Call
            bool resultedValue = viewModel.UseCustomHydroDynamicsSettings;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(!value));
        }

        [TestCase(true, true, 0)]
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 1)]
        public void SetUseCustomHydroDynamicsSettings_SetsCorrectPropertyValueOnModel(bool originalValue, bool setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.UseCustomHydroDynamicsSettings = originalValue;

            // Call
            void Call() => viewModel.UseCustomHydroDynamicsSettings = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.UseCustomHydroDynamicsSettings));
            Assert.That(domainData.HydroFromFlowData.UseDefaultHydroFromFlowSettings, Is.EqualTo(!setValue));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetUseCustomWindSettings_ReturnsCorrectValue(bool value)
        {
            // Setup
            domainData.UseGlobalMeteoData = value;

            // Call
            bool resultedValue = viewModel.UseCustomWindSettings;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(!value));
        }

        [TestCase(true, true, 0)]
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 1)]
        public void SetUseCustomWindSettings_SetsCorrectPropertyValueOnModel(bool originalValue, bool setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.UseCustomWindSettings = originalValue;

            // Call
            void Call() => viewModel.UseCustomWindSettings = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.UseCustomWindSettings));
            Assert.That(domainData.UseGlobalMeteoData, Is.EqualTo(!setValue));
        }
    }
}