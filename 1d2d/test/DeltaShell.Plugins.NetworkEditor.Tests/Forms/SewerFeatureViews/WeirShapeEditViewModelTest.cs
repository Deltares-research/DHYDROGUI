using System.Windows;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NSubstitute;
using NUnit.Framework;
using FlowDirection = DelftTools.Hydro.FlowDirection;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class WeirShapeEditViewModelTest
    {
        [Test]
        public void Constructor_ArgumentNull_ThrowsException()
        {
            // Call
            void Call() => new WeirShapeEditViewModel(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SettingCorrectionCoefficient_WeirHasSimpleWeirFormula_SetsCorrectValue()
        {
            // Setup
            var weir = new Weir();
            weir.WeirFormula = new SimpleWeirFormula();
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            const double newValue = 123;
            viewModel.CorrectionCoefficient = newValue;

            // Assert
            Assert.That(viewModel.CorrectionCoefficient, Is.EqualTo(newValue));
        }

        [Test]
        public void GetCorrectionCoefficient_WeirHasSimpleWeirFormula_ReturnsCorrectionCoefficient()
        {
            // Setup
            var weir = new Weir { WeirFormula = new SimpleWeirFormula() };

            const double value = 123;
            var viewModel = new WeirShapeEditViewModel(weir) { CorrectionCoefficient = value };

            // Call
            double correctionCoefficient = viewModel.CorrectionCoefficient = value;

            // Assert
            Assert.That(correctionCoefficient, Is.EqualTo(value));
        }

        [Test]
        public void GetCorrectionCoefficient_WeirDoesNotHaveSimpleWeirFormula_ReturnsNaN()
        {
            // Setup
            var weir = new Weir { WeirFormula = new SimpleWeirFormula() };

            const double value = 123;
            var viewModel = new WeirShapeEditViewModel(weir) { CorrectionCoefficient = value };

            weir.WeirFormula = new GatedWeirFormula();

            // Call
            double correctionCoefficient = viewModel.CorrectionCoefficient;

            // Assert
            Assert.That(correctionCoefficient, Is.NaN);
        }

        [Test]
        public void IsEnabled_SimpleWeirFormula_ReturnsTrue()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var weirFormula = new SimpleWeirFormula();
            weir.WeirFormula.Returns(weirFormula);

            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            bool isEnabled = viewModel.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.True);
        }

        [Test]
        public void IsEnabled_NotASimpleWeirFormula_ReturnsFalse()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var notASimpleWeir = Substitute.For<IGatedWeirFormula>();
            weir.WeirFormula.Returns(notASimpleWeir);

            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            bool isEnabled = viewModel.IsEnabled;

            // Assert
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsVisible_SimpleWeirFormula_ReturnsVisible()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var weirFormula = new SimpleWeirFormula();
            weir.WeirFormula.Returns(weirFormula);

            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            Visibility isVisible = viewModel.IsVisible;

            // Assert
            Assert.That(isVisible, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void IsVisible_NotASimpleWeirFormula_ReturnsHidden()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var notASimpleWeir = Substitute.For<IGatedWeirFormula>();
            weir.WeirFormula.Returns(notASimpleWeir);

            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            Visibility isVisible = viewModel.IsVisible;

            // Assert
            Assert.That(isVisible, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void SetCrestLevel_SetsWeirProperty()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            viewModel.CrestLevel = 1.23;

            // Assert
            Assert.That(weir.CrestLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCrestLevel_GetsWeirProperty()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            weir.CrestLevel = 2.34;
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            double result = viewModel.CrestLevel;

            // Assert
            Assert.That(result, Is.EqualTo(2.34));
        }

        [Test]
        public void SetCrestWidth_SetsWeirProperty()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            viewModel.CrestWidth = 1.23;

            // Assert
            Assert.That(weir.CrestWidth, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCrestWidth_GetsWeirProperty()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            weir.CrestWidth = 2.34;
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            double result = viewModel.CrestWidth;

            // Assert
            Assert.That(result, Is.EqualTo(2.34));
        }

        [Test]
        public void SetFlowDirection_SetsWeirProperty([Values] FlowDirection flowDirection)
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            viewModel.FlowDirection = flowDirection;

            // Assert
            Assert.That(weir.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GetFlowDirection_GetsWeirProperty([Values] FlowDirection flowDirection)
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            weir.FlowDirection = flowDirection;
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            FlowDirection result = viewModel.FlowDirection;

            // Assert
            Assert.That(result, Is.EqualTo(flowDirection));
        }
    }
}