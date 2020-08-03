using System;
using System.Collections.Generic;
using AutoFixture;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    [TestFixture]
    public class GeneralStructureViewModelTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Setup
            var fixture = new Fixture();

            GeneralStructureWeirFormula formula = 
                fixture.Build<GeneralStructureWeirFormula>()
                       .With(f => f.HorizontalDoorOpeningWidthTimeSeries, new TimeSeries())
                       .With(f => f.LowerEdgeLevelTimeSeries, new TimeSeries())
                       .Create();
            var weir = new Weir2D { WeirFormula = formula };

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                // Call
                var viewModel = 
                    new GeneralStructureViewModel(formula, 
                                                  weirPropertiesViewModel);

                // Assert
                Assert.That(viewModel.GatePropertiesViewModel, Is.Not.Null);
                Assert.That(viewModel.WeirPropertiesViewModel, Is.SameAs(weirPropertiesViewModel));

                Assert.That(viewModel.Upstream1Width, Is.EqualTo(formula.WidthStructureLeftSide));
                Assert.That(viewModel.Upstream1Level, Is.EqualTo(formula.BedLevelLeftSideStructure));

                Assert.That(viewModel.Upstream2Width, Is.EqualTo(formula.WidthLeftSideOfStructure));
                Assert.That(viewModel.Upstream2Level, Is.EqualTo(formula.BedLevelLeftSideOfStructure));

                Assert.That(viewModel.Downstream1Width, Is.EqualTo(formula.WidthStructureRightSide));
                Assert.That(viewModel.Downstream1Level, Is.EqualTo(formula.BedLevelRightSideStructure));

                Assert.That(viewModel.Downstream2Width, Is.EqualTo(formula.WidthRightSideOfStructure));
                Assert.That(viewModel.Downstream2Level, Is.EqualTo(formula.BedLevelRightSideOfStructure));

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
            var weir = new Weir2D();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                // Call | Assert
                void Call() => new GeneralStructureViewModel(null, weirPropertiesViewModel);

                var exception = Assert.Throws<System.ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula();
            void Call() => new GeneralStructureViewModel(formula, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("weirPropertiesViewModel"));
        }

        private IEnumerable<TestCaseData> GetPropertySetData()
        {
            // Stream fields
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Upstream1Width = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Upstream1Width ?? double.NaN),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.WidthStructureLeftSide));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Upstream2Width = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Upstream2Width ?? double.NaN),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.WidthLeftSideOfStructure));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Upstream1Level= v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Upstream1Level),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.BedLevelLeftSideStructure));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Upstream2Level = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Upstream2Level),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.BedLevelLeftSideOfStructure));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Downstream1Width = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Downstream1Width ?? double.NaN),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.WidthStructureRightSide));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Downstream2Width = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Downstream2Width ?? double.NaN),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.WidthRightSideOfStructure));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Downstream1Level= v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Downstream1Level),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.BedLevelRightSideStructure));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.Downstream2Level = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.Downstream2Level),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.BedLevelRightSideOfStructure));

            // Coefficients
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.FreeGateFlowPositive = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.FreeGateFlowPositive),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.PositiveFreeGateFlow));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.FreeGateFlowNegative = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.FreeGateFlowNegative),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.NegativeFreeGateFlow));

            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.DrownedGateFlowPositive = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.DrownedGateFlowPositive),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.PositiveDrownedGateFlow));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.DrownedGateFlowNegative = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.DrownedGateFlowNegative),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.NegativeDrownedGateFlow));

            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.FreeWeirFlowPositive = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.FreeWeirFlowPositive),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.PositiveFreeWeirFlow));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.FreeWeirFlowNegative = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.FreeWeirFlowNegative),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.NegativeFreeWeirFlow));

            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.DrownedWeirFlowPositive = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.DrownedWeirFlowPositive),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.PositiveDrownedWeirFlow));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.DrownedWeirFlowNegative = v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.DrownedWeirFlowNegative),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.NegativeDrownedWeirFlow));

            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.ContractionCoefficientPositive= v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.ContractionCoefficientPositive),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.PositiveContractionCoefficient));
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.ContractionCoefficientNegative= v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.ContractionCoefficientNegative),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.NegativeContractionCoefficient));

            // Extra Resistance
            yield return new TestCaseData(new Action<GeneralStructureViewModel, double>((f, v) => f.ExtraResistance= v), 
                                          new Func<GeneralStructureViewModel,   double>((f)    => f.ExtraResistance),
                                          new Func<GeneralStructureWeirFormula, double>((f)    => f.ExtraResistance));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertySetData))]
        public void Property_SetIsPropagatedCorrectly(Action<GeneralStructureViewModel, double> setPropertyViewModel,
                                                      Func<GeneralStructureViewModel, double> getPropertyViewModel,
                                                      Func<GeneralStructureWeirFormula, double> getPropertyFormula)
        {
            // Setup
            var fixture = new Fixture();

            var value = fixture.Create<double>();
            GeneralStructureWeirFormula formula = 
                fixture.Build<GeneralStructureWeirFormula>()
                       .With(f => f.HorizontalDoorOpeningWidthTimeSeries, new TimeSeries())
                       .With(f => f.LowerEdgeLevelTimeSeries, new TimeSeries())
                       .Create();
            var weir = new Weir2D { WeirFormula = formula };

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir))
            {
                var viewModel = new GeneralStructureViewModel(formula, weirPropertiesViewModel);

                // Call
                setPropertyViewModel.Invoke(viewModel, value);

                // Assert
                Assert.That(getPropertyViewModel.Invoke(viewModel), Is.EqualTo(value));
                Assert.That(getPropertyFormula.Invoke(formula), Is.EqualTo(value));
            }
        }
    }
}