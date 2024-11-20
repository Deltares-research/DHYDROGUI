using System;
using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    [TestFixture]
    public class SimpleGateFormulaViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            var weir = new Structure() {Formula = formula};

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                // Call
                var viewModel =
                    new SimpleGateFormulaViewModel(formula,
                                           weirPropertiesViewModel);

                // Assert
                Assert.That(viewModel.GatePropertiesViewModel, Is.Not.Null);
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.StructurePropertiesViewModel, Is.SameAs(weirPropertiesViewModel));
            }
        }

        [Test]
        public void Constructor_FormulaNull_ThrowsArgumentNullException()
        {
            // Setup
            var weir = new Structure();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                // Call | Assert
                void Call() => new SimpleGateFormulaViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            void Call() => new SimpleGateFormulaViewModel(formula, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("structurePropertiesViewModel"));
        }
    }
}