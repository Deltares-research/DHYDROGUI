using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class FrequencySpaceSettingsViewModelTest
    {
        private SpectralDomainData domainData;
        private FrequencySpaceSettingsViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            domainData = new SpectralDomainData();
            viewModel = new FrequencySpaceSettingsViewModel(domainData);
        }

        [Test]
        public void GetNrOfFrequencies_ReturnsCorrectValue()
        {
            // Setup
            const int value = 7;
            domainData.NFreq = value;

            // Call
            int resultedValue = viewModel.NrOfFrequencies;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetStartFrequency_ReturnsCorrectValue()
        {
            // Setup
            const double value = 7;
            domainData.FreqMin = value;

            // Call
            double resultedValue = viewModel.StartFrequency;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetEndFrequency_ReturnsCorrectValue()
        {
            // Setup
            const double value = 7;
            domainData.FreqMax = value;

            // Call
            double resultedValue = viewModel.EndFrequency;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [TestCase(7, 0)]
        [TestCase(5, 1)]
        public void SetNrOfFrequencies_SetsCorrectPropertyValueOnModel(int setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.NrOfFrequencies = 7;

            // Call
            void Call() => viewModel.NrOfFrequencies = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.NrOfFrequencies));
            Assert.That(domainData.NFreq, Is.EqualTo(setValue));
        }

        [TestCase(7.5, 0)]
        [TestCase(5.5, 1)]
        public void SetStartFrequency_SetsCorrectPropertyValueOnModel(double setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.StartFrequency = 7.5;

            // Call
            void Call() => viewModel.StartFrequency = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.StartFrequency));
            Assert.That(domainData.FreqMin, Is.EqualTo(setValue));
        }

        [TestCase(7.5, 0)]
        [TestCase(5.5, 1)]
        public void SetEndFrequency_SetsCorrectPropertyValueOnModel(double setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.EndFrequency = 7.5;

            // Call
            void Call() => viewModel.EndFrequency = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.EndFrequency));
            Assert.That(domainData.FreqMax, Is.EqualTo(setValue));
        }
    }
}