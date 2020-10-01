using System;
using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    [TestFixture]
    public class GatedWeirViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir = new Weir2D {WeirFormula = formula};

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                // Call
                var viewModel =
                    new GatedWeirViewModel(formula,
                                           weirPropertiesViewModel);

                // Assert
                Assert.That(viewModel.GatePropertiesViewModel, Is.Not.Null);
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.WeirPropertiesViewModel, Is.SameAs(weirPropertiesViewModel));
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
                void Call() => new GatedWeirViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            void Call() => new GatedWeirViewModel(formula, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("weirPropertiesViewModel"));
        }
    }
}