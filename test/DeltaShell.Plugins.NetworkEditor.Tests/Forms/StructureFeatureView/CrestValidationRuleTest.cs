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
    }
}
