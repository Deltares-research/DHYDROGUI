using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView.WeirFormulaViews
{
    [TestFixture]
    public class RiverWeirFormulaViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowRiverWeirFormulaView()
        {
            var riverWeirFormula = new RiverWeirFormula();
            var riverWeirFormulaView = new RiverWeirFormulaView
                                           {
                                               Data = riverWeirFormula
                                           };

            WindowsFormsTestHelper.ShowModal(riverWeirFormulaView);
        }
    }
}