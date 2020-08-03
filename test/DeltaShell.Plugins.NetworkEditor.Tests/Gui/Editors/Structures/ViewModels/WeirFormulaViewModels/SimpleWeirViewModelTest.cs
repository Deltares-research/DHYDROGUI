using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    [TestFixture]
    public class SimpleWeirViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var formula = new SimpleWeirFormula { LateralContraction = 20.0 };
            
            var weir = new Weir2D { WeirFormula = formula };

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                // Call
                var viewModel =
                    new SimpleWeirViewModel(formula, weirPropertiesViewModel);

                Assert.That(viewModel.WeirPropertiesViewModel, 
                            Is.SameAs(weirPropertiesViewModel));
                Assert.That(viewModel.ContractionCoefficient, 
                            Is.EqualTo(formula.LateralContraction));
            }
        }

        [Test]
        public void Constructor_FormulaNull_ThrowsArgumentNullException()
        {
            // Setup
            var weir = new Weir2D();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                // Call | Assert
                void Call() => new SimpleWeirViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<System.ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new SimpleWeirFormula();
            void Call() => new SimpleWeirViewModel(formula, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("weirPropertiesViewModel"));
        }

        [Test]
        public void ContractionCoefficient_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleWeirFormula() { LateralContraction=  6.0 };
            var weir2D = new Weir2D { WeirFormula = formula };

            const double contractionCoefficient = 20.0;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new SimpleWeirViewModel(formula,
                                                        weirPropertiesViewModel);

                // Call
                viewModel.ContractionCoefficient = contractionCoefficient;

                // Assert
                Assert.That(viewModel.ContractionCoefficient, 
                            Is.EqualTo(contractionCoefficient));
                Assert.That(formula.LateralContraction, 
                            Is.EqualTo(contractionCoefficient));
            }
        }
    }
}