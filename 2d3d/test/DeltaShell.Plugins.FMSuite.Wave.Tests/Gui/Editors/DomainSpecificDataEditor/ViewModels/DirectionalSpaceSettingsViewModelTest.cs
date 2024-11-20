using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class DirectionalSpaceSettingsViewModelTest
    {
        private SpectralDomainData domainData;
        private DirectionalSpaceSettingsViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            domainData = new SpectralDomainData();
            viewModel = new DirectionalSpaceSettingsViewModel(domainData);
        }

        [Test]
        public void SetTypeToCircle_ThenStartDirectionAndEndDirectionAreCorrectlySet()
        {
            // Setup
            viewModel.Type = DirectionalSpaceType.Sector;
            viewModel.StartDirection = 3;
            viewModel.EndDirection = 3;

            // Call
            viewModel.Type = DirectionalSpaceType.Circle;

            // Assert
            Assert.That(viewModel.StartDirection, Is.EqualTo(0));
            Assert.That(viewModel.EndDirection, Is.EqualTo(360));
        }

        [Test]
        public void GetNrOfDirections_ReturnsCorrectValue()
        {
            // Setup
            const int value = 7;
            domainData.NDir = value;

            // Call
            int resultedValue = viewModel.NrOfDirections;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetStartDirection_ReturnsCorrectValue()
        {
            // Setup
            const double value = 7;
            domainData.StartDir = value;

            // Call
            double resultedValue = viewModel.StartDirection;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetEndDirection_ReturnsCorrectValue()
        {
            // Setup
            const double value = 7;
            domainData.EndDir = value;

            // Call
            double resultedValue = viewModel.EndDirection;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [TestCase(WaveDirectionalSpaceType.Circle, DirectionalSpaceType.Circle)]
        [TestCase(WaveDirectionalSpaceType.Sector, DirectionalSpaceType.Sector)]
        public void GetType_ReturnsCorrectValue(WaveDirectionalSpaceType fileType,
                                                DirectionalSpaceType expectedInputType)
        {
            // Setup
            domainData.DirectionalSpaceType = fileType;

            // Call
            DirectionalSpaceType resultedInputType = viewModel.Type;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedInputType));
        }

        [TestCase(DirectionalSpaceType.Sector, DirectionalSpaceType.Sector, WaveDirectionalSpaceType.Sector, 0)]
        [TestCase(DirectionalSpaceType.Sector, DirectionalSpaceType.Circle, WaveDirectionalSpaceType.Circle, 1)]
        [TestCase(DirectionalSpaceType.Circle, DirectionalSpaceType.Circle, WaveDirectionalSpaceType.Circle, 0)]
        [TestCase(DirectionalSpaceType.Circle, DirectionalSpaceType.Sector, WaveDirectionalSpaceType.Sector, 1)]
        public void SetType_SetsCorrectPropertyValueOnModel(DirectionalSpaceType originalValue,
                                                            DirectionalSpaceType setValue,
                                                            WaveDirectionalSpaceType expectedFileType,
                                                            int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.Type = originalValue;

            // Call
            void Call() => viewModel.Type = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.Type));
            Assert.That(domainData.DirectionalSpaceType, Is.EqualTo(expectedFileType));
        }

        [TestCase(7, 0)]
        [TestCase(5, 1)]
        public void SetNrOfDirections_SetsCorrectPropertyValueOnModel(
            int setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.NrOfDirections = 7;

            // Call
            void Call() => viewModel.NrOfDirections = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.NrOfDirections));
            Assert.That(domainData.NDir, Is.EqualTo(setValue));
        }

        [TestCase(7.5, 0)]
        [TestCase(5.5, 1)]
        public void SetStartDirection_SetsCorrectPropertyValueOnModel(double setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.StartDirection = 7.5;

            // Call
            void Call() => viewModel.StartDirection = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.StartDirection));
            Assert.That(domainData.StartDir, Is.EqualTo(setValue));
        }

        [TestCase(7.5, 0)]
        [TestCase(5.5, 1)]
        public void SetEndDirection_SetsCorrectPropertyValueOnModel(double setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.EndDirection = 7.5;

            // Call
            void Call() => viewModel.EndDirection = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.EndDirection));
            Assert.That(domainData.EndDir, Is.EqualTo(setValue));
        }
    }
}