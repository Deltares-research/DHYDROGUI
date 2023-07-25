using System;
using System.Collections.Generic;
using System.ComponentModel;
using AutoFixture;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    [TestFixture]
    public class GeneralStructureFormulaViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Setup
            var fixture = new Fixture();

            GeneralStructureFormula formula = 
                fixture.Build<GeneralStructureFormula>()
                       .With(f => f.HorizontalGateOpeningWidthTimeSeries, new TimeSeries())
                       .With(f => f.GateLowerEdgeLevelTimeSeries, new TimeSeries())
                       .Create();
            var weir = new Structure { Formula = formula };

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                // Call
                var viewModel = 
                    new GeneralStructureFormulaViewModel(formula, 
                                                  weirPropertiesViewModel);

                // Assert
                Assert.That(viewModel.GatePropertiesViewModel, Is.Not.Null);
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.StructurePropertiesViewModel, Is.SameAs(weirPropertiesViewModel));

                Assert.That(viewModel.Upstream1Width, Is.EqualTo(formula.Upstream1Width));
                Assert.That(viewModel.Upstream1Level, Is.EqualTo(formula.Upstream1Level));

                Assert.That(viewModel.Upstream2Width, Is.EqualTo(formula.Upstream2Width));
                Assert.That(viewModel.Upstream2Level, Is.EqualTo(formula.Upstream2Level));

                Assert.That(viewModel.Downstream1Width, Is.EqualTo(formula.Downstream1Width));
                Assert.That(viewModel.Downstream1Level, Is.EqualTo(formula.Downstream1Level));

                Assert.That(viewModel.Downstream2Width, Is.EqualTo(formula.Downstream2Width));
                Assert.That(viewModel.Downstream2Level, Is.EqualTo(formula.Downstream2Level));

                Assert.That(viewModel.FreeGateFlowPositive, Is.EqualTo(formula.PositiveFreeGateFlow));
                Assert.That(viewModel.FreeGateFlowNegative, Is.EqualTo(formula.NegativeFreeGateFlow));

                Assert.That(viewModel.DrownedGateFlowPositive, Is.EqualTo(formula.PositiveDrownedGateFlow));
                Assert.That(viewModel.DrownedGateFlowNegative, Is.EqualTo(formula.NegativeDrownedGateFlow));

                Assert.That(viewModel.FreeWeirFlowPositive, Is.EqualTo(formula.PositiveFreeWeirFlow));
                Assert.That(viewModel.FreeWeirFlowNegative, Is.EqualTo(formula.NegativeFreeWeirFlow));

                Assert.That(viewModel.DrownedWeirFlowPositive, Is.EqualTo(formula.PositiveDrownedWeirFlow));
                Assert.That(viewModel.DrownedWeirFlowNegative, Is.EqualTo(formula.NegativeDrownedWeirFlow));

                Assert.That(viewModel.ContractionCoefficientPositive, Is.EqualTo(formula.PositiveContractionCoefficient));
                Assert.That(viewModel.ContractionCoefficientNegative, Is.EqualTo(formula.NegativeContractionCoefficient));

                Assert.That(viewModel.ExtraResistance, Is.EqualTo(formula.ExtraResistance));
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
                void Call() => new GeneralStructureFormulaViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<System.ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            void Call() => new GeneralStructureFormulaViewModel(formula, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("structurePropertiesViewModel"));
        }

        private static IEnumerable<TestCaseData> GetPropertySetData()
        {
            // Stream fields
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Upstream1Width = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Upstream1Width ?? double.NaN),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Upstream1Width),
                                          nameof(GeneralStructureFormulaViewModel.Upstream1Width));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Upstream2Width = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Upstream2Width ?? double.NaN),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Upstream2Width),
                                          nameof(GeneralStructureFormulaViewModel.Upstream2Width));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Upstream1Level= v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Upstream1Level),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Upstream1Level),
                                          nameof(GeneralStructureFormulaViewModel.Upstream1Level));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Upstream2Level = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Upstream2Level),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Upstream2Level),
                                          nameof(GeneralStructureFormulaViewModel.Upstream2Level));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Downstream1Width = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Downstream1Width ?? double.NaN),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Downstream1Width),
                                          nameof(GeneralStructureFormulaViewModel.Downstream1Width));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Downstream2Width = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Downstream2Width ?? double.NaN),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Downstream2Width),
                                          nameof(GeneralStructureFormulaViewModel.Downstream2Width));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Downstream1Level= v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Downstream1Level),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Downstream1Level),
                                          nameof(GeneralStructureFormulaViewModel.Downstream1Level));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.Downstream2Level = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.Downstream2Level),
                                          new Func<GeneralStructureFormula, double>((f)    => f.Downstream2Level),
                                          nameof(GeneralStructureFormulaViewModel.Downstream2Level));

            // Coefficients
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.FreeGateFlowPositive = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.FreeGateFlowPositive),
                                          new Func<GeneralStructureFormula, double>((f)    => f.PositiveFreeGateFlow),
                                          nameof(GeneralStructureFormulaViewModel.FreeGateFlowPositive));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.FreeGateFlowNegative = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.FreeGateFlowNegative),
                                          new Func<GeneralStructureFormula, double>((f)    => f.NegativeFreeGateFlow),
                                          nameof(GeneralStructureFormulaViewModel.FreeGateFlowNegative));

            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.DrownedGateFlowPositive = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.DrownedGateFlowPositive),
                                          new Func<GeneralStructureFormula, double>((f)    => f.PositiveDrownedGateFlow),
                                          nameof(GeneralStructureFormulaViewModel.DrownedGateFlowPositive));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.DrownedGateFlowNegative = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.DrownedGateFlowNegative),
                                          new Func<GeneralStructureFormula, double>((f)    => f.NegativeDrownedGateFlow),
                                          nameof(GeneralStructureFormulaViewModel.DrownedGateFlowNegative));

            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.FreeWeirFlowPositive = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.FreeWeirFlowPositive),
                                          new Func<GeneralStructureFormula, double>((f)    => f.PositiveFreeWeirFlow),
                                          nameof(GeneralStructureFormulaViewModel.FreeWeirFlowPositive));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.FreeWeirFlowNegative = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.FreeWeirFlowNegative),
                                          new Func<GeneralStructureFormula, double>((f)    => f.NegativeFreeWeirFlow),
                                          nameof(GeneralStructureFormulaViewModel.FreeWeirFlowNegative));

            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.DrownedWeirFlowPositive = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.DrownedWeirFlowPositive),
                                          new Func<GeneralStructureFormula, double>((f)    => f.PositiveDrownedWeirFlow),
                                          nameof(GeneralStructureFormulaViewModel.DrownedWeirFlowPositive));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.DrownedWeirFlowNegative = v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.DrownedWeirFlowNegative),
                                          new Func<GeneralStructureFormula, double>((f)    => f.NegativeDrownedWeirFlow),
                                          nameof(GeneralStructureFormulaViewModel.DrownedWeirFlowNegative));

            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.ContractionCoefficientPositive= v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.ContractionCoefficientPositive),
                                          new Func<GeneralStructureFormula, double>((f)    => f.PositiveContractionCoefficient),
                                          nameof(GeneralStructureFormulaViewModel.ContractionCoefficientPositive));
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.ContractionCoefficientNegative= v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.ContractionCoefficientNegative),
                                          new Func<GeneralStructureFormula, double>((f)    => f.NegativeContractionCoefficient),
                                          nameof(GeneralStructureFormulaViewModel.ContractionCoefficientNegative));

            // Extra Resistance
            yield return new TestCaseData(new Action<GeneralStructureFormulaViewModel, double>((f, v) => f.ExtraResistance= v), 
                                          new Func<GeneralStructureFormulaViewModel,   double>((f)    => f.ExtraResistance),
                                          new Func<GeneralStructureFormula, double>((f)    => f.ExtraResistance),
                                          nameof(GeneralStructureFormulaViewModel.ExtraResistance));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertySetData))]
        public void Property_SetIsPropagatedCorrectly(Action<GeneralStructureFormulaViewModel, double> setPropertyViewModel,
                                                      Func<GeneralStructureFormulaViewModel, double> getPropertyViewModel,
                                                      Func<GeneralStructureFormula, double> getPropertyFormula,
                                                      string propertyName)
        {
            // Setup
            var fixture = new Fixture();

            var value = fixture.Create<double>();
            GeneralStructureFormula formula = 
                fixture.Build<GeneralStructureFormula>()
                       .With(f => f.HorizontalGateOpeningWidthTimeSeries, new TimeSeries())
                       .With(f => f.GateLowerEdgeLevelTimeSeries, new TimeSeries())
                       .Create();
            var weir = new Structure { Formula = formula };
            
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                var viewModel = new GeneralStructureFormulaViewModel(formula, weirPropertiesViewModel);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                setPropertyViewModel.Invoke(viewModel, value);

                // Assert
                Assert.That(getPropertyViewModel.Invoke(viewModel), Is.EqualTo(value));
                Assert.That(getPropertyFormula.Invoke(formula), Is.EqualTo(value));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName, Is.EqualTo(propertyName));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        [Test]
        [TestCaseSource(nameof(GetPropertySetData))]
        public void Property_SameValue_DoesNotFirePropertyChanged(Action<GeneralStructureFormulaViewModel, double> setPropertyViewModel,
                                                                  Func<GeneralStructureFormulaViewModel, double> getPropertyViewModel,
                                                                  Func<GeneralStructureFormula, double> getPropertyFormula,
                                                                  string _)
        {
            // Setup
            var fixture = new Fixture();

            var value = fixture.Create<double>();
            GeneralStructureFormula formula = 
                fixture.Build<GeneralStructureFormula>()
                       .With(f => f.HorizontalGateOpeningWidthTimeSeries, new TimeSeries())
                       .With(f => f.GateLowerEdgeLevelTimeSeries, new TimeSeries())
                       .Create();
            var weir = new Structure { Formula = formula };
            
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir))
            {
                var viewModel = new GeneralStructureFormulaViewModel(formula, weirPropertiesViewModel);
                setPropertyViewModel.Invoke(viewModel, value);

                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                setPropertyViewModel.Invoke(viewModel, value);

                // Assert
                Assert.That(getPropertyViewModel.Invoke(viewModel), Is.EqualTo(value));
                Assert.That(getPropertyFormula.Invoke(formula), Is.EqualTo(value));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }
    }
}