using System;
using System.ComponentModel;
using AutoFixture;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    [TestFixture]
    public class SimpleWeirFormulaViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var formula = new SimpleWeirFormula {LateralContraction = 20.0};

            var weir = new Structure {Formula = formula};

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                // Call
                var viewModel =
                    new SimpleWeirFormulaViewModel(formula, weirPropertiesViewModel);

                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.StructurePropertiesViewModel,
                            Is.SameAs(weirPropertiesViewModel));
                Assert.That(viewModel.ContractionCoefficient,
                            Is.EqualTo(formula.LateralContraction));
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
                void Call() => new SimpleWeirFormulaViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new SimpleWeirFormula();
            void Call() => new SimpleWeirFormulaViewModel(formula, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("structurePropertiesViewModel"));
        }

        [Test]
        public void ContractionCoefficient_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleWeirFormula() {LateralContraction = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double contractionCoefficient = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new SimpleWeirFormulaViewModel(formula,
                                                        weirPropertiesViewModel);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.ContractionCoefficient = contractionCoefficient;

                // Assert
                Assert.That(viewModel.ContractionCoefficient,
                            Is.EqualTo(contractionCoefficient));
                Assert.That(formula.LateralContraction,
                            Is.EqualTo(contractionCoefficient));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1),
                            "Expected a single property changed event to be fired.");
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.ContractionCoefficient)));

                // Cleanup
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        [Test]
        public void ContractionCoefficient_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var fixture = new Fixture();
            var lateralContraction = fixture.Create<double>();

            var formula = new SimpleWeirFormula() {LateralContraction = lateralContraction};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new SimpleWeirFormulaViewModel(formula,
                                                        weirPropertiesViewModel);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.ContractionCoefficient = lateralContraction;

                // Assert
                Assert.That(viewModel.ContractionCoefficient,
                            Is.EqualTo(lateralContraction));
                Assert.That(formula.LateralContraction,
                            Is.EqualTo(lateralContraction));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0),
                            "Expected a no property changed event to be fired.");

                // Cleanup
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }
    }
}