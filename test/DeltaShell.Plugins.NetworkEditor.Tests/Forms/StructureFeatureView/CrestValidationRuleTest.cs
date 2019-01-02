using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class CrestValidationRuleTest
    {
        [Test]
        public void GivenACorrectWeirViewModel_WhenValidatingCrestProperties_ThenAValidResultIsReturned()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(), SelectedWeirType = SelectableWeirFormulaType.SimpleGate
            };

            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);
            viewModel.BedLevelStructureCentre = 10.0;

            var validationResult = new CrestValidationRule().Validate(viewModel.BedLevelStructureCentre, CultureInfo.CurrentCulture);
            Assert.IsTrue(validationResult.IsValid);
        }

        [Test]
        public void GivenAWeirViewModelWhereACrestPropertyOutsideDoubleBounds_WhenValidatingCrestProperties_ThenAnOverFlownExceptionIsThrown()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
                SelectedWeirType = SelectableWeirFormulaType.SimpleGate
            };

            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);

            var validationResult =
                new CrestValidationRule().Validate("1.79769313486232E+310", CultureInfo.CurrentCulture);
            Assert.IsFalse(validationResult.IsValid);
        }
        [Test]
        public void GivenAWeirViewModelWhereACrestPropertyOutsideAString_WhenValidatingCrestProperties_ThenAFormatExceptionIsThrown()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
                SelectedWeirType = SelectableWeirFormulaType.SimpleGate
            };

            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);

            var validationResult =
                new CrestValidationRule().Validate("ThisIsNotAValidDouble", CultureInfo.CurrentCulture);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void GivenAWeirViewModelWhereACrestPropertyIsAnObject_WhenValidatingCrestProperties_ThenAnInvalidCastExceptionIsThrown()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
                SelectedWeirType = SelectableWeirFormulaType.SimpleGate
            };

            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);

            var validationResult =
                new CrestValidationRule().Validate(new object(), CultureInfo.CurrentCulture);
            Assert.IsFalse(validationResult.IsValid);
        }
    }
}
