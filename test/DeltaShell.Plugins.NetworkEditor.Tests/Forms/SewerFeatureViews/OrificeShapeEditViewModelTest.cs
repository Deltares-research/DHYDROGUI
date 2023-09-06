using System.Windows;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class OrificeShapeEditViewModelTest
    {
        [Test]
        public void Constructor_ArgumentNull_ThrowsException()
        {
            // Call
            void Call() => new OrificeShapeEditViewModel(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetGateLowerEdgeLevel_OrificeHasGatedWeirFormula_ReturnsExpectedValue()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new GatedWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            const double expectedValue = 123;
            viewModel.GateLowerEdgeLevel = expectedValue;

            // Call
            double gateLowerEdgeLevel = viewModel.GateLowerEdgeLevel;

            // Assert
            Assert.That(gateLowerEdgeLevel, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetGateLowerEdgeLevel_NotAGatedWeirFormula_ReturnsNaN()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new SimpleWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            double gateLowerEdgeLevel = viewModel.GateLowerEdgeLevel;

            // Assert
            Assert.That(gateLowerEdgeLevel, Is.NaN);
        }

        [Test]
        public void SettingGateLowerEdgeLevel_OrificeHasGatedWeirFormula_SetsCorrectValue()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new GatedWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            const double newValue = 123;
            viewModel.GateLowerEdgeLevel = newValue;

            // Assert
            Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(newValue));
        }

        [Test]
        public void GetContractionCoefficient_OrificeHasGatedWeirFormula_ReturnsExpectedValue()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new GatedWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            const double expectedValue = 123;
            viewModel.ContractionCoefficient = expectedValue;

            // Call
            double contractionCoefficient = viewModel.ContractionCoefficient;

            // Assert
            Assert.That(contractionCoefficient, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetContractionCoefficient_NotAGatedWeirFormula_ReturnsNaN()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new SimpleWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            double contractionCoefficient = viewModel.ContractionCoefficient;

            // Assert
            Assert.That(contractionCoefficient, Is.NaN);
        }

        [Test]
        public void SettingContractionCoefficient_OrificeHasGatedWeirFormula_SetsCorrectValue()
        {
            // Setup
            var orifice = new Orifice { WeirFormula = new GatedWeirFormula() };
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            const double newValue = 123;
            viewModel.GateLowerEdgeLevel = newValue;

            // Assert
            Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(newValue));
        }

        [Test]
        public void IsEnabled_GatedWeirFormula_ReturnsTrue()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var weirFormula = Substitute.For<IGatedWeirFormula>();
            orifice.WeirFormula.Returns(weirFormula);

            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            bool isEnabled = viewModel.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_NotAGatedWeirFormula_ReturnsFalse()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var weirFormula = Substitute.For<IWeirFormula>();
            orifice.WeirFormula.Returns(weirFormula);

            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            bool isEnabled = viewModel.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsVisible_GatedWeirFormula_ReturnsVisible()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var weirFormula = Substitute.For<IGatedWeirFormula>();
            orifice.WeirFormula.Returns(weirFormula);

            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            Visibility isVisible = viewModel.IsVisible;

            // Assert
            Assert.That(isVisible, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void IsVisible_NotAGatedWeirFormula_ReturnsHidden()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var weirFormula = Substitute.For<IWeirFormula>();
            orifice.WeirFormula.Returns(weirFormula);

            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            Visibility isVisible = viewModel.IsVisible;

            // Assert
            Assert.That(isVisible, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void SetCrestLevel_SetsOrificeProperty()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            viewModel.CrestLevel = 1.23;

            // Assert
            Assert.That(orifice.CrestLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCrestLevel_GetsOrificeProperty()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            orifice.CrestLevel = 2.34;
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            double result = viewModel.CrestLevel;

            // Assert
            Assert.That(result, Is.EqualTo(2.34));
        }

        [Test]
        public void SetCrestWidth_SetsOrificeProperty()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            viewModel.CrestWidth = 1.23;

            // Assert
            Assert.That(orifice.CrestWidth, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCrestWidth_GetsOrificeProperty()
        {
            // Setup
            var orifice = Substitute.For<IOrifice>();
            orifice.CrestWidth = 2.34;
            var viewModel = new OrificeShapeEditViewModel(orifice);

            // Call
            double result = viewModel.CrestWidth;

            // Assert
            Assert.That(result, Is.EqualTo(2.34));
        }
    }
}